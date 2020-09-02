using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Order
{
    public class OrderSummeryModels
    {
        public class OrderSummery
        {
            public string Response;
            public string Message;
            //public List<OrderProductDataList> OrderProductList { get; set; }
            public List<OrderCustDataList> OrderCustomerList { get; set; }
        }

        public class OrderProductDataList
        {
            public string Proid;
            public string PName;
            public string proImage;
            public string produnit;
            public string unit;
            public string price;
            public string weight;
            public string Dunit;
          
        
        }

        public class OrderCustDataList
        {
                        
            public string cid;
            public string Caddrid;
            public string Cfname;
            public string Clname;
            public string addr;
            public string cph;
            public string tag;
            public string Countryname;
            public string statename;
            public string Cityname;
            

        }


        public class getproduct
        {

            public string response;
            public string message;
            public List<ProductDataList> ProductList { get; set; }


        }

        public class ProductDataList
        {
            public ProductDataList()
            {
                ProductImageList = new List<ProductDataImagelist>();
            }
            public string pname;
            public string pdec;
            public string pkey;

            public string pnote;
            public string pprice;
            public string pwight;
            public string pvideo;

            public string poffer;
            //public string pbuy2;
            //public string pbuy5;
            public string shipping;
            public string psold;
            public string pJustBougth;
            public string pgst;

            public List<ProductDataImagelist> ProductImageList { get; set; }
        }

        public class ProductDataImagelist
        {
            public string prodid;
            public string proimagid;
            public string PImgname;
            public string PDisOrder;
        }

    }
}