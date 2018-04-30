
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OhIceCreamShopApps.Models;
using System;
using System.Threading.Tasks;

namespace OhIceCreamShopApps
{
    public static class DocumentDbClientFactory
    {
        public static async Task<DocumentClientItem> GetDocumentClientAsync()
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

            return new DocumentClientItem()
            {
                DatabaseLink = databaseLink,
                DocumentCollectionLink = documentCollectionLink,
                DocumentClient = documentClient,
            };
        }
    }
}