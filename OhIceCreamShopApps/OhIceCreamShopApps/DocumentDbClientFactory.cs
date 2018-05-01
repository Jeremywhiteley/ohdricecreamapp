
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OhIceCreamShopApps.Models;
using System;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public sealed class DocumentClientDetails
    {
        public Uri DatabaseLink { get; set; }

        public Uri RatingsCollectionLink { get; set; }

        public DocumentClient DocumentClient { get; set; }
    }

    public static class DocumentDbClientFactory
    {
        private static object ResolveDocumentClientItemLock = new object();
        private static DocumentClientDetails ResolvedDocumentClientItem;

        public static async ValueTask<DocumentClientDetails> GetDocumentClientAsync()
        {
            if (ResolvedDocumentClientItem != null)
            {
                return ResolvedDocumentClientItem;
            }

            // TODO: technically there can still be a race here if two threads come in at the same time, but we can sort that out later
            ResolvedDocumentClientItem = await InitializeDocumentClientAsync();

            return ResolvedDocumentClientItem;

            async Task<DocumentClientDetails> InitializeDocumentClientAsync()
            {
                var uri = new Uri(Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT_URI"));
                var secret = Environment.GetEnvironmentVariable("COSMOSDB_SECRET");
                var documentClient = new DocumentClient(uri, secret);

                var database = new Database { Id = "IceCreamApp" };
                var databaseItem = await documentClient.CreateDatabaseIfNotExistsAsync(database);
                var databaseLink = UriFactory.CreateDatabaseUri(database.Id);

                var ratingsCollection = new DocumentCollection { Id = "Ratings" };
                var ratingsCollectionItem = await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, ratingsCollection);
                var ratingCollectionLink = UriFactory.CreateDocumentCollectionUri(database.Id, ratingsCollection.Id);

                return new DocumentClientDetails
                {
                    DatabaseLink = databaseLink,
                    RatingsCollectionLink = ratingCollectionLink,
                    DocumentClient = documentClient,
                };
            }
        }
    }
}