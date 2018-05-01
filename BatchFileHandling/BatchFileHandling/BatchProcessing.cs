
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
using Newtonsoft.Json.Linq;

namespace BatchFileHandling
{
    public static class BatchProcessing
    {
        [FunctionName("BatchProcessing")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var nextFileUrl = await ParseNextFileUrlFromRequest(req);

            log.Info($"Received blob path of: {nextFileUrl}");

            string dateId = ParseDateIdFromFileUrl(nextFileUrl);

            var cloudBlobClient = GetCloudBlobClient();

            const string containerReference = "blob-lock";

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerReference);
            await cloudBlobContainer.CreateIfNotExistsAsync();

            var fileName = $"lock-{dateId}-txt";
            var blob = cloudBlobContainer.GetBlockBlobReference(fileName);
            if (!await blob.ExistsAsync())
            {
                await blob.UploadTextAsync("");
            }

            var fileUrls = new List<string>();

            var blobLeaseId = await blob.AcquireLeaseAsync(TimeSpan.FromSeconds(60));
            AccessCondition accessCondition = new AccessCondition() { LeaseId = blobLeaseId };
            BlobRequestOptions options = new BlobRequestOptions();
            OperationContext operationContext = new OperationContext();
            try
            {
                try
                {
                    using (var reader = new StreamReader(await blob.OpenReadAsync()))
                    {
                        string fileUrl;

                        while ((fileUrl = await reader.ReadLineAsync()) != null)
                        {
                            if (!String.IsNullOrWhiteSpace(fileUrl))
                            {
                                fileUrls.Add(fileUrl);
                            }
                        }
                    }
                }
                catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 404)
                {
                    log.Warning($"Blob for {dateId} did not exist yet...");
                }

                log.Info($"Blob contained {fileUrls.Count} file urls already...");

                fileUrls.Add(nextFileUrl);

                using (var writer = new StreamWriter(await blob.OpenWriteAsync(accessCondition, options, operationContext)))
                {
                    foreach (string fileUrl in fileUrls)
                    {
                        await writer.WriteLineAsync(fileUrl);
                    }
                }

                if (fileUrls.Count < 3)
                {
                    log.Info($"Only {fileUrls.Count} so far, returning accepted.");

                    return new AcceptedResult();
                }

                log.Info($"We hae {fileUrls.Count} files, returning them to caller!");


                return new OkObjectResult(fileUrls);
            }
            finally
            {
                log.Info($"Releasing lease \"{blobLeaseId}\" on blob {blob.Name}...");

                await blob.ReleaseLeaseAsync(new AccessCondition { LeaseId = blobLeaseId });

                log.Info($"Lease \"{blobLeaseId}\" on blob {blob.Name} released!");

            }
        }

        private static CloudBlobClient GetCloudBlobClient()
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");


            // Check whether the connection string can be parsed.
            if (!CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {
                throw new Exception("No connection string configured for blob storage");
            }
            // If the connection string is valid, proceed with operations against Blob storage here.

            // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
            return storageAccount.CreateCloudBlobClient();
        }

        private static async Task<string> ParseNextFileUrlFromRequest(HttpRequest req)
        {
            string nextFileUrl;

            using (var jsonReader = new JsonTextReader(new StreamReader(req.Body)))
            {
                var input = await JObject.ReadFromAsync(jsonReader);

                nextFileUrl = input["blobPath"].Value<string>();
            }

            return nextFileUrl;
        }
		

        private static string ParseDateIdFromFileUrl(string nextFileUrl)
        {
            Uri fileUri = new Uri(nextFileUrl);
            var dateId = fileUri.Segments[2].Split('-');
            return dateId[0];
        }
    }
}
