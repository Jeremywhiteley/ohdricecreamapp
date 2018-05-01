
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BatchFileHandling
{
    public static class BatchProcessing
    {
        [FunctionName("BatchProcessing")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //StorageItem eventData = JsonConvert.DeserializeObject<EventGridEvent>(req);
            using (StreamReader sr = new StreamReader(req.Body))
            {
                var blobURL = await sr.ReadToEndAsync();
                var containerReference = "blobLock";

                string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

                CloudStorageAccount storageAccount;

                // Check whether the connection string can be parsed.
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    // If the connection string is valid, proceed with operations against Blob storage here.

                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerReference);
                    CloudBlob blob = cloudBlobContainer.GetBlobReference(blobURL);

                    {
                        using (StreamReader reader = new StreamReader(await blob.OpenReadAsync()))
                        {
                            var files = new List<string>();
                            int countLines = 0;
                            var line = String.Empty;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                if (!String.IsNullOrWhiteSpace(line))
                                {
                                    countLines++;
                                    files.Add(line);
                                }
                                if (countLines==3)
                                {
                                    return new OkObjectResult(files);
                                }
                            }
                        }
                        using (StreamWriter write = new StreamWriter(await ))
                    }
                }
                else
                {
                    // Otherwise, let the user know that they need to define the environment variable.
                    Console.WriteLine(
                        "A connection string has not been defined in the system environment variables. " +
                        "Add a environment variable named 'storageconnectionstring' with your storage " +
                        "connection string as a value.");
                    Console.WriteLine("Press any key to exit the sample application.");
                    Console.ReadLine();
                }

                return new OkObjectResult(202);
                //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }
        }

    }
}
