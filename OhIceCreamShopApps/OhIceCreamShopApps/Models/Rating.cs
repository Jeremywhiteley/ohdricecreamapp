using Newtonsoft.Json;

namespace OhIceCreamShopApps.Models
{
    public class Rating
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userid")]
        public string UserId { get; set; }

        [JsonProperty("productid")]
        public string ProductId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("locationname")]
        public string LocationName { get; set; }

        [JsonProperty("usernotes")]
        public string UserNotes { get; set; }

        [JsonProperty("sentimentscore")]
        public float SentimentScore { get; set; }
    }

    public class ProductInfo
    {
        public string productid { get; set; }
        public string productname { get; set; }
        public string productdescription { get; set; }
    }

    public class UserInfo
    {
        public string userid { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
    }
}
