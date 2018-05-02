using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using OhIceCreamShopApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class UpdateAggregates
    {
        private static readonly HttpClient ApiHttpClient = new HttpClient();

        [FunctionName("UpdateAggregates")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var ratings = await FetchRatingsAsync();
            var products = await FetchProductsAsync();
            var importedOrders = await FetchImportedOrdersAsync();
            var liveOrders = await FetchLiveOrdersAsync();

            var sentimentScores = products.GroupJoin(ratings, pi => pi.productid, r => r.ProductId, (pi, r) => new SentimentReportingItem()
            {
                Id = pi.productid,
                ProductId = pi.productid,
                ProductName = pi.productname,
                TotalRatings = r.Count(),
                TotalSentimentScore = r.Sum(item => item.SentimentScore),
            });

            var groupedImportedOrdersByProduct = importedOrders.SelectMany(ioi => ioi.Order)
                .SelectMany(o => o.lineitems)
                .GroupBy(li => new { productId = li.productid, productName = li.productname })
                .Select(o => new SalesByProduct()
                {
                    Id = o.Key.productId,
                    ProductId = o.Key.productId,
                    ProductName = o.Key.productName,
                    TotalQuantitySold = o.Sum(li => li.quantity),
                    TotalSales = o.Sum(li => li.totalcost),
                });

            var groupedLiveOrdersByProduct = liveOrders.SelectMany(loi => loi.details)
                .GroupBy(li => new { li.productId, li.productName })
                .Select(o => new SalesByProduct()
                {
                    Id = o.Key.productId,
                    ProductId = o.Key.productId,
                    ProductName = o.Key.productName,
                    TotalQuantitySold = o.Sum(li => Int32.Parse(li.quantity)),
                    TotalSales = o.Sum(li => (float)Decimal.Parse(li.totalCost)),
                });

            var productResults = groupedLiveOrdersByProduct.GroupJoin(groupedImportedOrdersByProduct, o => o.ProductId, o => o.ProductId, (a, b) => new SalesByProduct()
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.ProductName,
                TotalQuantitySold = a.TotalQuantitySold + b.Sum(c => c.TotalQuantitySold),
                TotalSales = a.TotalSales + b.Sum(c => c.TotalSales),
            });

            var groupedImportedOrdersByDistributor = importedOrders.SelectMany(ioi => ioi.Order)
                .GroupBy(o => new { o.locationName, o.ponumber })
                .Select(o => new SalesByDistributor()
                {
                    Id = o.Key.ponumber,
                    Distributor = o.Key.locationName,
                    PointOfSale = o.Key.ponumber,
                    TotalQuantitySold = o.Sum(li => li.lineitems.Sum(a => a.quantity)),
                    TotalSales = o.Sum(li => li.lineitems.Sum(a => a.totalcost)),
                });

            var groupedLiveOrdersByDistributor = liveOrders
                .GroupBy(h => new { h.header.locationName, h.header.salesNumber })
                .Select(o => new SalesByDistributor()
                {
                    Id = o.Key.salesNumber,
                    Distributor = o.Key.locationName,
                    PointOfSale = o.Key.salesNumber,
                    TotalQuantitySold = o.Sum(li => li.details.Sum(a => Int32.Parse(a.quantity))),
                    TotalSales = o.Sum(li => li.details.Sum(a => (float)Decimal.Parse(a.totalCost))),
                });

            var distributorResults = groupedLiveOrdersByDistributor.GroupJoin(groupedImportedOrdersByDistributor, o => o.Distributor, o => o.Distributor, (a, b) => new SalesByDistributor()
            {
                Id = a.Id,
                Distributor = a.Distributor,
                PointOfSale = a.PointOfSale,
                TotalQuantitySold = a.TotalQuantitySold + b.Sum(c => c.TotalQuantitySold),
                TotalSales = a.TotalSales + b.Sum(c => c.TotalSales),
            });

            foreach (var sentimentScore in sentimentScores)
            {
                await UpsertDocumentAsync(sentimentScore, "SentimentScore", sentimentScore.Id);
            }

            foreach (var productResult in productResults)
            {
                await UpsertDocumentAsync(productResult, "ProductScore", productResult.Id);
            }

            foreach (var distributorResult in distributorResults)
            {
                await UpsertDocumentAsync(distributorResult, "DistributorScore", distributorResult.Id);
            }
        }

        private static async Task UpsertDocumentAsync<T>(T item, string collectionName, string id)
        {
            var documentClientDetails = await DocumentDbClientFactory.GetDocumentClientAsync("ChallengeAggregates", collectionName);

            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri("ChallengeAggregates", collectionName);
            await documentClientDetails.DocumentClient.UpsertDocumentAsync(documentCollectionLink, item);
        }

        private static async Task<IEnumerable<ImportOrderItem>> FetchImportedOrdersAsync()
        {
            var documentClientDetails = await DocumentDbClientFactory.GetDocumentClientAsync("ChallengeDatabase", "OrderHeaderDetails");

            var ratingsQuery = documentClientDetails.DocumentClient
                .CreateDocumentQuery<ImportOrderItem>(documentClientDetails.DocumentCollectionLink)
                .AsDocumentQuery();

            var orders = new List<ImportOrderItem>();

            while (ratingsQuery.HasMoreResults)
            {
                var results = await ratingsQuery.ExecuteNextAsync<ImportOrderItem>();
                orders.AddRange(results);
            }

            return orders;
        }

        private static async Task<IEnumerable<LiveOrderItem>> FetchLiveOrdersAsync()
        {
            var documentClientDetails = await DocumentDbClientFactory.GetDocumentClientAsync("Challenge7DB", "Challenge7Orders");

            var ratingsQuery = documentClientDetails.DocumentClient
                .CreateDocumentQuery<LiveOrderItem>(documentClientDetails.DocumentCollectionLink)
                .AsDocumentQuery();

            var orders = new List<LiveOrderItem>();

            while (ratingsQuery.HasMoreResults)
            {
                var results = await ratingsQuery.ExecuteNextAsync<LiveOrderItem>();
                orders.AddRange(results);
            }

            return orders;
        }

        private static async Task<IEnumerable<Rating>> FetchRatingsAsync()
        {
            var documentClientDetails = await DocumentDbClientFactory.GetRatingsClientAsync();

            var ratingsQuery = documentClientDetails.DocumentClient
                .CreateDocumentQuery<Rating>(documentClientDetails.DocumentCollectionLink)
                .AsDocumentQuery();

            var ratings = new List<Rating>();

            while (ratingsQuery.HasMoreResults)
            {
                var results = await ratingsQuery.ExecuteNextAsync<Rating>();
                ratings.AddRange(results);
            }

            return ratings;
        }

        private static async Task<IEnumerable<ProductInfo>> FetchProductsAsync()
        {
            using (var response = await ApiHttpClient.GetAsync($"https://serverlessohlondonproduct.azurewebsites.net/api/GetProducts"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return new List<ProductInfo>();
                }

                return await response.Content.ReadAsAsync<IEnumerable<ProductInfo>>();
            }
        }
    }
}
