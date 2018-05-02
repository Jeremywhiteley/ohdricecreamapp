
using CsvHelper.Configuration;
namespace CreatePODoc
{
    // ponumber,datetime,locationid,locationname,locationaddress,locationpostcode,totalcost,totaltax
    public class OrderLineItemsMap
        : ClassMap<OrderLineItems>
    {
        public OrderLineItemsMap()
        {
            Map(m => m.poNumber).Name("ponumber");
            Map(m => m.productid).Name("productid");
            Map(m => m.quantity).Name("quantity");
            Map(m => m.unitcost).Name("unitcost");
            Map(m => m.totalcost).Name("totalcost");
            Map(m => m.totaltax).Name("totaltax");
        }
    }
}
