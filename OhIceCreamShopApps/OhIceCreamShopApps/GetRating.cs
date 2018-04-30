using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using OhIceCreamShopApps.Models;
using System;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            string ratingId = req.Query["ratingId"];

            if (String.IsNullOrEmpty(ratingId))
            {
                return new BadRequestObjectResult($"RatingId is missing from query parameters");
            }

            var rating = await GetDocumentAsync(ratingId);
            return new OkObjectResult(rating);
        }

        private static async Task<Rating> GetDocumentAsync(string ratingId)
        {
            var documentClientDetails = await DocumentDbClientFactory.GetDocumentClientAsync();
            var docUri = UriFactory.CreateDocumentUri("IceCreamApp", "Ratings", ratingId);

            return await documentClientDetails.DocumentClient.ReadDocumentAsync<Rating>(docUri);
        }
    }
}