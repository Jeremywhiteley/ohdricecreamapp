using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Challenge7
{
    public static class BatchProcess
    {
        [FunctionName("BatchProcess")]
        public static void Run([EventHubTrigger("batchhub", Connection = "eventConnectionString")]EventData[] myEventHubMessages, TraceWriter log)
        {
            // set up docDB client
            var documentClient = new DocumentClient(new Uri("https://ohdrteamdb-001.documents.azure.com:443/"), "1ZHhCMRJGECdqFajFLVIYYhSQG5OIsQSo0OjFacj9b9ZQeYvKZ5nDzXwXYpBomEmgpht9v5EAnNcYDYkF4TWuA==");
            var databaseResponse = documentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = "Challenge7DB" }).Result;
            var database = databaseResponse.Resource as Database;
            var databaseLink = UriFactory.CreateDatabaseUri(database.Id);
            var documentCollectionResponse = documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, new DocumentCollection() { Id = "Challenge7Orders" }).Result;
            var documentCollection = documentCollectionResponse.Resource as DocumentCollection;
            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri(database.Id, documentCollection.Id);

            // process messages
            foreach (EventData message in myEventHubMessages)
            {
                string messagePaylog = Encoding.UTF8.GetString(message.Body.Array);
                if (!messagePaylog.StartsWith("Message"))
                {
                    // process each message
                    var myEvent = JsonConvert.DeserializeObject(messagePaylog);

                    // write to DocDB
                    documentClient.CreateDocumentAsync(documentCollectionLink, myEvent).Wait();
                }
                
            }

        }
    }
}
