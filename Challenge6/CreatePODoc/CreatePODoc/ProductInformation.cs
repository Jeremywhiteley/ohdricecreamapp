using Newtonsoft.Json;
using System;
namespace CreatePODoc
{
    //productid	productname	productdescription

    public class ProductInformation
    {
        [JsonProperty("productid")]
        public string productid { get; set; }

        [JsonProperty("productname")]
        public string productname { get; set; }

        [JsonProperty("productdescription")]
        public string productdescription { get; set; }
    }
}