using Newtonsoft.Json;

namespace OhIceCreamShopApps.Models
{
    public class SentimentReportingItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("totalRatings")]
        public int TotalRatings { get; set; }

        [JsonProperty("totalSentimentScore")]
        public float TotalSentimentScore { get; set; }

        [JsonProperty("averageSentimentScore")]
        public float AverageSentimentScore
        {
            get
            {
                if (TotalRatings == 0) return 0;
                return TotalSentimentScore / TotalRatings;
            }
        }
    }
}
