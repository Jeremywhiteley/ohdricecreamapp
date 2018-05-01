using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BatchFileHandling
{
    public class EventGridEvent
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("eventType")]
        public string EventType { get; set; }
        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; }
        [JsonProperty("data")]
        public StorageItem Data { get; set; }
        [JsonProperty("metadataVersion")]
        public int MetadataVersion { get; set; }
        [JsonProperty("dataVersion")]
        public int? DataVersion { get; set; }
    }
}
