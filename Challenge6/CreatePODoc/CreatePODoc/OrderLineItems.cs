using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CreatePODoc
{
    //ponumber	productid	quantity	unitcost	totalcost	totaltax

    public class OrderLineItems
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("ponumber")]
        public string poNumber { get; set; }

        [JsonProperty("productid")]
        public string productid { get; set; }

        [JsonProperty("productname")]
        public string productname { get; set; }

        [JsonProperty("productdescription")]
        public string productdescription { get; set; }

        [JsonProperty("quantity")]
        public int quantity { get; set; }

        [JsonProperty("unitcost")]
        public decimal unitcost { get; set; }

        [JsonProperty("totalcost")]
        public decimal totalcost { get; set; }

        [JsonProperty("totaltax")]
        public decimal totaltax { get; set; }
       
    }
}