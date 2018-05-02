
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public sealed class DocumentClientDetails
    {
        public Uri DatabaseLink { get; set; }

        public Uri DocumentCollectionLink { get; set; }

        public DocumentClient DocumentClient { get; set; }
    }

    public static class DocumentDbClientFactory
    {
        private static ConcurrentDictionary<string, DocumentClientDetails> ResolvedDocumentClientDetails = new ConcurrentDictionary<string, DocumentClientDetails>();

        public static async ValueTask<DocumentClientDetails> GetDocumentClientAsync(string databaseName, string collectionName)
        {
            if (ResolvedDocumentClientDetails.TryGetValue(collectionName, out var documentClientDetails))
            {
                return documentClientDetails;
            }

            documentClientDetails = await InitializeDocumentClientAsync(databaseName, collectionName);

            ResolvedDocumentClientDetails.TryAdd(collectionName, documentClientDetails);

            return documentClientDetails;
        }

        private static async Task<DocumentClientDetails> InitializeDocumentClientAsync(string databaseName, string collectionName)
        {
            var uri = new Uri(Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT_URI"));
            var secret = Environment.GetEnvironmentVariable("COSMOSDB_SECRET");
            var documentClient = new DocumentClient(uri, secret);

            var database = new Database { Id = databaseName };
            var databaseItem = await documentClient.CreateDatabaseIfNotExistsAsync(database);
            var databaseLink = UriFactory.CreateDatabaseUri(database.Id);

            var collection = new DocumentCollection { Id = collectionName };
            var collectionItem = await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, collection);
            var collectionLink = UriFactory.CreateDocumentCollectionUri(database.Id, collection.Id);

            return new DocumentClientDetails
            {
                DatabaseLink = databaseLink,
                DocumentCollectionLink = collectionLink,
                DocumentClient = documentClient,
            };
        }

        public static ValueTask<DocumentClientDetails> GetRatingsClientAsync() => GetDocumentClientAsync("IceCreamApp", "Ratings");
    }
}