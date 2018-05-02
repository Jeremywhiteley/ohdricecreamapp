
using CsvHelper.Configuration;
namespace CreatePODoc
{
    // ponumber,datetime,locationid,locationname,locationaddress,locationpostcode,totalcost,totaltax
    public class OrderHeaderDetailsMap
        : ClassMap<OrderHeaderDetails>
    {
        public OrderHeaderDetailsMap()
        {
            Map(m => m.poNumber).Name("ponumber");
            Map(m => m.LocationId).Name("locationid");
            Map(m => m.LocationName).Name("locationname");
            Map(m => m.LocationAddress).Name("locationaddress");
            Map(m => m.LocationPostCode).Name("locationpostcode");
            Map(m => m.TotalCost).Name("totalcost");
            Map(m => m.TotalTax).Name("totaltax");
        }
    }
}
