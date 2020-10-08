using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.ProductManagement
{
    public class ProductModel
    {
        public class getproduct
        {
            public List<TopBannerImage> TopBannerImages { get; set; }
            public List<SecondBannerImage> SecondBannerImages { get; set; }
            public string response;
            public string message;

            public string ButtonDisplayMessage;
            public string IsDisplayOffer;
            public string IsDisplasyOfferAll;
            public string whatsapp;
            public List<ProductDataList> ProductList { get; set; }
            public List<OfferProductList> OfferProductList { get; set; }

        }
        //20-08-2020 Developed By :- Hiren
        public class getNewproduct
        {
            public string response;
            public string message;
            public string WhatsAppNo;
            public string BannerPosition;
            //public List<HomePageBannerImage> HomePageBannerImages { get; set; }
            public List<NewProductDataList> ProductList { get; set; }

        }

        public class HomePageBannerImage
        {
            public string Title;
            public string bannerURL;
            public string bannerId;
            public int ActionId;
            public string action;
            public string categoryId;
            public string categoryName;
            public int ProductId;
            public string ProductName;
            public string openUrlLink;
            public List<ProductAttributelist> ProductAttributesList { get; set; }
        }
        public class NewProductDataList
        {
            public NewProductDataList()
            {
                //ProductImageList = new List<ProductDataImagelist>();
                ProductAttributesList = new List<ProductAttributelist>();
            }
            //public List<ProductDataImagelist> ProductImageList { get; set; }
            public List<ProductAttributelist> ProductAttributesList { get; set; }

            public string CategoryId;
            public string CategoryName;
            public string ProductId;
            public string ProductName;
            public string OfferEndDate;
            public string SoldCount;
            public int DisplayOrder;
            public string SoshoRecommended;
            public bool IsSoshoRecommended;
            public string SpecialMessage;
            public bool IsSpecialMessage;
            public bool IsProductDescription;
            public string ProductDescription;
            public string ProductNotes;
            public string ProductKeyFeatures;
            public string ItemType;

            //public string SubCategoryId;
            //public string SubCategoryName;
            //public string MRP;
            //public string Discount;
            //public string Name;
            //public string SellingPrice;
            //public string Weight;
            //public string IsProductVariant;
            //public string IsQtyFreeze;
            //public string MaxQty;
            //public string MinQty;
            public bool isFreeShipping;
            public bool isFixedShipping;
            public double FixedShipRate;

            public string Title;
            public string bannerURL;
            public string bannerId;
            public int ActionId;
            public string action;
            public string openUrlLink;
            public string ActionCategoryId;
            public string ActionCategoryName;
        }
        public class ProductDataList
        {
            public ProductDataList()
            {
                ProductImageList = new List<ProductDataImagelist>();
            }

            public string productid;
            public string whatsapp;

            public string whatsappmsg;

            public string minqty;
            public string maxqty;

            public string pname;
            public string pdec;
            public string pkey;

            public string pnote;
            public string pprice;
            public string pwight;
            public string pvideo;
            public string enddate;
            public string penddate;
            public string poffer;
            public string pbuy2;
            public string pbuy5;
            public string shipping;
            public string psold;
            public string pJustBougth;
            public string pgst;
            public string IsQtyFreeze;
           





            public List<ProductDataImagelist> ProductImageList { get; set; }
         
        }

        public class OfferProductList
        {
            
            public string productid;
            public string pname;
            public string pdec;
            public string pkey;
            public string pprice;
            public string PImgname;
        

        }

        public class ProductDataImagelist
        {
            public string prodid;
            public string proimagid;
            public string PImgname;
            public string PDisOrder;
        }

        public class TopBannerImage
        {
            public string bannerURL;
            public string bannerId;
        }
        public class SecondBannerImage
        {
            public string bannerURL;
            public string bannerId;
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
        }
        

        
    }
}