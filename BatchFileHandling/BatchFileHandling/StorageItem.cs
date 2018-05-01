using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BatchFileHandling
{
    public class StorageItem
    {
        [JsonProperty("api")]
        public string Api { get; set; }
        [JsonProperty("clientRequestId")]
        public Guid ClientRequestId { get; set; }
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }
        [JsonProperty("eTag")]
        public string ETag { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("contentLength")]
        public int ContentLength { get; set; }
        [JsonProperty("blobType")]
        public string BlobType { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("sequencer")]
        public string Sequencer { get; set; }
        [JsonProperty("storageDiagnostics")]
        public StorageDiagnostics StorageDiagnostics { get; set; }
    }
}
