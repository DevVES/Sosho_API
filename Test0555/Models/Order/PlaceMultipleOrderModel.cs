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
        public string JurisdictionID { get; set; }
        public string AddressId { get; set; }
        public string discountamount { get; set; } = "";
        public string Redeemeamount { get; set; } = "";
        public string PromoCodeamount { get; set; } = "";
        public string WalletId;
        public string WalletLinkId;
        public string WalletType;
        public string WalletCrAmount;
        public string WalletCrDate;
        public string WalletCrDescription;
        public string Walletbalance;


        public string PromoCodeId;
        public string PromoCodeLinkId;
        public string PromoCodetype;
        public string PromoCodeCrAmount;
        public string PromoCodeCrDate;
        public string PromoCodeCrDescription;
        public string PromoCodebalance;
        public string orderMRP { get; set; }
        public string totalAmount { get; set; }
        public string totalQty { get; set; }
        public string totalWeight { get; set; }
        public List<ProductListNew> products { get; set; }
    }
    public class ProductListNew
    {
        public bool IsBannerProduct { get; set; }
        public string productid { get; set; }
        public string couponCode { get; set; } = "0";
        public string refrcode { get; set; } = "0";
        public string Quantity { get; set; }
        public decimal PaidAmount { get; set; }
        public string UnitId { get; set; }
        public string Unit { get; set; }
        public string AttributeId { get; set; }
    }
}