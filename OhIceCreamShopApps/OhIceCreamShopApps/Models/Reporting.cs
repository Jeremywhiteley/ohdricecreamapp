using System;

namespace OhIceCreamShopApps.Models
{

    public class ImportOrderItem
    {
        public Order[] Order { get; set; }
    }

    public class Order
    {
        public string id { get; set; }
        public string ponumber { get; set; }
        public DateTime datetime { get; set; }
        public string locationId { get; set; }
        public string locationName { get; set; }
        public string locationAddress { get; set; }
        public string locationPostCode { get; set; }
        public float totalCost { get; set; }
        public float totalTax { get; set; }
        public Lineitem[] lineitems { get; set; }
    }

    public class Lineitem
    {
        public string id { get; set; }
        public string ponumber { get; set; }
        public string productid { get; set; }
        public string productname { get; set; }
        public string productdescription { get; set; }
        public int quantity { get; set; }
        public float unitcost { get; set; }
        public float totalcost { get; set; }
        public float totaltax { get; set; }
    }

    public class LiveOrderItem
    {
        public Header header { get; set; }
        public Detail[] details { get; set; }
    }

    public class Header
    {
        public string salesNumber { get; set; }
        public string dateTime { get; set; }
        public string locationId { get; set; }
        public string locationName { get; set; }
        public string locationAddress { get; set; }
        public string locationPostcode { get; set; }
        public string totalCost { get; set; }
        public string totalTax { get; set; }
    }

    public class Detail
    {
        public string productId { get; set; }
        public string quantity { get; set; }
        public string unitCost { get; set; }
        public string totalCost { get; set; }
        public string totalTax { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
    }
}
