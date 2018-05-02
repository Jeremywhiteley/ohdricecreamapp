using Newtonsoft.Json;

namespace OhIceCreamShopApps.Models
{
    public class SalesByProduct
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("totalQuantitySold")]
        public int TotalQuantitySold { get; set; }

        [JsonProperty("totalSales")]
        public float TotalSales { get; set; }
    }
}
