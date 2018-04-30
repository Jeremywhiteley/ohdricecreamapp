using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using OhIceCreamShopApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            string userId = req.Query["userId"];

            if (String.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult($"UserId is missing from query parameters");
            }

            var doc = await GetDocumentsAsync(userId);
            return new OkObjectResult(doc);
        }

        private static async Task<IEnumerable<Rating>> GetDocumentsAsync(string userId)
        {
            var uri = new Uri("https://ohdrteamdb-001.documents.azure.com:443/");
            var secret = "";
            var documentClient = new DocumentClient(uri, secret);

            var database = new Database() { Id = "IceCreamApp" };
            var databaseItem = await documentClient.CreateDatabaseIfNotExistsAsync(database);
            var databaseLink = UriFactory.CreateDatabaseUri(database.Id);

            var documentCollection = new DocumentCollection() { Id = "Ratings" };
            var documentCollectionItem = await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, documentCollection);
            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri(database.Id, documentCollection.Id);

            var ratingsQuery = documentClient.CreateDocumentQuery<Rating>(documentCollectionLink)
                .Where(r => r.UserId == userId)
                .AsDocumentQuery();

            var ratings = new List<Rating>();

            while (ratingsQuery.HasMoreResults)
            {
                var results = await ratingsQuery.ExecuteNextAsync<Rating>();
                ratings.AddRange(results);
            }

            return ratings;
        }
    }
}