using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CreatePODoc
{
    public class OrderHeaderDetails
    {
        public OrderHeaderDetails ()
        {
            lineitems = new List<OrderLineItems>();
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("ponumber")]
        public string poNumber { get; set; }

        [JsonProperty("locationId")]
        public string LocationId { get; set; }

        [JsonProperty("locationName")]
        public string LocationName { get; set; }

        [JsonProperty("locationAddress")]
        public string LocationAddress { get; set; }

        [JsonProperty("locationPostCode")]
        public string LocationPostCode { get; set; }

        [JsonProperty("totalCost")]
        public double TotalCost { get; set; }

        [JsonProperty("totalTax")]
        public double TotalTax { get; set; }

        [JsonProperty("lineitems")]
        public List<OrderLineItems> lineitems { get; set; }


    }
}