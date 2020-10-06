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

                BannerModel.ProductAttributes attributeHomePagelist = new BannerModel.ProductAttributes();
                objbaner.HomePageBannerImages = new List<BannerModel.IntermediateBannerImage>();
                string condstr = "";
                if (JurisdictionId != "" && JurisdictionId != null)
                {
                    condstr = " and JB.JurisdictionId = " + JurisdictionId + " AND JB.BannerType = 'HomePage' ";
                }
                string querystr = "Select  ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty,ISNULL(PC.CategoryID,0) AS ProdCategoryId," +
                                     " ISNULL(PC.CategoryName,'') AS ProductCategoryName, ISNULL(P.MinQty,0) AS MinQty, " +
                                     " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                                     " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(Im.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                                     " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " +
                                     " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title, " +
                                     " ISNULL(PA.AttributeId,0) AS AttributeId" +
                                     " From HomepageBanner Im " +
                                     " Left join category cg on  cg.categoryId = Im.categoryId " +
                                     " Left join Product P on  P.Id = Im.ProductId " +
                                     " Left join category PC on PC.CategoryID = P.CategoryID " +
                                     " Left join UnitMaster U on U.Id = P.UnitId " +
                                     " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                                     " Left join (SELECT ProductId, ISNULL(ID,0) AS AttributeId  FROM Product_ProductAttribute_Mapping WHERE ISSelected = 1 ) PA on  P.Id = PA.ProductId " +
                                     " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate +
                                     "' and Im.StartDate<='" + startend + "'" +
                                     condstr +
                                     "  order by Im.Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);
                string Id = "", ImageName1 = "", sAction = "", sCategoryId = "", sCategoryName = "", sopenUrlLink = "";
                string sProductName = "", sUnitName = "", sWeight = "", sSellingPrice = "", sMRP = "", sDiscount = "";
                string sTitle = "";
                string sMaxQty = "", sMinQty = "", sAttributeId = "";
                int sActionId = 0, sProductId = 0;
                bool sIsQtyFreeze = false;

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
                        for (int n = 0; n < dtmain.Rows.Count; n++)
                        {
                            Id = dtmain.Rows[n]["Id"].ToString();
                            ImageName1 = dtmain.Rows[n]["ImageName"].ToString();
                            //sAction = dtmain.Rows[n]["Action"].ToString();
                            if (!String.IsNullOrEmpty(dtmain.Rows[n]["ActionId"].ToString()) || Convert.ToInt32(dtmain.Rows[n]["ActionId"]) > 0)
                            {
                                sActionId = Convert.ToInt32(dtmain.Rows[n]["ActionId"]);
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["MaxQty"]) > 0)
                            {
                                sMaxQty = dtmain.Rows[n]["MaxQty"].ToString();
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["MinQty"]) > 0)
                            {
                                sMinQty = dtmain.Rows[n]["MinQty"].ToString();
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["ProductId"]) > 0)
                            {
                                sProductId = Convert.ToInt32(dtmain.Rows[n]["ProductId"]);
                            }
                            sProductName = dtmain.Rows[n]["ProductName"].ToString();
                            sTitle = dtmain.Rows[n]["Title"].ToString();
                            if (Convert.ToInt32(dtmain.Rows[n]["IsQtyFreeze"]) > 0)
                            {
                                sIsQtyFreeze = Convert.ToBoolean(dtmain.Rows[n]["IsQtyFreeze"]);
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["MRP"]) > 0)
                            {
                                sMRP = dtmain.Rows[n]["MRP"].ToString();
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["Discount"]) > 0)
                            {
                                sDiscount = dtmain.Rows[n]["Discount"].ToString();
                            }
                            if (Convert.ToInt32(dtmain.Rows[n]["SellingPrice"]) > 0)
                            {
                                sSellingPrice = dtmain.Rows[n]["SellingPrice"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dtmain.Rows[n]["Weight"].ToString()) && Convert.ToInt32(dtmain.Rows[n]["Weight"]) > 0)
                            {
                                sWeight = dtmain.Rows[n]["Weight"].ToString();
                            }
                            if (!String.IsNullOrEmpty(dtmain.Rows[n]["UnitName"].ToString()))
                            {
                                sUnitName = "-" + dtmain.Rows[n]["UnitName"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dtmain.Rows[n]["CategoryID"].ToString()))
                            {
                                sCategoryId = dtmain.Rows[n]["CategoryID"].ToString();
                                sCategoryName = dtmain.Rows[n]["CategoryName"].ToString();
                            }
                            else
                            {
                                sCategoryId = "0";
                                sCategoryName = "";
                            }
                            if (!string.IsNullOrEmpty(dtmain.Rows[n]["Link"].ToString()))
                                sopenUrlLink = dtmain.Rows[n]["Link"].ToString();
                            else
                                sopenUrlLink = "";

                            if (Convert.ToInt32(dtmain.Rows[n]["AttributeId"]) > 0)
                            {
                                sAttributeId = dtmain.Rows[n]["AttributeId"].ToString();
                            }

                            BannerModel.IntermediateBannerImage objIntermediatebaner = new BannerModel.IntermediateBannerImage();
                            objIntermediatebaner.Title = sTitle;
                            objIntermediatebaner.bannerURL = urlpath + ImageName1;
                            objIntermediatebaner.bannerId = Id;
                            objIntermediatebaner.ActionId = sActionId;
                            objIntermediatebaner.action = "";
                            objIntermediatebaner.categoryId = sCategoryId;
                            objIntermediatebaner.categoryName = sCategoryName;
                            objIntermediatebaner.openUrlLink = sopenUrlLink;
                            objIntermediatebaner.ProductId = sProductId;
                            objIntermediatebaner.ProductName = sProductName;
                            objbaner.HomePageBannerImages.Add(objIntermediatebaner);

                            string Attribuepathimg = "";
                            string Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
                            DataTable dtAttrpathimg = dbc.GetDataTable(Attributedata);
                            if (dtAttrpathimg != null && dtAttrpathimg.Rows.Count > 0)
                            {
                                Attribuepathimg = dtAttrpathimg.Rows[0]["KeyValue"].ToString();
                            }

                            string AttImageDetails = "SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails,Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
                                             " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice,pam.PackingType,pam.ProductImage, " +
                                             " pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy,pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
                                             " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
                                             " FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                            DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);
                            List<BannerModel.ProductAttributes> objAttrList = new List<BannerModel.ProductAttributes>();
                            if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                            {
                                string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
                                sMaxQty = ""; sMinQty = "";
                                Boolean bAisOutOfStock = false, bAisSelected = false;
                                //List<BannerModel.IntermediateBannerImage.BannerProductAttribute> objProductAttribute = new List<BannerModel.BannerProductAttribute>();
                                for (int nCtr = 0; nCtr < dtAttdetails.Rows.Count; nCtr++)
                                {
                                    attributeHomePagelist = new BannerModel.ProductAttributes();
                                    sApackSizeId = dtAttdetails.Rows[nCtr]["Id"].ToString();
                                    sAMrp = dtAttdetails.Rows[nCtr]["Mrp"].ToString();
                                    sMinQty = dtAttdetails.Rows[nCtr]["MinQty"].ToString();
                                    sMaxQty = dtAttdetails.Rows[nCtr]["MaxQty"].ToString();


                                    sADiscount = dtAttdetails.Rows[nCtr]["Discount"].ToString();
                                    if (sADiscount.ToString() != "0")
                                    {
                                        if (dtAttdetails.Rows[nCtr]["DiscountType"].ToString() == "%")
                                            sAPDiscount = sADiscount.ToString() + "% Off";
                                        else if (dtAttdetails.Rows[nCtr]["DiscountType"].ToString() == "Fixed")
                                            sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
                                        else
                                            sAPDiscount = "";
                                    }
                                    else
                                        sAPDiscount = "";

                                    sAPackingType = dtAttdetails.Rows[nCtr]["PackingType"].ToString();
                                    sAsoshoPrice = dtAttdetails.Rows[nCtr]["SoshoPrice"].ToString();
                                    sAweight = dtAttdetails.Rows[nCtr]["DUnit"].ToString();
                                    sAImage = dtAttdetails.Rows[nCtr]["ProductImage"].ToString();
                                    if (dtAttdetails.Rows[nCtr]["isOutOfStock"].ToString() == "1")
                                        bAisOutOfStock = true;
                                    else
                                        bAisOutOfStock = false;

                                    sisSelected = dtAttdetails.Rows[nCtr]["isSelectedDetails"].ToString();
                                    sisbestbuy = dtAttdetails.Rows[nCtr]["IsBestBuy"].ToString();
                                    sisQtyFreeze = dtAttdetails.Rows[nCtr]["IsQtyFreeze"].ToString();

                                    attributeHomePagelist.Mrp = sAMrp;
                                    //attributelist.Discount = sADiscount;
                                    attributeHomePagelist.Discount = sAPDiscount;
                                    attributeHomePagelist.PackingType = sAPackingType;
                                    attributeHomePagelist.soshoPrice = sAsoshoPrice;
                                    attributeHomePagelist.weight = sAweight;
                                    attributeHomePagelist.AImageName = Attribuepathimg + sAImage;
                                    attributeHomePagelist.isOutOfStock = bAisOutOfStock.ToString();
                                    attributeHomePagelist.MinQty = sMinQty;
                                    attributeHomePagelist.MaxQty = sMaxQty;
                                    attributeHomePagelist.isSelected = sisSelected;
                                    attributeHomePagelist.isQtyFreeze = sisQtyFreeze;
                                    attributeHomePagelist.isBestBuy = sisbestbuy;
                                    attributeHomePagelist.AttributeId = sApackSizeId;
                                    objAttrList.Add(attributeHomePagelist);
                                }

                            }
                            objIntermediatebaner.ProductAttributesList = objAttrList;
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
                BannerModel.ProductAttributes attributelist = new BannerModel.ProductAttributes();
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
                            cond = " and JB.JurisdictionId = " + JurisdictionId + " AND JB.BannerType = 'Intermediate' ";
                        }
                            string qry = "Select  ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty,ISNULL(PC.CategoryID,0) AS ProdCategoryId," + 
                                     " ISNULL(PC.CategoryName,'') AS ProductCategoryName, ISNULL(P.MinQty,0) AS MinQty, " +
                                     " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                                     " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(Im.Action,'') AS Action, ISNULL(Im.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                                     " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " + 
                                     " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title, " +
                                     " ISNULL(PA.AttributeId,0) AS AttributeId" +
                                     " From IntermediateBanners Im " +
                                     " Left join category cg on  cg.categoryId = Im.categoryId " +
                                     " Left join Product P on  P.Id = Im.ProductId " +
                                     " Left join category PC on PC.CategoryID = P.CategoryID " +
                                     " Left join UnitMaster U on U.Id = P.UnitId " +
                                     " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                                     " Left join (SELECT ProductId, ISNULL(ID,0) AS AttributeId  FROM Product_ProductAttribute_Mapping WHERE ISSelected = 1 ) PA on  P.Id = PA.ProductId " +
                                     " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate + 
                                     "' and Im.StartDate<='" + startend + "' and TypeId = " + sTypeId + "" +
                                     cond + 
                                     "  order by Im.Id desc";
                        DataTable dtMainBanner = dbc.GetDataTable(qry);


                        Id = ""; ImageName1 = ""; sAction = ""; sCategoryId = ""; sCategoryName = ""; sopenUrlLink="";
                        sProductName = ""; sUnitName = ""; sWeight = ""; sSellingPrice = ""; sMRP = ""; sDiscount = "";
                        sTitle = "";
                        sMaxQty = ""; sMinQty = ""; sAttributeId = "";
                        sActionId = 0; sProductId = 0;
                        sIsQtyFreeze = false;
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
                                sMaxQty = ""; sMinQty = ""; sAttributeId = "";
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
                                    if (!string.IsNullOrEmpty(dtMainBanner.Rows[j]["Weight"].ToString()) && Convert.ToInt32(dtMainBanner.Rows[j]["Weight"]) > 0)
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

                                    if (Convert.ToInt32(dtMainBanner.Rows[j]["AttributeId"]) > 0)
                                    {
                                        sAttributeId = dtMainBanner.Rows[j]["AttributeId"].ToString();
                                    }

                                    BannerModel.IntermediateBannerImage objIntermediatebaner = new BannerModel.IntermediateBannerImage();
                                    objIntermediatebaner.Title = sTitle;
                                    objIntermediatebaner.bannerURL = urlpath1 + ImageName1;
                                    objIntermediatebaner.bannerId = Id;
                                    objIntermediatebaner.ActionId = sActionId;
                                    objIntermediatebaner.action = sAction;
                                    objIntermediatebaner.categoryId = sCategoryId;
                                    objIntermediatebaner.categoryName = sCategoryName;
                                    objIntermediatebaner.openUrlLink = sopenUrlLink;
                                    objIntermediatebaner.ProductId = sProductId;
                                    objIntermediatebaner.ProductName = sProductName;
                                    objbaner.IntermediateBannerImages.Add(objIntermediatebaner);

                                    string Attribuepathimg = "";
                                    string Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
                                    DataTable dtAttrpathimg = dbc.GetDataTable(Attributedata);
                                    if (dtAttrpathimg != null && dtAttrpathimg.Rows.Count > 0)
                                    {
                                        Attribuepathimg = dtAttrpathimg.Rows[0]["KeyValue"].ToString();
                                    }

                                    string AttImageDetails = "SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails,Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
                                                     " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice,pam.PackingType,pam.ProductImage, " +
                                                     " pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy,pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
                                                     " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
                                                     " FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                                    DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);
                                    List<BannerModel.ProductAttributes> objAttrList = new List<BannerModel.ProductAttributes>();
                                    if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                                    {
                                        string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                        string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
                                        sMaxQty = ""; sMinQty = "";
                                        Boolean bAisOutOfStock = false, bAisSelected = false;
                                        //List<BannerModel.IntermediateBannerImage.BannerProductAttribute> objProductAttribute = new List<BannerModel.BannerProductAttribute>();
                                        for (int jCtr = 0; jCtr < dtAttdetails.Rows.Count; jCtr++)
                                        {
                                            attributelist = new BannerModel.ProductAttributes();
                                            sApackSizeId = dtAttdetails.Rows[jCtr]["Id"].ToString();
                                            sAMrp = dtAttdetails.Rows[jCtr]["Mrp"].ToString();
                                            sMinQty = dtAttdetails.Rows[jCtr]["MinQty"].ToString();
                                            sMaxQty = dtAttdetails.Rows[jCtr]["MaxQty"].ToString();


                                            sADiscount = dtAttdetails.Rows[jCtr]["Discount"].ToString();
                                            if (sADiscount.ToString() != "0")
                                            {
                                                if (dtAttdetails.Rows[jCtr]["DiscountType"].ToString() == "%")
                                                    sAPDiscount = sADiscount.ToString() + "% Off";
                                                else if (dtAttdetails.Rows[jCtr]["DiscountType"].ToString() == "Fixed")
                                                    sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
                                                else
                                                    sAPDiscount = "";
                                            }
                                            else
                                                sAPDiscount = "";

                                            sAPackingType = dtAttdetails.Rows[jCtr]["PackingType"].ToString();
                                            sAsoshoPrice = dtAttdetails.Rows[jCtr]["SoshoPrice"].ToString();
                                            sAweight = dtAttdetails.Rows[jCtr]["DUnit"].ToString();
                                            sAImage = dtAttdetails.Rows[jCtr]["ProductImage"].ToString();
                                            if (dtAttdetails.Rows[jCtr]["isOutOfStock"].ToString() == "1")
                                                bAisOutOfStock = true;
                                            else
                                                bAisOutOfStock = false;

                                            sisSelected = dtAttdetails.Rows[jCtr]["isSelectedDetails"].ToString();
                                            sisbestbuy = dtAttdetails.Rows[jCtr]["IsBestBuy"].ToString();
                                            sisQtyFreeze = dtAttdetails.Rows[jCtr]["IsQtyFreeze"].ToString();

                                            attributelist.Mrp = sAMrp;
                                            //attributelist.Discount = sADiscount;
                                            attributelist.Discount = sAPDiscount;
                                            attributelist.PackingType = sAPackingType;
                                            attributelist.soshoPrice = sAsoshoPrice;
                                            attributelist.weight = sAweight;
                                            attributelist.AImageName = Attribuepathimg + sAImage;
                                            attributelist.isOutOfStock = bAisOutOfStock.ToString();
                                            attributelist.MinQty = sMinQty;
                                            attributelist.MaxQty = sMaxQty;
                                            attributelist.isSelected = sisSelected;
                                            attributelist.isQtyFreeze = sisQtyFreeze;
                                            attributelist.isBestBuy = sisbestbuy;
                                            attributelist.AttributeId = sApackSizeId;
                                            objAttrList.Add(attributelist);
                                        }

                                    }
                                    objIntermediatebaner.ProductAttributesList = objAttrList;



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
