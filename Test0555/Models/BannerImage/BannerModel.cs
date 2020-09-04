using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models
{
    public class BannerModel
    {
        public class BnnerImage
        {

            public string response;
            public string message;
            public List<BannerDataList> BannerImageList { get; set; }
            

        }
        public class BannerDataList
        {
            public string ImgUrl;
            public string Title;
            public string AltText;
            public string DataLink;
        }

        public class NewBnnerImage
        {
            public string response;
            public string message;
            public string BannerPosition;
            public List<IntermediateBannerImage> IntermediateBannerImages { get; set; }
            public List<IntermediateBannerImage> BannerImageList { get; set; }
        }
        public class IntermediateBannerImage
        {
            public string bannerURL;
            public string bannerId;
            public int ActionId;
            public string action;
            public string categoryId;
            public string categoryName;
            public int ProductId;
            public string ProductName;
            public string openUrlLink;
            public string  MaxQty;
            public string MinQty;
            public bool IsQtyFreeze;
            public string MRP;
            public string Discount;
            public string SellingPrice;
            public string Weight;
        }
    }
}