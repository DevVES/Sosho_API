using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Order
{
    public class PlaceMultipleOrderModel
    {
        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string discountamount { get; set; } = "";
        public string Redeemeamount { get; set; } = "";
        public string orderMRP { get; set; }
        public string totalAmount { get; set; }
        public string totalQty { get; set; }
        public string totalWeight { get; set; }
        public List<ProductList> products { get; set; }
    }

    public class ProductList
    {
        public string productid { get; set; }
        public string couponCode { get; set; } = "0";
        public string refrcode { get; set; } = "0";
        public string Quantity { get; set; }
        public string buywith { get; set; }
        public decimal PaidAmount { get; set; }
    }
    public class PlaceMultipleOrderNewModel
    {
        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string discountamount { get; set; } = "";
        public string Redeemeamount { get; set; } = "";
        public string orderMRP { get; set; }
        public string totalAmount { get; set; }
        public string totalQty { get; set; }
        public string totalWeight { get; set; }
        public List<ProductListNew> products { get; set; }
    }
    public class ProductListNew
    {
        public string productid { get; set; }
        public string couponCode { get; set; } = "0";
        public string refrcode { get; set; } = "0";
        public string Quantity { get; set; }
        public decimal PaidAmount { get; set; }
        public string UnitId { get; set; }
        public string Unit { get; set; }
    }
}