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
        public decimal Cashbackamount { get; set; } = 0;
        public string discountamount { get; set; } = "";
        public string Redeemeamount { get; set; } = "";
        //public string PromoCodeamount { get; set; } = "";
        public string WalletId;
        public string WalletLinkId;
        public string WalletType;
        public string WalletCrAmount;
        public string WalletCrDate;
        public string WalletCrDescription;
        public string Walletbalance;


        public string PromoCodeId;
        public string PromoCode;
        public string PromoCodeLinkId;
        public string PromoCodetype;
        public string PromoCodeCrAmount;
        public string PromoCodeCrDate;
        public string PromoCodeCrDescription;
        public string PromoCodebalance;
        public string orderMRP { get; set; }
        public string totalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string totalQty { get; set; }
        public string totalWeight { get; set; }
        public string ReOrderId { get; set; }
        public List<ProductListNew> products { get; set; }
    }
    public class ProductListNew
    {
        public int BannerProductType { get; set; }
        public int BannerId { get; set; }
        public string productid { get; set; }
        public string couponCode { get; set; } = "0";
        public string refrcode { get; set; } = "0";
        public string Quantity { get; set; }
        public decimal PaidAmount { get; set; }
        public string UnitId { get; set; }
        public string Unit { get; set; }
        public string AttributeId { get; set; }
    }
    public class ReOrderProductList
    {
        public string response { get; set; }
        public string message { get; set; }
        public string AddressId { get; set; }
        public List<CustAddressDataList> CustAddressList { get; set; }
        public List<NewProductDataList> ProductList { get; set; }
        //public string OrderId { get; set; }
        
        //public string ProductId { get; set; }
        //public string AttributeId { get; set; }
        //public string ProductName { get; set; }
        //public string UnitId { get; set; }
        //public string UnitName { get; set; }
        //public string Unit { get; set; }
        //public bool isOutOfStock { get; set; }
        //public bool isOfferExpired { get; set; }
        //public decimal Mrp { get; set; }
        //public decimal SoshoPrice { get; set; }
        //public decimal Quantity { get; set; }
    }

    public class CustAddressDataList
    {
        public string Custid;
        public string fname;
        public string lname;
        public string tagname;
        public string countryId;
        public string countryName;
        public string stateId;
        public string statename;
        public string cityId;
        public string cityname;
        public string addr;
        public string email;
        public string pcode;
        public string mob;
        public string CustomerAddressId;
        public string AreaId;
        public string Area;
        public string BuildingId;
        public string Building;
        public string BuildingNo;
        public string LandMark;
        public string OtherDetail;


    }
    public class NewProductDataList
    {
        public NewProductDataList()
        {
            ProductAttributesList = new List<ProductAttributelist>();
        }
        public string CategoryId;
        public string CategoryName;
        public string ProductId;
        public string ProductName;
        //public int Quantity;
        public string OfferEndDate;
        public string ItemType;
        public string Title;
        public string bannerURL;
        public string bannerId;
        public bool isOfferExpired { get; set; }
        public bool isProductAvailable { get; set; }
        public List<ProductAttributelist> ProductAttributesList { get; set; }
    }

    public class ProductAttributelist
    {
        public double Mrp;
        public string Discount;
        public string PackingType;
        public double soshoPrice;
        public string weight;
        public bool isOutOfStock;
        public bool isSelected;
        public bool isQtyFreeze;
        public bool isBestBuy;
        public int MinQty;
        public int MaxQty;
        public string AttributeId;
        public string AImageName;
        public int Quantity;
    }
}