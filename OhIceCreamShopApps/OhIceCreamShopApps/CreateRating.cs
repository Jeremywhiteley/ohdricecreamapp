using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using OhIceCreamShopApps.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class CreateRating
    {
        private static readonly HttpClient ApiHttpClient = new HttpClient();

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            string requestBody;

            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var rating = JsonConvert.DeserializeObject<Rating>(requestBody);

            log.Info($"Create rating request received for product \"{rating.ProductId}\" from user \"{rating.UserId}\".");
            log.Info($"Testing commit to staging");

            var getProductTask = FetchProductAsync(rating.ProductId);
            var getUserTask = FetchUserAsync(rating.UserId);

            await Task.WhenAll(
                getProductTask,
                getUserTask);

            var product = getProductTask.Result;
            var user = getUserTask.Result;

            if (product != null && user != null)
            {
                var createdRating = await CreateRatingAsync(rating);
                return new OkObjectResult(createdRating);
            }

            return new BadRequestObjectResult($"User {rating.UserId} and Product {rating.ProductId} not found.");
        }

        private static async Task<UserInfo> FetchUserAsync(string userId)
        {
            using (var response = await ApiHttpClient.GetAsync($"https://serverlessohlondonuser.azurewebsites.net/api/GetUser?userId={userId}"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return default(UserInfo);
                }

                return await response.Content.ReadAsAsync<UserInfo>();
            }
        }

        private static async Task<ProductInfo> FetchProductAsync(string productId)
        {
            using (var response = await ApiHttpClient.GetAsync($"https://serverlessohlondonproduct.azurewebsites.net/api/GetProduct?productId={productId}"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return default(ProductInfo);
                }

                return await response.Content.ReadAsAsync<ProductInfo>();
            }
        }

        private static async Task<Rating> CreateRatingAsync(Rating rating)
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

            var document = await documentClient.CreateDocumentAsync(documentCollectionLink, rating);

            return (Rating)((dynamic)document.Resource);
        }
    }
}
