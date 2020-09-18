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
            //public List<TopBannerImage> TopBannerImages { get; set; }
            //public List<SecondBannerImage> SecondBannerImages { get; set; }

            public string response;
            public string message;
            public string WhatsAppNo;
            public List<NewProductDataList> ProductList { get; set; }
        }
        public class NewProductDataList
        {
            public NewProductDataList()
            {
                ProductImageList = new List<ProductDataImagelist>();
                ProductAttributesList = new List<ProductAttributelist>();
            }
            public List<ProductDataImagelist> ProductImageList { get; set; }
            public List<ProductAttributelist> ProductAttributesList { get; set; }

            public string CategoryId;
            public string CategoryName;
            public string MRP;
            public string Discount;
            public string Name;
            public string OfferEndDate;
            public string SellingPrice;
            public string SoldCount;
            public string SpecialMessage;
            public string Weight;
            public string DisplayOrder;
            //public string IsProductDetails;
            public string IsProductVariant;
            public string IsQtyFreeze;
            public string SoshoRecommended;
            public string IsSoshoRecommended;
            public string IsSpecialMessage;
            public string MaxQty;
            public string MinQty;
            public string isFreeShipping;
            public string isFixedShipping;
            public string FixedShipRate;
            public string IsProductDescription;
            public string ProductDescription;
            public string ProductNotes;
            public string ProductKeyFeatures;
            public string ProductId;

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
            public string Mrp;
            public string Discount;
            public string PackingType;
            public string soshoPrice;
            public string weight;
            public string isOutOfStock;
            public string isSelected;
            public string isQtyFreeze;
            public string isBestBuy;
            public string MinQty;
            public string MaxQty;
            public string packSizeId;
            public string AImageName;
        }
        

        
    }
}