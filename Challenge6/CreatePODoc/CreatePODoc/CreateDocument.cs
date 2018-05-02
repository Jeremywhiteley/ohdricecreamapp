
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using CsvHelper;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace CreatePODoc
{
    public static class CreateDocument
    {
        static CloudStorageAccount storageAccount = null;

        [FunctionName("CreateDocument")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            string[] fileList = JsonConvert.DeserializeObject<string[]>(requestBody);

            // create the document client
            var documentClient = new DocumentClient(new Uri("https://ohdrteamdb-001.documents.azure.com:443/"), "1ZHhCMRJGECdqFajFLVIYYhSQG5OIsQSo0OjFacj9b9ZQeYvKZ5nDzXwXYpBomEmgpht9v5EAnNcYDYkF4TWuA==");
            var databaseResponse = documentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = "ChallengeDatabase" }).Result;
            var database = databaseResponse.Resource as Database;
            var databaseLink = UriFactory.CreateDatabaseUri(database.Id);
            var documentCollectionResponse = documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, new DocumentCollection() { Id = "OrderHeaderDetails" }).Result;
            var documentCollection = documentCollectionResponse.Resource as DocumentCollection;
            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri(database.Id, documentCollection.Id);

            // load the blobs
            if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("connectionstring"), out storageAccount))
            {

                // get contents of header file
                var headerCSVReader = GetCsvReader(fileList[0]);
                headerCSVReader.Configuration.RegisterClassMap<OrderHeaderDetailsMap>();
                var headerRecords = headerCSVReader.GetRecords<OrderHeaderDetails>();

                // get contents of details file
                var detailsCSVReader = GetCsvReader(fileList[1]);
                detailsCSVReader.Configuration.RegisterClassMap<OrderLineItemsMap>();
                var orderlineItems = detailsCSVReader.GetRecords<OrderLineItems>();

                // get contents of product
                var productCSVReader = GetCsvReader(fileList[2]);
                productCSVReader.Configuration.RegisterClassMap<ProductInformationMap>();
                var productRecords = productCSVReader.GetRecords<ProductInformation>();

                List<OrderHeaderDetails> myOrder = new List<OrderHeaderDetails>();

                foreach (var headerRecord in headerRecords)
                {
                    myOrder.Add(headerRecord);
                    foreach(var lineRecord in orderlineItems)
                    {
                        if (headerRecord.poNumber == lineRecord.poNumber)
                        {
                            headerRecord.lineitems.Add(lineRecord);
                            foreach (var productRecord in productRecords)
                            {
                                if (productRecord.productid == lineRecord.productid)
                                {
                                    lineRecord.productname = productRecord.productname;
                                    lineRecord.productdescription = productRecord.productdescription;
                                }
                            }
                        }
                    }
                }

                // write order to docDB
                documentClient.CreateDocumentAsync(documentCollectionLink, new { Order = myOrder }).Wait();
            }

            // write to CosmosDB

            return new OkResult();
        }

        private static CsvReader GetCsvReader(string blobURL)
        {
            CloudBlob tmpBlob = new CloudBlob(new Uri(blobURL));

            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(tmpBlob.Container.Name);
            CloudBlob headerBlob = cloudBlobContainer.GetBlobReference(tmpBlob.Name);
            Stream headerStream = new MemoryStream();
            headerBlob.DownloadToStreamAsync(headerStream).Wait();
            StreamReader headerReader = new StreamReader(headerStream);
            headerStream.Seek(0, SeekOrigin.Begin);

            return new CsvReader(headerReader);
        }
    }
}
