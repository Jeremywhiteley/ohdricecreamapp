
using CsvHelper.Configuration;
namespace CreatePODoc
{
    // ponumber,datetime,locationid,locationname,locationaddress,locationpostcode,totalcost,totaltax
    public class ProductInformationMap
        : ClassMap<ProductInformation>
    {
        public ProductInformationMap()
        {
            Map(m => m.productid).Name("productid");
            Map(m => m.productname).Name("productname");
            Map(m => m.productdescription).Name("productdescription");
        }
    }
}
