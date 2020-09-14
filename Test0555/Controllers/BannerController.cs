using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models;
using System.Configuration;

namespace Test0555.Controllers
{
    public class BannerController : ApiController
    {
        dbConnection dbc = new dbConnection();
        [HttpGet]

        public BannerModel.BnnerImage getbannerimag()
        {
            BannerModel.BnnerImage objbaner = new BannerModel.BnnerImage();
            try
            {
                //DateTime datemain = dbc.getindiantime().ToString("dd/MMM/yyyy");
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                string querystr = "select top 1 * from HomepageBanner where IsActive=1 and IsDeleted=0 and Doc>='" + startdate + "' and Doc<='" + startend + "' order by Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                    DataTable dtpath = dbc.GetDataTable(querydata);
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        objbaner.response = "1";
                        objbaner.message = "Successfully";
                        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                        objbaner.BannerImageList = new List<BannerModel.BannerDataList>();
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string Title1 = dtmain.Rows[i]["Title"].ToString();
                            string AltText1 = dtmain.Rows[i]["AltText"].ToString();
                            string DataLink1 = dtmain.Rows[i]["Link"].ToString();
                            string ImageName1 = dtmain.Rows[i]["ImageName"].ToString();
                            objbaner.BannerImageList.Add(new BannerModel.BannerDataList
                            {
                                ImgUrl = urlpath + ImageName1,
                                Title = Title1,
                                AltText = AltText1,
                                DataLink = DataLink1

                            });
                        }
                    }
                    else
                    {
                        objbaner.response = "0";
                        objbaner.message = "Details Not Found";
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Details Not Found";

                }

                return objbaner;
            }
            catch (Exception ee)
            {
                objbaner.response = "-1";
                objbaner.message = "Something Wrong ";
                return objbaner;
            }
        }

        [HttpGet]

        public BannerModel.NewBnnerImage GetDashBoardBannerImag(string JurisdictionId = "")
        {
            BannerModel.NewBnnerImage objbaner = new BannerModel.NewBnnerImage();
            try
            {
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                int iBannerPosition = (ConfigurationManager.AppSettings["BannerPosition"] != null && ConfigurationManager.AppSettings["BannerPosition"].Trim() != "") ? Convert.ToInt16(ConfigurationManager.AppSettings["BannerPosition"].Trim()) : 0;
                objbaner.BannerPosition = iBannerPosition.ToString();
                string querystr = "select * from HomepageBanner where IsActive=1 and IsDeleted=0 and Doc>='" + startdate + "' and Doc<='" + startend + "' order by Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                    DataTable dtpath = dbc.GetDataTable(querydata);
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        objbaner.response = "1";
                        objbaner.message = "Successfully";

                        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                        objbaner.BannerImageList = new List<BannerModel.IntermediateBannerImage>();
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string ImageName1 = dtmain.Rows[i]["ImageName"].ToString();
                            string Id = dtmain.Rows[i]["Id"].ToString();
                            string title = dtmain.Rows[i]["Title"].ToString();
                            objbaner.BannerImageList.Add(new BannerModel.IntermediateBannerImage
                            {
                                Title = title,
                                bannerURL = urlpath + ImageName1,
                                bannerId = Id,
                                action = "",
                                categoryId = "",
                                categoryName = "",
                                openUrlLink = "",
                                ProductName = "",
                                MaxQty = "",
                                MinQty = "",
                                IsQtyFreeze = false,
                                MRP = "",
                                Discount = "",
                                SellingPrice = "",
                                Weight = ""

                            });
                        }
                    }
                    else
                    {
                        objbaner.response = "0";
                        objbaner.message = "Details Not Found";
                        objbaner.BannerPosition = iBannerPosition.ToString();
                        objbaner.BannerImageList = new List<BannerModel.IntermediateBannerImage>();
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Details Not Found";
                    objbaner.BannerPosition = iBannerPosition.ToString();
                    objbaner.BannerImageList = new List<BannerModel.IntermediateBannerImage>();

                }
                //Intermediate Banner
                string bannerqry = "Select Distinct TypeId From IntermediateBanners where IsActive=1 and IsDeleted=0";
                DataTable dtBanner = dbc.GetDataTable(bannerqry);

                objbaner.IntermediateBannerImages = new List<BannerModel.IntermediateBannerImage>();
                if (dtBanner != null && dtBanner.Rows.Count > 0)
                {
                    string sTypeId = "";
                    for (int i = 0; i < dtBanner.Rows.Count; i++)
                    {
                        sTypeId = dtBanner.Rows[i]["TypeId"].ToString();
                        string cond = "";
                        if (JurisdictionId != "" && JurisdictionId != null)
                        {
                            cond = " and JB.JurisdictionId = " + JurisdictionId;
                        }
                            string qry = "Select  ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty,ISNULL(PC.CategoryID,0) AS ProdCategoryId," + 
                                     " ISNULL(PC.CategoryName,'') AS ProductCategoryName, ISNULL(P.MinQty,0) AS MinQty, " +
                                     " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                                     " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(Im.Action,'') AS Action, ISNULL(Im.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                                     " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " + 
                                     " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title " + 
                                     " From IntermediateBanners Im " +
                                     " Left join category cg on  cg.categoryId = Im.categoryId " +
                                     " Left join Product P on  P.Id = Im.ProductId " +
                                     " Left join category PC on PC.CategoryID = P.CategoryID " +
                                     " Left join UnitMaster U on U.Id = P.UnitId " +
                                     " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                                     " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate + 
                                     "' and Im.StartDate<='" + startend + "' and TypeId = " + sTypeId + "" +
                                     cond + 
                                     "  order by Im.Id desc";
                        DataTable dtMainBanner = dbc.GetDataTable(qry);

                        string Id = "", ImageName1 = "", sAction = "", sCategoryId = "", sCategoryName = "", sopenUrlLink="";
                        string sProductName = "", sUnitName = "", sWeight = "", sSellingPrice = "", sMRP = "", sDiscount = "";
                        string sTitle = "";
                        string sMaxQty = "", sMinQty = "";
                        int sActionId = 0, sProductId = 0;
                        bool sIsQtyFreeze = false;
                        if (dtMainBanner != null && dtMainBanner.Rows.Count > 0)
                        {
                            string imagepathqry = "select KeyValue from StringResources where KeyName='TopBannerImageUrl'";
                            DataTable dtimagepath = dbc.GetDataTable(imagepathqry);
                            if (dtimagepath != null && dtimagepath.Rows.Count > 0)
                            {
                                objbaner.response = "1";
                                objbaner.message = "Successfully";
                                objbaner.BannerPosition = iBannerPosition.ToString();
                                string urlpath1 = dtimagepath.Rows[0]["KeyValue"].ToString();

                                Id = ""; ImageName1 = ""; sAction = ""; sCategoryId = ""; sCategoryName = ""; sopenUrlLink = "";
                                sProductName = ""; sUnitName = ""; sWeight = ""; sSellingPrice = ""; sMRP = ""; sDiscount = "";
                                sMaxQty = ""; sMinQty = "";
                                sActionId = 0;  sProductId = 0;
                                sIsQtyFreeze = false;
                                for (int j = 0; j < dtMainBanner.Rows.Count; j++)
                                {
                                    Id = dtMainBanner.Rows[j]["Id"].ToString();
                                    ImageName1 = dtMainBanner.Rows[j]["ImageName"].ToString();
                                    sAction = dtMainBanner.Rows[j]["Action"].ToString();
                                    if (!String.IsNullOrEmpty( dtMainBanner.Rows[j]["ActionId"].ToString()) || Convert.ToInt32(dtMainBanner.Rows[j]["ActionId"]) > 0 )
                                    {
                                        sActionId = Convert.ToInt32(dtMainBanner.Rows[j]["ActionId"]);
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["MaxQty"]) > 0)
                                    {
                                        sMaxQty = dtMainBanner.Rows[j]["MaxQty"].ToString();
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["MinQty"]) > 0)
                                    {
                                        sMinQty = dtMainBanner.Rows[j]["MinQty"].ToString();
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["ProductId"]) > 0)
                                    {
                                        sProductId = Convert.ToInt32(dtMainBanner.Rows[j]["ProductId"]);
                                    }
                                    sProductName = dtMainBanner.Rows[j]["ProductName"].ToString();
                                    sTitle = dtMainBanner.Rows[j]["Title"].ToString();
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["IsQtyFreeze"]) > 0)
                                    {
                                        sIsQtyFreeze = Convert.ToBoolean(dtMainBanner.Rows[j]["IsQtyFreeze"]);
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["MRP"]) > 0)
                                    {
                                        sMRP = dtMainBanner.Rows[j]["MRP"].ToString();
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["Discount"]) > 0)
                                    {
                                        sDiscount = dtMainBanner.Rows[j]["Discount"].ToString();
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["SellingPrice"]) > 0)
                                    {
                                        sSellingPrice = dtMainBanner.Rows[j]["SellingPrice"].ToString();
                                    }
                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["Weight"]) > 0)
                                    {
                                        sWeight = dtMainBanner.Rows[j]["Weight"].ToString();
                                    }
                                    if (!String.IsNullOrEmpty(dtMainBanner.Rows[j]["UnitName"].ToString()))
                                    {
                                        sUnitName = "-" + dtMainBanner.Rows[j]["UnitName"].ToString();
                                    }
                                    if (!string.IsNullOrEmpty(dtMainBanner.Rows[j]["CategoryID"].ToString()))
                                    {
                                        sCategoryId = dtMainBanner.Rows[j]["CategoryID"].ToString();
                                        sCategoryName = dtMainBanner.Rows[j]["CategoryName"].ToString();
                                    }
                                    else
                                    {
                                        sCategoryId = "0";
                                        sCategoryName = "";
                                    }
                                    if (!string.IsNullOrEmpty(dtMainBanner.Rows[j]["Link"].ToString()))
                                        sopenUrlLink = dtMainBanner.Rows[j]["Link"].ToString();
                                    else
                                        sopenUrlLink = "";

                                    objbaner.IntermediateBannerImages.Add(new BannerModel.IntermediateBannerImage
                                    {
                                        Title = sTitle,
                                        bannerURL = urlpath1 + ImageName1,
                                        bannerId = Id,
                                        ActionId = sActionId,
                                        action = sAction,
                                        categoryId = sCategoryId,
                                        categoryName = sCategoryName,
                                        openUrlLink = sopenUrlLink,
                                        ProductId = sProductId,
                                        ProductName = sProductName,
                                        MaxQty = sMaxQty,
                                        MinQty = sMinQty,
                                        IsQtyFreeze = sIsQtyFreeze,
                                        MRP = sMRP,
                                        Discount = sDiscount,
                                        SellingPrice = sSellingPrice,
                                        Weight = sWeight + sUnitName
                                    });

                                }
                            }
                            else
                            {
                                objbaner.response = "0";
                                objbaner.message = "Intermediate Banner Details Not Found";
                            }
                        }
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Intermediate Banner Details Not Found";
                }

                return objbaner;
            }
            catch (Exception ee)
            {
                objbaner.response = "-1";
                objbaner.message = "Something Wrong ";
                return objbaner;
            }
        }
    }
}
