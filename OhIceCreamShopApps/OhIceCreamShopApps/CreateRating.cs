using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using OhIceCreamShopApps.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class CreateRating
    {
        private static readonly HttpClient ApiHttpClient = new HttpClient();

        // Create Telemetry for Sentiment Monioring
        static string key = TelemetryConfiguration.Active.InstrumentationKey =
        System.Environment.GetEnvironmentVariable(
            "APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

        public static TelemetryClient telemetryClient =
            new TelemetryClient() { InstrumentationKey = key };

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            ExecutionContext context,
            TraceWriter log)
        {
            string requestBody;

            using (var streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var rating = JsonConvert.DeserializeObject<Rating>(requestBody);

            log.Info($"Create rating request received for product \"{rating.ProductId}\" from user \"{rating.UserId}\".");

            rating = GetSentimentAnalysis(log, rating);

            if (rating.SentimentScore < 0.3)
            {
                TrackTelemetry(context, rating);
            }

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

        private static void TrackTelemetry(ExecutionContext context, Rating rating)
        {
            

            // Track an Event
            var evt = new EventTelemetry("BadSentiment");
            UpdateTelemetryContext(evt.Context, context, rating.UserId, rating.SentimentScore);
            telemetryClient.TrackEvent(evt);
        }

        private static void UpdateTelemetryContext(TelemetryContext context, ExecutionContext functionContext, string userName, float sentimentScore)
        {
            context.Operation.Id = functionContext.InvocationId.ToString();
            context.Operation.ParentId = functionContext.InvocationId.ToString();
            context.Operation.Name = functionContext.FunctionName;
            context.User.Id = userName;
            context.Properties["sentimentScore"] = sentimentScore.ToString();
        }

        private static Rating GetSentimentAnalysis(TraceWriter log, Rating rating)
        {
            // Handle sentiment analysis for usernotes
            // Create a client.
            ITextAnalyticsAPI client = new TextAnalyticsAPI
            {
                AzureRegion = AzureRegions.Westus,
                SubscriptionKey = Environment.GetEnvironmentVariable("COGNITIVE_SERVICES_KEY")
            };

            Guid sentimentId = Guid.NewGuid();

            SentimentBatchResult result = client.Sentiment(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", sentimentId.ToString() , rating.UserNotes),
                        }));


            // Printing sentiment results
            foreach (var document in result.Documents)
            {
                log.Info($"Document ID: {document.Id} , Sentiment Score: {document.Score}");
                rating.SentimentScore = (float)document.Score;
            }

            return rating;
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
            var documentClientDetails = await DocumentDbClientFactory.GetRatingsClientAsync();

            var document = await documentClientDetails.DocumentClient.CreateDocumentAsync(documentClientDetails.DocumentCollectionLink, rating);

            return (Rating)((dynamic)document.Resource);
        }
    }
}
