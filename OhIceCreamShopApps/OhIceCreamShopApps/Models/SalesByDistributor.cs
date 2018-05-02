using Newtonsoft.Json;

namespace OhIceCreamShopApps.Models
{
    public class SalesByDistributor
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("distributor")]
        public string Distributor { get; set; }

        [JsonProperty("pointOfSale")]
        public string PointOfSale { get; set; }

        [JsonProperty("totalQuantitySold")]
        public int TotalQuantitySold { get; set; }

        [JsonProperty("totalSales")]
        public float TotalSales { get; set; }
    }
}
