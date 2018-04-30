using Microsoft.Azure.Documents.Client;
using System;

namespace OhIceCreamShopApps.Models
{
    public class DocumentClientItem
    {
        public Uri DatabaseLink { get; set; }

        public Uri DocumentCollectionLink { get; set; }

        public DocumentClient DocumentClient { get; set; }
    }
}