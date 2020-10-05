using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Http;
using Test0555.Models.ProductManagement;
namespace Test0555.Controllers
{
    public class ProductController : ApiController
    {
        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        public enum FilterRate
        {
            LowToHigh = 1,
            HighToLow = 2,
            Discount = 3,
            SoshoRecommended = 4
        }

        [HttpGet]
        //02-10-2020 Developed By :- Vidhi Doshi
        public ProductModel.getNewproduct GetDashBoardProductDetails(string JurisdictionID, string CategoryId = "", string ProductId = "", string StartNo = "", string EndNo = "", string Filter = "")
        {
            ProductModel.getNewproduct objeprodt = new ProductModel.getNewproduct();
            try
            {
                int iBannerPosition = (ConfigurationManager.AppSettings["BannerPosition"] != null && ConfigurationManager.AppSettings["BannerPosition"].Trim() != "") ? Convert.ToInt16(ConfigurationManager.AppSettings["BannerPosition"].Trim()) : 0;
                EndNo = (iBannerPosition + 1).ToString();
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                //objeprodt.HomePageBannerImages = new List<ProductModel.HomePageBannerImage>();
                objeprodt.BannerPosition = iBannerPosition.ToString();
                objeprodt.ProductList = new List<ProductModel.NewProductDataList>();
                ProductModel.ProductAttributelist attributeHomePagelist = new ProductModel.ProductAttributelist();

                ProductModel.NewProductDataList objHomepageBanner = new ProductModel.NewProductDataList();

                string condstr = "";
                if (JurisdictionID != "" && JurisdictionID != null)
                {
                    condstr = " and JB.JurisdictionId = " + JurisdictionID + " AND JB.BannerType = 'HomePage' ";
                }
                string querystr = " Select ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty,ISNULL(PC.CategoryID,0) AS ProdCategoryId," +
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
                string Id = "", ImageName1 = "", sAction = "", bCategoryId = "", sCategoryName = "", sopenUrlLink = "";
                string sProductName = "", sUnitName = "", sWeight = "", sSellingPrice = "", sMRP = "", bDiscount = "";
                string sTitle = "";
                string bMaxQty = "", bMinQty = "", sAttributeId = "";
                int sActionId = 0, bProductId = 0;
                bool sIsQtyFreeze = false;
                if (ProductId == "0" || ProductId == "")
                {
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                        DataTable dtpath = dbc.GetDataTable(querydata);
                        if (dtpath != null && dtpath.Rows.Count > 0)
                        {

                            string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                            for (int n = 0; n < dtmain.Rows.Count; n++)
                            {
                                Id = dtmain.Rows[n]["Id"].ToString();
                                ImageName1 = dtmain.Rows[n]["ImageName"].ToString();
                                if (!String.IsNullOrEmpty(dtmain.Rows[n]["ActionId"].ToString()) || Convert.ToInt32(dtmain.Rows[n]["ActionId"]) > 0)
                                {
                                    sActionId = Convert.ToInt32(dtmain.Rows[n]["ActionId"]);
                                }
                                if (Convert.ToInt32(dtmain.Rows[n]["MaxQty"]) > 0)
                                {
                                    bMaxQty = dtmain.Rows[n]["MaxQty"].ToString();
                                }
                                if (Convert.ToInt32(dtmain.Rows[n]["MinQty"]) > 0)
                                {
                                    bMinQty = dtmain.Rows[n]["MinQty"].ToString();
                                }
                                if (Convert.ToInt32(dtmain.Rows[n]["ProductId"]) > 0)
                                {
                                    bProductId = Convert.ToInt32(dtmain.Rows[n]["ProductId"]);
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
                                    bDiscount = dtmain.Rows[n]["Discount"].ToString();
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
                                    bCategoryId = dtmain.Rows[n]["CategoryID"].ToString();
                                    sCategoryName = dtmain.Rows[n]["CategoryName"].ToString();
                                }
                                else
                                {
                                    bCategoryId = "0";
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


                                //ProductModel.HomePageBannerImage objHomePagebaner = new ProductModel.HomePageBannerImage();
                                //objHomePagebaner.Title = sTitle;
                                //objHomePagebaner.bannerURL = urlpath + ImageName1;
                                //objHomePagebaner.bannerId = Id;
                                //objHomePagebaner.ActionId = sActionId;
                                //objHomePagebaner.action = "";
                                //objHomePagebaner.categoryId = bCategoryId;
                                //objHomePagebaner.categoryName = sCategoryName;
                                //objHomePagebaner.openUrlLink = sopenUrlLink;
                                //objHomePagebaner.ProductId = bProductId;
                                //objHomePagebaner.ProductName = sProductName;
                                //objHomepageBanner.Add(objHomePagebaner);

                                ProductModel.NewProductDataList objHomePagebaner = new ProductModel.NewProductDataList();
                                objHomePagebaner.ItemType = "2";
                                objHomePagebaner.Title = sTitle;
                                objHomePagebaner.bannerId = Id;
                                objHomePagebaner.bannerURL = urlpath + ImageName1;
                                objHomePagebaner.ActionId = sActionId;
                                objHomePagebaner.action = sAction;
                                objHomePagebaner.CategoryId = bCategoryId;
                                objHomePagebaner.CategoryName = sCategoryName;
                                objHomePagebaner.openUrlLink = sopenUrlLink;
                                objHomePagebaner.ProductId = bProductId.ToString();
                                objHomePagebaner.ProductName = sProductName;
                                objHomePagebaner.OfferEndDate = "";
                                objHomePagebaner.SoldCount = "";
                                objHomePagebaner.SpecialMessage = "";
                                objHomePagebaner.DisplayOrder = 0;
                                objHomePagebaner.SoshoRecommended = "";
                                objHomePagebaner.IsSoshoRecommended = false;
                                objHomePagebaner.IsSpecialMessage = false;
                                objHomePagebaner.IsProductDescription = false;
                                objHomePagebaner.ProductDescription = "";
                                objHomePagebaner.ProductNotes = "";
                                objHomePagebaner.ProductKeyFeatures = "";
                                objHomePagebaner.isFreeShipping = false;
                                objHomePagebaner.isFixedShipping = false;
                                objHomePagebaner.FixedShipRate = 0;

                                objeprodt.ProductList.Add(objHomePagebaner);


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
                                                 " FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + bProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                                DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);
                                List<ProductModel.ProductAttributelist> objAttrList = new List<ProductModel.ProductAttributelist>();
                                if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                                {
                                    string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                    string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
                                    bMaxQty = ""; bMinQty = "";
                                    Boolean bAisOutOfStock = false;
                                    for (int nCtr = 0; nCtr < dtAttdetails.Rows.Count; nCtr++)
                                    {
                                        attributeHomePagelist = new ProductModel.ProductAttributelist();
                                        sApackSizeId = dtAttdetails.Rows[nCtr]["Id"].ToString();
                                        sAMrp = dtAttdetails.Rows[nCtr]["Mrp"].ToString();
                                        bMinQty = dtAttdetails.Rows[nCtr]["MinQty"].ToString();
                                        bMaxQty = dtAttdetails.Rows[nCtr]["MaxQty"].ToString();


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

                                        attributeHomePagelist.Mrp = Convert.ToDouble(sAMrp);
                                        attributeHomePagelist.Discount = sAPDiscount;
                                        attributeHomePagelist.PackingType = sAPackingType;
                                        attributeHomePagelist.soshoPrice = Convert.ToDouble(sAsoshoPrice);
                                        attributeHomePagelist.weight = sAweight;
                                        attributeHomePagelist.AImageName = Attribuepathimg + sAImage;
                                        attributeHomePagelist.isOutOfStock = Convert.ToBoolean(bAisOutOfStock);
                                        attributeHomePagelist.MinQty = Convert.ToInt32(bMinQty);
                                        attributeHomePagelist.MaxQty = Convert.ToInt32(bMaxQty);
                                        attributeHomePagelist.isSelected = Convert.ToBoolean(sisSelected);
                                        attributeHomePagelist.isQtyFreeze = Convert.ToBoolean(sisQtyFreeze);
                                        attributeHomePagelist.isBestBuy = Convert.ToBoolean(sisbestbuy);
                                        attributeHomePagelist.AttributeId = sApackSizeId;
                                        objAttrList.Add(attributeHomePagelist);
                                    }

                                }
                                objHomePagebaner.ProductAttributesList = objAttrList;
                            }
                        }

                    }
                }

                string sWhatappNo = "";
                string subquerystr = "select isnull(Mobile,'') as Mobile From users where JurisdictionID =  " + JurisdictionID;
                DataTable dtsubmain = dbc.GetDataTable(subquerystr);
                if (dtsubmain != null && dtsubmain.Rows.Count > 0)
                {
                    sWhatappNo = dtsubmain.Rows[0]["Mobile"].ToString();
                }
                string querymain = " with pte as ( select TOP " + iBannerPosition + " ROW_NUMBER() over(order by convert(int,Isnull(Product.DisplayOrder,'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                querymain += "(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,";
                querymain += " Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,";
                querymain += "CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1, ";
                querymain += " isnull(cat.CategoryName, '') as CategoryName,isnull(Name, '') as [Name],isnull(sold, '') as [sold],isnull(ProductBanner, '') as ProductBanner,";
                querymain += " isnull(DisplayOrder, '') as DisplayOrder,isnull(Recommended,'') as Recommended,isnull(ProductDiscription,'') as ProductDiscription,isnull(Note,'') as Note,isnull(KeyFeatures,'') as KeyFeatures,";
                querymain += " case when isnull(ProductDiscription,'') = '' then 'false' else 'true' end as IsProductDetails,";
                querymain += " case when isnull(ProductTemplateID,'') = '2' then 'true' else 'false' end as Productvariant,";
                querymain += " case when isnull(Recommended,'') = '' then 'false' else 'true' end as IsSoshoRecommended,";
                querymain += "case when isnull(ProductBanner,'') = '' then 'false' else 'true' end as IsSpecialMessage ";
                querymain += "  ,Product.CategoryID,ProductMRP AS mrp,Isnull(cast(cast(discount as decimal(10,2)) AS FLOAT),'') AS discount,DiscountType,SoshoPrice,MaxQty,MinQty,case when isnull(IsProductDescription,'') = '1' then 'true' else 'false' end as IsProductDescription ";
                querymain += " ,case when isnull(IsFreeShipping,'') = '' then 'false' else 'true' end as IsFreeShipping ,Product.SubCategoryID  ";
                querymain += " ,case when isnull(IsFixedShipping,'') = '' then 'false' else 'true' end as IsFixedShipping, FixedShipRate,isnull(Scat.SubCategory, '') as SubCategoryName ";
                querymain += " from Product ";
                querymain += " inner join Unitmaster on Unitmaster.id=Product.UnitId ";
                querymain += " inner join Category cat on cat.CategoryID = Product.CategoryID ";
                querymain += " inner join tblSubCategory Scat on Scat.Id = Product.SubCategoryID ";
                querymain += " Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                querymain += "and Product.IsActive = 1 and Product.IsDeleted = 0 and Isnull(Product.IsApproved,'') = 1 and Product.JurisdictionID =" + JurisdictionID;
                if (!string.IsNullOrEmpty(CategoryId))
                {
                    querymain += " and Product.CategoryID =" + CategoryId;
                }
                if (!string.IsNullOrEmpty(ProductId))
                {
                    querymain += " and Product.Id >" + ProductId;
                }
                if (!string.IsNullOrEmpty(Filter) && Filter == FilterRate.SoshoRecommended.ToString())
                {
                    querymain += " and ISNULL(Recommended,'') != '' ";
                }
                if (!string.IsNullOrEmpty(Filter) && Filter == FilterRate.Discount.ToString())
                {
                    querymain += " and ISNULL(Discount,0) > 0 ";
                }
                if (!string.IsNullOrEmpty(Filter) && Filter== FilterRate.LowToHigh.ToString())
                {
                    querymain += " Order by Product.SoshoPrice ";
                }
                if (!string.IsNullOrEmpty(Filter) && Filter == FilterRate.HighToLow.ToString())
                {
                    querymain += " Order by Product.SoshoPrice DESC ";
                }
               
                querymain += " ) select * From pte where RowNumber between " + StartNo + " and " + EndNo;
                DataTable dtproduct = dbc.GetDataTable(querymain);

                int nPosCtr = 0;
                if (dtproduct != null && dtproduct.Rows.Count > 0)
                {
                    
                    string querydata = "select KeyValue from StringResources where KeyName='ProductImageUrl'";
                    DataTable dtpathimg = dbc.GetDataTable(querydata);
                    string urlpathimg = "", Attribuepathimg = "";
                    //string urlpathvid = "";
                    if (dtpathimg != null && dtpathimg.Rows.Count > 0)
                    {
                        //Image Path

                        urlpathimg = dtpathimg.Rows[0]["KeyValue"].ToString();
                    }
                    string Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
                    DataTable dtAttrpathimg = dbc.GetDataTable(Attributedata);
                    if (dtAttrpathimg != null && dtAttrpathimg.Rows.Count > 0)
                    {
                        //Image Path

                        Attribuepathimg = dtAttrpathimg.Rows[0]["KeyValue"].ToString();
                    }

                    objeprodt.response = "1";
                    objeprodt.message = "Successfully";
                    objeprodt.WhatsAppNo = sWhatappNo;
                    

                    string sProductId = "", sMrp = "", sDiscount = "", sEdate = "", sPname = "", sPDiscount = "", sSoshoPrice = "", sSold = "", sProductBanner = "";
                    string sDUnit = "", sDisplayOrder = "", sMaxQty = "", sMinQty = "", sCategoryId = "", sCategory = "", sProductvariant = "", sIsSoshoRecommended = "";
                    string sIsSpecialMessage = "", sProductDiscription = "", sIsProductDetails = "", sRecommended = "", sProductNotes = "", sProductKeyFeatures = "", sIsProductDescription = "";
                    string sisFreeShipping = "", sisFixedShipping = "", sFixedShipRate = "", sSubCategoryId = "", sSubCategory = "";
                    decimal dDiscount = 0;
                    Boolean bIsQtyFreeze = false;
                    int prodrowCount = dtproduct.Rows.Count;
                    
                    for (int i = 0; i < dtproduct.Rows.Count; i++)
                    {
                        nPosCtr++;
                        sProductId = dtproduct.Rows[i]["Pid"].ToString();

                        ProductModel.NewProductDataList objProduct = new ProductModel.NewProductDataList();
                        ProductModel.ProductDataImagelist dataImagelist = new ProductModel.ProductDataImagelist();
                        ProductModel.ProductAttributelist attributelist = new ProductModel.ProductAttributelist();

                        //if (urlpathimg != "")
                        //{

                        //    string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + sProductId;
                        //    DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                        //    if (dtdetails != null && dtdetails.Rows.Count > 0)
                        //    {
                        //        string productid3 = sProductId;
                        //        string proimgid = dtdetails.Rows[0]["id"].ToString();
                        //        string Imagename = dtdetails.Rows[0]["ImageFileName"].ToString();
                        //        string pdisorder = dtdetails.Rows[0]["DisplayOrder"].ToString();

                        //        dataImagelist.proimagid = proimgid;
                        //        dataImagelist.PImgname = urlpathimg + Imagename;
                        //        dataImagelist.prodid = sProductId;
                        //        dataImagelist.PDisOrder = pdisorder;
                        //        objProduct.ProductImageList.Add(dataImagelist);
                        //    }
                        //}
                        if (Attribuepathimg != "")
                        {
                            string AttImageDetails = "SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails,Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
                                                     " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice,pam.PackingType,pam.ProductImage, " +
                                                     " pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy,pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
                                                     " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
                                                     " FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                            DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);

                            if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                            {
                                string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
                                sMaxQty = ""; sMinQty = "";
                                Boolean bAisOutOfStock = false;
                                for (int j = 0; j < dtAttdetails.Rows.Count; j++)
                                {
                                    attributelist = new ProductModel.ProductAttributelist();
                                    sApackSizeId = dtAttdetails.Rows[j]["Id"].ToString();
                                    sAMrp = dtAttdetails.Rows[j]["Mrp"].ToString();
                                    sMinQty = dtAttdetails.Rows[j]["MinQty"].ToString();
                                    sMaxQty = dtAttdetails.Rows[j]["MaxQty"].ToString();


                                    sADiscount = dtAttdetails.Rows[j]["Discount"].ToString();
                                    if (sADiscount.ToString() != "0")
                                    {
                                        if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "%")
                                            sAPDiscount = sADiscount.ToString() + "% Off";
                                        else if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "Fixed")
                                            sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
                                        else
                                            sAPDiscount = "";
                                    }
                                    else
                                        sAPDiscount = "";

                                    sAPackingType = dtAttdetails.Rows[j]["PackingType"].ToString();
                                    sAsoshoPrice = dtAttdetails.Rows[j]["SoshoPrice"].ToString();
                                    sAweight = dtAttdetails.Rows[j]["DUnit"].ToString();
                                    sAImage = dtAttdetails.Rows[j]["ProductImage"].ToString();
                                    if (dtAttdetails.Rows[j]["isOutOfStock"].ToString() == "1")
                                        bAisOutOfStock = true;
                                    else
                                        bAisOutOfStock = false;

                                    sisSelected = dtAttdetails.Rows[j]["isSelectedDetails"].ToString();
                                    sisbestbuy = dtAttdetails.Rows[j]["IsBestBuy"].ToString();
                                    sisQtyFreeze = dtAttdetails.Rows[j]["IsQtyFreeze"].ToString();

                                    attributelist.Mrp = Convert.ToDouble(sAMrp);
                                    attributelist.Discount = sAPDiscount;
                                    attributelist.PackingType = sAPackingType;
                                    attributelist.soshoPrice = Convert.ToDouble(sAsoshoPrice);
                                    attributelist.weight = sAweight;
                                    attributelist.AImageName = Attribuepathimg + sAImage;
                                    attributelist.isOutOfStock = Convert.ToBoolean(bAisOutOfStock);
                                    attributelist.MinQty = Convert.ToInt32(sMinQty);
                                    attributelist.MaxQty = Convert.ToInt32(sMaxQty);
                                    attributelist.isSelected = Convert.ToBoolean(sisSelected);
                                    attributelist.isQtyFreeze = Convert.ToBoolean(sisQtyFreeze);
                                    attributelist.isBestBuy = Convert.ToBoolean(sisbestbuy);
                                    attributelist.AttributeId = sApackSizeId;
                                    objProduct.ProductAttributesList.Add(attributelist);
                                }

                            }
                        }
                        sCategoryId = dtproduct.Rows[i]["CategoryId"].ToString();
                        sCategory = dtproduct.Rows[i]["CategoryName"].ToString();

                        if (sDiscount.ToString() != "0")
                        {
                            if (dtproduct.Rows[i]["DiscountType"].ToString() == "%")
                                sPDiscount = sDiscount.ToString() + "% Off";
                            else if (dtproduct.Rows[i]["DiscountType"].ToString() == "Fixed")
                                sPDiscount = CommonString.rusymbol + " " + sDiscount.ToString() + " Off";
                            else
                                sPDiscount = "";
                        }
                        else
                            sPDiscount = "";

                        dDiscount = 0;
                        decimal.TryParse(sDiscount.ToString(), out dDiscount);

                        sPname = dtproduct.Rows[i]["Name"].ToString();
                        sEdate = dtproduct.Rows[i]["edate"].ToString();
                        sSold = dtproduct.Rows[i]["sold"].ToString();
                        sProductBanner = dtproduct.Rows[i]["ProductBanner"].ToString();
                        sDisplayOrder = dtproduct.Rows[i]["DisplayOrder"].ToString();
                        sIsSoshoRecommended = dtproduct.Rows[i]["IsSoshoRecommended"].ToString();
                        sIsSpecialMessage = dtproduct.Rows[i]["IsSpecialMessage"].ToString();
                        sIsProductDescription = dtproduct.Rows[i]["IsProductDescription"].ToString();
                        sRecommended = dtproduct.Rows[i]["Recommended"].ToString();
                        sProductDiscription = dtproduct.Rows[i]["ProductDiscription"].ToString();
                        sProductNotes = dtproduct.Rows[i]["Note"].ToString();
                        sProductKeyFeatures = dtproduct.Rows[i]["KeyFeatures"].ToString();
                        sisFreeShipping = dtproduct.Rows[i]["IsFreeShipping"].ToString();
                        sisFixedShipping = dtproduct.Rows[i]["IsFixedShipping"].ToString();
                        sFixedShipRate = dtproduct.Rows[i]["FixedShipRate"].ToString();

                        objProduct.ItemType = "1";
                        objProduct.CategoryId = sCategoryId;
                        objProduct.CategoryName = sCategory;
                        objProduct.OfferEndDate = sEdate;
                        objProduct.SoldCount = sSold;
                        objProduct.SpecialMessage = sProductBanner;
                        objProduct.DisplayOrder = Convert.ToInt32(sDisplayOrder);
                        objProduct.SoshoRecommended = sRecommended;
                        objProduct.IsSoshoRecommended = Convert.ToBoolean(sIsSoshoRecommended);
                        objProduct.IsSpecialMessage = Convert.ToBoolean(sIsSpecialMessage);
                        objProduct.IsProductDescription = Convert.ToBoolean(sIsProductDescription);
                        objProduct.ProductDescription = sProductDiscription;
                        objProduct.ProductNotes = sProductNotes;
                        objProduct.ProductKeyFeatures = sProductKeyFeatures;
                        objProduct.ProductId = sProductId;
                        objProduct.isFreeShipping = Convert.ToBoolean(sisFreeShipping);
                        objProduct.isFixedShipping = Convert.ToBoolean(sisFixedShipping);
                        objProduct.FixedShipRate = Convert.ToDouble(sFixedShipRate);
                        objProduct.ProductName = "";
                        objProduct.Title = "";
                        objProduct.bannerId = "0";
                        objProduct.bannerURL = "";
                        objProduct.ActionId = 0;
                        objProduct.action = "";
                        objProduct.openUrlLink = "";
                        objeprodt.ProductList.Add(objProduct);

                    }

                    string cond = string.Empty;
                    if (JurisdictionID != "" && JurisdictionID != null)
                    {
                        cond = " and JB.JurisdictionId = " + JurisdictionID + " AND JB.BannerType = 'Intermediate' ";
                    }
                    string qry = "Select  ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty,ISNULL(PC.CategoryID,0) AS ProdCategoryId," +
                             " ISNULL(PC.CategoryName,'') AS ProductCategoryName, ISNULL(P.MinQty,0) AS MinQty, " +
                             " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                             " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(Im.Action,'') AS Action, ISNULL(Im.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                             " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " +
                             " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title, " +
                             " ISNULL(PA.AttributeId,0) AS AttributeId, " +
                             " ISNULL(P.SubCategoryId,0) AS SubCategoryId,ISNULL(SC.SubCategory,'') AS SubCategoryName " +
                             " From IntermediateBanners Im " +
                             " Left join category cg on  cg.categoryId = Im.categoryId " +
                             " Left join Product P on  P.Id = Im.ProductId " +
                             " Left join category PC on PC.CategoryID = P.CategoryID " +
                             " Left join tblSubCategory SC on SC.Id = P.SubCategoryId  " +
                             " Left join UnitMaster U on U.Id = P.UnitId " +
                             " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                             " Left join (SELECT ProductId, ISNULL(ID,0) AS AttributeId  FROM Product_ProductAttribute_Mapping WHERE ISSelected = 1 ) PA on  P.Id = PA.ProductId " +
                             " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate +
                             "' and Im.StartDate<='" + startend + "'" +
                             cond +
                             "  order by Im.Id desc";
                    DataTable dtInterBanner = dbc.GetDataTable(qry);
                    ProductModel.NewProductDataList  objBannerProduct = new ProductModel.NewProductDataList();
                    if (dtInterBanner != null && dtInterBanner.Rows.Count > 0)
                    {
                        nPosCtr = 0;
                        for (int j = 0; j < dtInterBanner.Rows.Count; j++)
                        {
                            sAction = dtInterBanner.Rows[j]["Action"].ToString();
                            if (!String.IsNullOrEmpty(dtInterBanner.Rows[j]["ActionId"].ToString()) || Convert.ToInt32(dtInterBanner.Rows[j]["ActionId"]) > 0)
                            {
                                sActionId = Convert.ToInt32(dtInterBanner.Rows[j]["ActionId"]);
                            }
                            if (!string.IsNullOrEmpty(dtInterBanner.Rows[j]["Link"].ToString()))
                                sopenUrlLink = dtInterBanner.Rows[j]["Link"].ToString();
                            else
                                sopenUrlLink = "";

                            if (Convert.ToInt32(dtInterBanner.Rows[j]["AttributeId"]) > 0)
                            {
                                sAttributeId = dtInterBanner.Rows[j]["AttributeId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dtInterBanner.Rows[j]["CategoryID"].ToString()))
                            {
                                sCategoryId = dtInterBanner.Rows[j]["CategoryID"].ToString();
                                sCategoryName = dtInterBanner.Rows[j]["CategoryName"].ToString();
                            }
                            else
                            {
                                sCategoryId = "0";
                                sCategoryName = "";
                            }
                            if (Convert.ToInt32(dtInterBanner.Rows[j]["ProductId"]) > 0)
                            {
                                sProductId = Convert.ToInt32(dtInterBanner.Rows[j]["ProductId"]).ToString();
                            }
                            sProductName = dtInterBanner.Rows[j]["ProductName"].ToString();

                            if (!string.IsNullOrEmpty(dtInterBanner.Rows[j]["Link"].ToString()))
                                sopenUrlLink = dtInterBanner.Rows[j]["Link"].ToString();
                            else
                                sopenUrlLink = "";

                            sTitle = dtInterBanner.Rows[j]["Title"].ToString();
                            Id = dtInterBanner.Rows[j]["Id"].ToString();
                            querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                            DataTable dtpath = dbc.GetDataTable(querydata);
                            string urlpath = string.Empty;
                            ImageName1 = dtInterBanner.Rows[j]["ImageName"].ToString();

                            if (dtpath != null && dtpath.Rows.Count > 0)
                            {
                                urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                            }
                            ProductModel.NewProductDataList objIntermediateBanner = new ProductModel.NewProductDataList();
                            objIntermediateBanner.ItemType = "3";
                            objIntermediateBanner.Title = sTitle;
                            objIntermediateBanner.bannerId = Id;
                            objIntermediateBanner.bannerURL = urlpath + ImageName1;
                            objIntermediateBanner.ActionId = sActionId;
                            objIntermediateBanner.action = sAction;
                            objIntermediateBanner.CategoryId = sCategoryId;
                            objIntermediateBanner.CategoryName = sCategoryName;
                            objIntermediateBanner.openUrlLink = sopenUrlLink;
                            objIntermediateBanner.ProductId = sProductId;
                            objIntermediateBanner.ProductName = sProductName;
                            objIntermediateBanner.OfferEndDate = "";
                            objIntermediateBanner.SoldCount = "";
                            objIntermediateBanner.SpecialMessage = "";
                            objIntermediateBanner.DisplayOrder = 0;
                            objIntermediateBanner.SoshoRecommended = "";
                            objIntermediateBanner.IsSoshoRecommended = false;
                            objIntermediateBanner.IsSpecialMessage = false;
                            objIntermediateBanner.IsProductDescription = false;
                            objIntermediateBanner.ProductDescription = "";
                            objIntermediateBanner.ProductNotes = "";
                            objIntermediateBanner.ProductKeyFeatures = "";
                            objIntermediateBanner.isFreeShipping = false;
                            objIntermediateBanner.isFixedShipping = false;
                            objIntermediateBanner.FixedShipRate = 0;

                            objeprodt.ProductList.Add(objIntermediateBanner);


                            Attribuepathimg = "";
                            Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
                            dtAttrpathimg = dbc.GetDataTable(Attributedata);
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
                            List<ProductModel.ProductAttributelist> objAttrList = new List<ProductModel.ProductAttributelist>();
                            if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                            {
                                string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
                                sMaxQty = ""; sMinQty = "";
                                Boolean bAisOutOfStock = false;
                                //List<BannerModel.IntermediateBannerImage.BannerProductAttribute> objProductAttribute = new List<BannerModel.BannerProductAttribute>();
                                for (int nCtr = 0; nCtr < dtAttdetails.Rows.Count; nCtr++)
                                {
                                    attributeHomePagelist = new ProductModel.ProductAttributelist();
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

                                    attributeHomePagelist.Mrp = Convert.ToDouble(sAMrp);
                                    attributeHomePagelist.Discount = sAPDiscount;
                                    attributeHomePagelist.PackingType = sAPackingType;
                                    attributeHomePagelist.soshoPrice = Convert.ToDouble(sAsoshoPrice);
                                    attributeHomePagelist.weight = sAweight;
                                    attributeHomePagelist.AImageName = Attribuepathimg + sAImage;
                                    attributeHomePagelist.isOutOfStock = Convert.ToBoolean(bAisOutOfStock.ToString());
                                    attributeHomePagelist.MinQty = Convert.ToInt32(sMinQty);
                                    attributeHomePagelist.MaxQty = Convert.ToInt32(sMaxQty);
                                    attributeHomePagelist.isSelected = Convert.ToBoolean(sisSelected);
                                    attributeHomePagelist.isQtyFreeze = Convert.ToBoolean(sisQtyFreeze);
                                    attributeHomePagelist.isBestBuy = Convert.ToBoolean(sisbestbuy);
                                    attributeHomePagelist.AttributeId = sApackSizeId;
                                    objAttrList.Add(attributeHomePagelist);
                                }

                            }
                            objBannerProduct.ProductAttributesList = objAttrList;
                            break;
                        }
                    }
                }
                else
                {
                    objeprodt.response = "0";
                    objeprodt.message = "Product Details Not Found";
                    objeprodt.WhatsAppNo = "";

                }
                return objeprodt;
            }
            catch (Exception ex)
            {
                objeprodt.response = CommonString.Errorresponse;
                objeprodt.message = ex.StackTrace + " " + ex.Message;
                return objeprodt;
            }
        }

        //[HttpGet]
        ////20-08-2020 Developed By :- Hiren
        //public ProductModel.getNewproduct GetDashBoardProductDetailsOld(string JurisdictionID, string CategoryId = "", string StartNo = "", string EndNo = "")
        //{
        //    ProductModel.getNewproduct objeprodt = new ProductModel.getNewproduct();

        //    try
        //    {

        //        string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
        //        string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

        //        string sWhatappNo = "";
        //        string subquerystr = "select isnull(Mobile,'') as Mobile From users where JurisdictionID =  " + JurisdictionID;
        //        DataTable dtsubmain = dbc.GetDataTable(subquerystr);
        //        if (dtsubmain != null && dtsubmain.Rows.Count > 0)
        //        {
        //            sWhatappNo = dtsubmain.Rows[0]["Mobile"].ToString();
        //        }
        //        string querymain = " with pte as ( select ROW_NUMBER() over(order by convert(int,Isnull(Product.DisplayOrder,'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
        //        querymain += "(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,";
        //        querymain += " Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,";
        //        querymain += "CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1, ";
        //        querymain += " isnull(cat.CategoryName, '') as CategoryName,isnull(Name, '') as [Name],isnull(sold, '') as [sold],isnull(ProductBanner, '') as ProductBanner,";
        //        querymain += " isnull(DisplayOrder, '') as DisplayOrder,isnull(Recommended,'') as Recommended,isnull(ProductDiscription,'') as ProductDiscription,isnull(Note,'') as Note,isnull(KeyFeatures,'') as KeyFeatures,";
        //        querymain += " case when isnull(ProductDiscription,'') = '' then 'false' else 'true' end as IsProductDetails,";
        //        querymain += " case when isnull(ProductTemplateID,'') = '2' then 'true' else 'false' end as Productvariant,";
        //        querymain += " case when isnull(Recommended,'') = '' then 'false' else 'true' end as IsSoshoRecommended,";
        //        querymain += "case when isnull(ProductBanner,'') = '' then 'false' else 'true' end as IsSpecialMessage ";
        //        querymain += "  ,Product.CategoryID,ProductMRP AS mrp,Isnull(cast(cast(discount as decimal(10,2)) AS FLOAT),'') AS discount,DiscountType,SoshoPrice,MaxQty,MinQty,case when isnull(IsProductDescription,'') = '1' then 'true' else 'false' end as IsProductDescription ";
        //        querymain += " ,case when isnull(IsFreeShipping,'') = '' then 'false' else 'true' end as IsFreeShipping ,Product.SubCategoryID  ";
        //        querymain += " ,case when isnull(IsFixedShipping,'') = '' then 'false' else 'true' end as IsFixedShipping, FixedShipRate,isnull(Scat.SubCategory, '') as SubCategoryName ";
        //        querymain += " from Product ";
        //        querymain += " inner join Unitmaster on Unitmaster.id=Product.UnitId ";
        //        querymain += " inner join Category cat on cat.CategoryID = Product.CategoryID ";
        //        querymain += " inner join tblSubCategory Scat on Scat.Id = Product.SubCategoryID ";
        //        querymain += " Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
        //        querymain += "and Product.IsActive = 1 and Product.IsDeleted = 0 and Isnull(Product.IsApproved,'') = 1 and Product.JurisdictionID =" + JurisdictionID;
        //        if (!string.IsNullOrEmpty(CategoryId))
        //        {
        //            querymain += " and Product.CategoryID =" + CategoryId;
        //        }
        //        querymain += " ) select * From pte where RowNumber between " + StartNo + " and " + EndNo;
        //        //querymain += "Order By convert(int,Isnull(Product.DisplayOrder,'0'))";
        //        DataTable dtproduct = dbc.GetDataTable(querymain);
        //        if (dtproduct != null && dtproduct.Rows.Count > 0)
        //        {
        //            string querydata = "select KeyValue from StringResources where KeyName='ProductImageUrl'";
        //            DataTable dtpathimg = dbc.GetDataTable(querydata);
        //            string urlpathimg = "", Attribuepathimg = "";
        //            //string urlpathvid = "";
        //            if (dtpathimg != null && dtpathimg.Rows.Count > 0)
        //            {
        //                //Image Path

        //                urlpathimg = dtpathimg.Rows[0]["KeyValue"].ToString();
        //            }
        //            string Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
        //            DataTable dtAttrpathimg = dbc.GetDataTable(Attributedata);
        //            if (dtAttrpathimg != null && dtAttrpathimg.Rows.Count > 0)
        //            {
        //                //Image Path

        //                Attribuepathimg = dtAttrpathimg.Rows[0]["KeyValue"].ToString();
        //            }

        //            objeprodt.response = "1";
        //            objeprodt.message = "Successfully";
        //            objeprodt.WhatsAppNo = sWhatappNo;
        //            objeprodt.ProductList = new List<ProductModel.NewProductDataList>();
        //            string sProductId = "", sMrp = "", sDiscount = "", sEdate = "", sPname = "", sPDiscount = "", sSoshoPrice = "", sSold = "", sProductBanner = "";
        //            string sDUnit = "", sDisplayOrder = "", sMaxQty = "", sMinQty = "", sCategoryId = "", sCategory = "", sProductvariant = "", sIsSoshoRecommended = "";
        //            string sIsSpecialMessage = "", sProductDiscription = "", sIsProductDetails = "", sRecommended = "", sProductNotes = "", sProductKeyFeatures = "", sIsProductDescription = "";
        //            string sisFreeShipping = "", sisFixedShipping = "", sFixedShipRate = "", sSubCategoryId = "", sSubCategory = "";
        //            decimal dDiscount = 0;
        //            Boolean bIsQtyFreeze = false;
        //            for (int i = 0; i < dtproduct.Rows.Count; i++)
        //            {
        //                sProductId = dtproduct.Rows[i]["Pid"].ToString();

        //                ProductModel.NewProductDataList objProduct = new ProductModel.NewProductDataList();
        //                ProductModel.ProductDataImagelist dataImagelist = new ProductModel.ProductDataImagelist();
        //                ProductModel.ProductAttributelist attributelist = new ProductModel.ProductAttributelist();

        //                if (urlpathimg != "")
        //                {

        //                    string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + sProductId;
        //                    DataTable dtdetails = dbc.GetDataTable(ImageDetails);

        //                    if (dtdetails != null && dtdetails.Rows.Count > 0)
        //                    {
        //                        string productid3 = sProductId;
        //                        string proimgid = dtdetails.Rows[0]["id"].ToString();
        //                        string Imagename = dtdetails.Rows[0]["ImageFileName"].ToString();
        //                        string pdisorder = dtdetails.Rows[0]["DisplayOrder"].ToString();

        //                        dataImagelist.proimagid = proimgid;
        //                        dataImagelist.PImgname = urlpathimg + Imagename;
        //                        dataImagelist.prodid = sProductId;
        //                        dataImagelist.PDisOrder = pdisorder;

        //                        objProduct.ProductImageList.Add(dataImagelist);
        //                        //objProduct.ProductImageList = urlpathimg + Imagename;

        //                        //objProduct.ProductImageList = dataImagelist;
        //                    }
        //                }
        //                if (Attribuepathimg != "")
        //                {
        //                    string AttImageDetails = "SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails,Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
        //                                             " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice,pam.PackingType,pam.ProductImage, " +
        //                                             " pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy,pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
        //                                             " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
        //                                             " FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
        //                    DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);

        //                    if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
        //                    {
        //                        string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
        //                        string sAPDiscount = "", sisSelected = "", sisbestbuy = "", sisQtyFreeze = "";
        //                        sMaxQty = ""; sMinQty = "";
        //                        Boolean bAisOutOfStock = false, bAisSelected = false;
        //                        for (int j = 0; j < dtAttdetails.Rows.Count; j++)
        //                        {
        //                            attributelist = new ProductModel.ProductAttributelist();
        //                            sApackSizeId = dtAttdetails.Rows[j]["Id"].ToString();
        //                            sAMrp = dtAttdetails.Rows[j]["Mrp"].ToString();
        //                            sMinQty = dtAttdetails.Rows[j]["MinQty"].ToString();
        //                            sMaxQty = dtAttdetails.Rows[j]["MaxQty"].ToString();


        //                            sADiscount = dtAttdetails.Rows[j]["Discount"].ToString();
        //                            if (sADiscount.ToString() != "0")
        //                            {
        //                                if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "%")
        //                                    sAPDiscount = sADiscount.ToString() + "% Off";
        //                                else if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "Fixed")
        //                                    sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
        //                                else
        //                                    sAPDiscount = "";
        //                            }
        //                            else
        //                                sAPDiscount = "";

        //                            sAPackingType = dtAttdetails.Rows[j]["PackingType"].ToString();
        //                            sAsoshoPrice = dtAttdetails.Rows[j]["SoshoPrice"].ToString();
        //                            sAweight = dtAttdetails.Rows[j]["DUnit"].ToString();
        //                            sAImage = dtAttdetails.Rows[j]["ProductImage"].ToString();
        //                            if (dtAttdetails.Rows[j]["isOutOfStock"].ToString() == "1")
        //                                bAisOutOfStock = true;
        //                            else
        //                                bAisOutOfStock = false;

        //                            //if (dtAttdetails.Rows[j]["isSelected"].ToString() == "1")
        //                            //    bAisSelected = true;
        //                            //else
        //                            //    bAisSelected = false;

        //                            sisSelected = dtAttdetails.Rows[j]["isSelectedDetails"].ToString();
        //                            sisbestbuy = dtAttdetails.Rows[j]["IsBestBuy"].ToString();
        //                            sisQtyFreeze = dtAttdetails.Rows[j]["IsQtyFreeze"].ToString();

        //                            attributelist.Mrp = sAMrp;
        //                            //attributelist.Discount = sADiscount;
        //                            attributelist.Discount = sAPDiscount;
        //                            attributelist.PackingType = sAPackingType;
        //                            attributelist.soshoPrice = sAsoshoPrice;
        //                            attributelist.weight = sAweight;
        //                            attributelist.AImageName = Attribuepathimg + sAImage;
        //                            attributelist.isOutOfStock = bAisOutOfStock.ToString();
        //                            attributelist.MinQty = sMinQty;
        //                            attributelist.MaxQty = sMaxQty;
        //                            attributelist.isSelected = sisSelected;
        //                            attributelist.isQtyFreeze = sisQtyFreeze;
        //                            attributelist.isBestBuy = sisbestbuy;
        //                            attributelist.AttributeId = sApackSizeId;
        //                            objProduct.ProductAttributesList.Add(attributelist);
        //                        }

        //                    }
        //                }
        //                sCategoryId = dtproduct.Rows[i]["CategoryId"].ToString();
        //                sCategory = dtproduct.Rows[i]["CategoryName"].ToString();
        //                sSubCategoryId = dtproduct.Rows[i]["SubCategoryId"].ToString();
        //                sSubCategory = dtproduct.Rows[i]["SubCategoryName"].ToString();
        //                sMrp = dtproduct.Rows[i]["mrp"].ToString();
        //                sDiscount = dtproduct.Rows[i]["Discount"].ToString();

        //                if (sDiscount.ToString() != "0")
        //                {
        //                    if (dtproduct.Rows[i]["DiscountType"].ToString() == "%")
        //                        sPDiscount = sDiscount.ToString() + "% Off";
        //                    else if (dtproduct.Rows[i]["DiscountType"].ToString() == "Fixed")
        //                        sPDiscount = CommonString.rusymbol + " " + sDiscount.ToString() + " Off";
        //                    else
        //                        sPDiscount = "";
        //                }
        //                else
        //                    sPDiscount = "";

        //                dDiscount = 0;
        //                decimal.TryParse(sDiscount.ToString(), out dDiscount);



        //                objProduct.MRP = sMrp;
        //                //objProduct.Discount = dDiscount.ToString();
        //                objProduct.Discount = sPDiscount.ToString();
        //                sPname = dtproduct.Rows[i]["Name"].ToString();
        //                sEdate = dtproduct.Rows[i]["edate"].ToString();
        //                sSoshoPrice = dtproduct.Rows[i]["SoshoPrice"].ToString();
        //                sSold = dtproduct.Rows[i]["sold"].ToString();
        //                sProductBanner = dtproduct.Rows[i]["ProductBanner"].ToString();
        //                sDUnit = dtproduct.Rows[i]["DUnit"].ToString();
        //                sDisplayOrder = dtproduct.Rows[i]["DisplayOrder"].ToString();
        //                sProductvariant = dtproduct.Rows[i]["Productvariant"].ToString();
        //                sIsSoshoRecommended = dtproduct.Rows[i]["IsSoshoRecommended"].ToString();
        //                sIsSpecialMessage = dtproduct.Rows[i]["IsSpecialMessage"].ToString();
        //                sIsProductDescription = dtproduct.Rows[i]["IsProductDescription"].ToString();
        //                sIsProductDetails = dtproduct.Rows[i]["IsProductDetails"].ToString();
        //                sRecommended = dtproduct.Rows[i]["Recommended"].ToString();
        //                sProductDiscription = dtproduct.Rows[i]["ProductDiscription"].ToString();
        //                sProductNotes = dtproduct.Rows[i]["Note"].ToString();
        //                sProductKeyFeatures = dtproduct.Rows[i]["KeyFeatures"].ToString();
        //                sisFreeShipping = dtproduct.Rows[i]["IsFreeShipping"].ToString();
        //                sisFixedShipping = dtproduct.Rows[i]["IsFixedShipping"].ToString();
        //                sFixedShipRate = dtproduct.Rows[i]["FixedShipRate"].ToString();

        //                if (dtproduct.Rows[i]["IsQtyFreeze"].ToString() == "1")
        //                    bIsQtyFreeze = true;
        //                else
        //                    bIsQtyFreeze = false;


        //                sMaxQty = dtproduct.Rows[i]["MaxQty"].ToString();
        //                sMinQty = dtproduct.Rows[i]["MinQty"].ToString();

        //                objProduct.CategoryId = sCategoryId;
        //                objProduct.CategoryName = sCategory;
        //                objProduct.SubCategoryId = sSubCategoryId;
        //                objProduct.SubCategoryName = sSubCategory;
        //                objProduct.Name = sPname;
        //                objProduct.OfferEndDate = sEdate;
        //                objProduct.SellingPrice = sSoshoPrice;
        //                objProduct.SoldCount = sSold;
        //                objProduct.SpecialMessage = sProductBanner;
        //                objProduct.Weight = sDUnit;
        //                objProduct.DisplayOrder = Convert.ToInt32(sDisplayOrder);
        //                //objProduct.IsProductDetails = sIsProductDetails;
        //                objProduct.IsProductVariant = sProductvariant;
        //                objProduct.IsQtyFreeze = bIsQtyFreeze.ToString();
        //                objProduct.SoshoRecommended = sRecommended;
        //                objProduct.IsSoshoRecommended = Convert.ToBoolean(sIsSoshoRecommended);
        //                objProduct.IsSpecialMessage = Convert.ToBoolean(sIsSpecialMessage);
        //                objProduct.MaxQty = sMaxQty;
        //                objProduct.MinQty = sMinQty;
        //                objProduct.IsProductDescription = Convert.ToBoolean(sIsProductDescription);
        //                objProduct.ProductDescription = sProductDiscription;
        //                objProduct.ProductNotes = sProductNotes;
        //                objProduct.ProductKeyFeatures = sProductKeyFeatures;
        //                objProduct.ProductId = sProductId;
        //                objProduct.isFreeShipping = sisFreeShipping;
        //                objProduct.isFixedShipping = sisFixedShipping;
        //                objProduct.FixedShipRate = sFixedShipRate;

        //                objeprodt.ProductList.Add(objProduct);
        //            }
        //        }
        //        else
        //        {
        //            objeprodt.response = "0";
        //            objeprodt.message = "Product Details Not Found";
        //            objeprodt.WhatsAppNo = "";

        //        }
        //        return objeprodt;
        //    }
        //    catch (Exception ex)
        //    {
        //        objeprodt.response = CommonString.Errorresponse;
        //        objeprodt.message = ex.StackTrace + " " + ex.Message;
        //        return objeprodt;
        //    }
        //}

        [HttpGet]

        public ProductModel.getproduct GetProductDetails(string CustomerId = "")
        {
            ProductModel.getproduct objeprodt = new ProductModel.getproduct();

            try
            {

                string OfferProduct = "select Top 1 Product.Id,Product.Name From FreeProduct inner Join Product on Product.Id = FreeProduct.FreeProductId where FreeProduct.IsActive=1 ";
                DataTable dtofferd = dbc.GetDataTable(OfferProduct);

                string OfferPrdId = "";
                DataTable dtoffer = new DataTable();
                if (dtofferd != null && dtofferd.Rows.Count > 0)
                {
                    OfferPrdId = dtofferd.Rows[0]["Id"].ToString();



                    string Offerquerymain = "select Product.id as Pid, [Product].[IsQtyFreeze],(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where Product.id=" + OfferPrdId;
                    dtoffer = dbc.GetDataTable(Offerquerymain);

                }



                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [Order].CustomerId=" + CustomerId;


                    string custmob = "select OrderItem.ProductId,[Order].Id as OrderId from [Order] Inner Join OrderItem on OrderItem.OrderId = [Order].Id where OrderItem.ProductId in (select FreeProductId from FreeProduct) " + where;
                    DataTable dtcustmob = dbc.GetDataTable(custmob);
                    //Start 19-02-2020 Code Modified to hide offer
                    //if (dtcustmob == null || dtcustmob.Rows.Count == 0)
                    //{
                    //    objeprodt.IsDisplayOffer = "True";
                    //    objeprodt.IsDisplasyOfferAll = "True";
                    //}
                    //else
                    //{
                    //    objeprodt.IsDisplayOffer = "False";
                    //    objeprodt.IsDisplasyOfferAll = "True";
                    //}
                    if (dtcustmob == null || dtcustmob.Rows.Count == 0)
                    {
                        objeprodt.IsDisplayOffer = "False";
                        objeprodt.IsDisplasyOfferAll = "False";
                    }
                    else
                    {
                        objeprodt.IsDisplayOffer = "False";
                        objeprodt.IsDisplasyOfferAll = "False";
                    }
                    //End 19-02-2020 Code Modified to hide offer
                }
                else
                {
                    objeprodt.IsDisplayOffer = "False";
                    objeprodt.IsDisplasyOfferAll = "True";
                }
                string querymain = "select Product.id as Pid, [Product].[IsQtyFreeze],(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' Order By convert(int,Isnull(Product.DisplayOrder,'0'))";

                //string querymain = "select Product.id as Pid, [Product].[IsQtyFreeze],(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where Product.id = 266 Order By convert(int,Isnull(Product.DisplayOrder,'0'))";
                //Darshan Temp Data view
                //string querymain = "select top 1 Product.id as Pid,(select  taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate>='05-Sep-2019' and EndDate<='16-Sep-2019'";


                DataTable dtmain = dbc.GetDataTable(querymain);

                string startdate = dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");

                string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "'";

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='ProductImageUrl'";
                    string queryvid = "select KeyValue from StringResources where KeyName='ProductVideoUrl'";

                    DataTable dtpathimg = dbc.GetDataTable(querydata);
                    DataTable dtpathvid = dbc.GetDataTable(queryvid);
                    string urlpathimg = "";
                    //string urlpathvid = "";
                    if (dtpathimg != null && dtpathimg.Rows.Count > 0)
                    {
                        //Image Path

                        urlpathimg = dtpathimg.Rows[0]["KeyValue"].ToString();
                    }

                    objeprodt.response = CommonString.successresponse;
                    objeprodt.message = CommonString.successmessage;

                    //  objeprodt.ButtonDisplayMessage = "Free Product Available Now!";

                    objeprodt.ButtonDisplayMessage = "Claim your app download gift now!";

                    objeprodt.ProductList = new List<ProductModel.ProductDataList>();

                    objeprodt.OfferProductList = new List<ProductModel.OfferProductList>();

                    for (int i = 0; i < dtoffer.Rows.Count; i++)
                    {

                        string pid = dtoffer.Rows[i]["Pid"].ToString();
                        string pname2 = dtoffer.Rows[i]["Name"].ToString();
                        string pdec2 = dtoffer.Rows[i]["ProductDiscription"].ToString();
                        string pkey2 = dtoffer.Rows[i]["KeyFeatures"].ToString();
                        string ptax2 = dtoffer.Rows[i]["Tax"].ToString();
                        string punit = dtoffer.Rows[i]["DUnit"].ToString();
                        string pnote2 = dtoffer.Rows[i]["Note"].ToString();

                        string price = dtoffer.Rows[i]["Mrp"].ToString();
                        string pvid = dtoffer.Rows[i]["VideoName"].ToString();
                        string poffer = dtoffer.Rows[i]["Offer"].ToString();
                        string person2 = dtoffer.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                        string person5 = dtoffer.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                        string shipp = dtoffer.Rows[i]["FixedShipRate"].ToString();
                        string psold = dtoffer.Rows[i]["sold"].ToString();
                        string pboug = dtoffer.Rows[i]["JustBougth"].ToString();
                        string edate = dtoffer.Rows[i]["edate"].ToString();

                        decimal price1 = 0;
                        decimal.TryParse(price.ToString(), out price1);
                        decimal shippincgcharge = 0;
                        decimal.TryParse(shipp.ToString(), out shippincgcharge);

                        decimal pp2 = 0;
                        decimal.TryParse(person2.ToString(), out pp2);
                        decimal pp5 = 0;
                        decimal.TryParse(person5.ToString(), out pp5);




                        ProductModel.OfferProductList objProduct = new ProductModel.OfferProductList();



                        objProduct.productid = OfferPrdId;
                        objProduct.pname = pname2;
                        objProduct.pdec = pdec2;
                        objProduct.pkey = pkey2;
                        objProduct.pprice = price.ToString();

                        if (urlpathimg != "")
                        {

                            //
                            string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + OfferPrdId;
                            DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                            if (dtdetails != null && dtdetails.Rows.Count > 0)
                            {
                                //objeprodt.ProductImageList = new List<ProductModel.ProductDataImagelist>();

                                string productid3 = OfferPrdId;
                                string proimgid = dtdetails.Rows[0]["id"].ToString();
                                string Imagename = dtdetails.Rows[0]["ImageFileName"].ToString();
                                string pdisorder = dtdetails.Rows[0]["DisplayOrder"].ToString();

                                objProduct.PImgname = urlpathimg + Imagename;


                            }
                        }
                        objeprodt.OfferProductList.Add(objProduct);
                    }

                    for (int i = 0; i < dtmain.Rows.Count; i++)
                    {
                        string whatsappno = "6359408097";

                        string minqty = "1";
                        string maxqty = "99";
                        string isqtyfreez = dtmain.Rows[i]["IsQtyFreeze"].ToString();

                        string pid = dtmain.Rows[i]["Pid"].ToString();
                        string pname2 = dtmain.Rows[i]["Name"].ToString();
                        string pdec2 = dtmain.Rows[i]["ProductDiscription"].ToString();
                        string pkey2 = dtmain.Rows[i]["KeyFeatures"].ToString();
                        string ptax2 = dtmain.Rows[i]["Tax"].ToString();
                        string punit = dtmain.Rows[i]["DUnit"].ToString();
                        string pnote2 = dtmain.Rows[i]["Note"].ToString();

                        string price = dtmain.Rows[i]["Mrp"].ToString();
                        string pvid = dtmain.Rows[i]["VideoName"].ToString();
                        string poffer = dtmain.Rows[i]["Offer"].ToString();
                        string person2 = dtmain.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                        string person5 = dtmain.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                        string shipp = dtmain.Rows[i]["FixedShipRate"].ToString();
                        string psold = dtmain.Rows[i]["sold"].ToString();
                        string pboug = dtmain.Rows[i]["JustBougth"].ToString();
                        string edate = dtmain.Rows[i]["edate"].ToString();
                        string whatsappmsg = "Hello!l like this product on Sosho.in and thought of sharing with you! They’ve got some amazing group buy discounts, valid only till " + edate + ". How about we buy it together?";

                        decimal price1 = 0;
                        decimal.TryParse(price.ToString(), out price1);
                        decimal shippincgcharge = 0;
                        decimal.TryParse(shipp.ToString(), out shippincgcharge);

                        decimal pp2 = 0;
                        decimal.TryParse(person2.ToString(), out pp2);
                        decimal pp5 = 0;
                        decimal.TryParse(person5.ToString(), out pp5);




                        //1 peson Price 

                        //decimal price1person = price1 + shippincgcharge;
                        //decimal price2person = price1 - pp2 + shippincgcharge;
                        //decimal price5person = price1 - pp5 + shippincgcharge;


                        ProductModel.ProductDataList objProduct = new ProductModel.ProductDataList();

                        objProduct.whatsapp = whatsappno;
                        objProduct.whatsappmsg = whatsappmsg;

                        objProduct.minqty = minqty;
                        objProduct.maxqty = maxqty;


                        objProduct.pname = pname2;
                        objProduct.pdec = pdec2;
                        objProduct.pkey = pkey2;
                        objProduct.pnote = pnote2;
                        objProduct.pprice = price.ToString();
                        objProduct.pwight = punit;
                        objProduct.poffer = poffer;
                        objProduct.pbuy2 = pp2.ToString();
                        objProduct.pbuy5 = pp5.ToString();
                        objProduct.shipping = shippincgcharge.ToString();
                        objProduct.pvideo = pvid;
                        objProduct.psold = psold;
                        objProduct.pJustBougth = pboug;
                        objProduct.pgst = ptax2;
                        objProduct.enddate = edate;
                        objProduct.IsQtyFreeze = isqtyfreez;
                        objProduct.productid = pid;
                        objProduct.penddate = dtmain.Rows[i]["Enddate1"].ToString();


                        //Multiple Image Singal Product details List
                        if (urlpathimg != "")
                        {

                            //
                            string ImageDetails = "SELECT   [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + dtmain.Rows[i]["Pid"].ToString() + " and isnull(Isdeleted,0)=0";

                            //string ImageDetails = "SELECT   [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=1 and isnull(Isdeleted,0)=0";

                            DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                            if (dtdetails != null && dtdetails.Rows.Count > 0)
                            {
                                //objeprodt.ProductImageList = new List<ProductModel.ProductDataImagelist>();
                                for (int j = 0; j < dtdetails.Rows.Count; j++)
                                {
                                    string productid3 = dtdetails.Rows[j]["Productid"].ToString();
                                    string proimgid = dtdetails.Rows[j]["id"].ToString();
                                    string Imagename = dtdetails.Rows[j]["ImageFileName"].ToString();
                                    string pdisorder = dtdetails.Rows[j]["DisplayOrder"].ToString();

                                    objProduct.ProductImageList.Add(new ProductModel.ProductDataImagelist
                                    {
                                        prodid = productid3,
                                        proimagid = proimgid,
                                        PImgname = urlpathimg + Imagename,
                                        //PImgname = Imagename,
                                        PDisOrder = pdisorder,
                                    });
                                }
                            }
                            objeprodt.ProductList.Add(objProduct);
                        }

                    }
                    //objeprodt.ProductList.Add(
                }
                else
                {
                    objeprodt.response = CommonString.DataNotFoundResponse;
                    objeprodt.message = CommonString.DataNotFoundMessage;

                }
                return objeprodt;
            }
            catch (Exception ex)
            {
                objeprodt.response = CommonString.Errorresponse;
                objeprodt.message = ex.StackTrace + " " + ex.Message;
                return objeprodt;
            }

        }

        [HttpGet]

        public ProductModel.getproduct GetMultipleProductDetails(string CustomerId = "")
        {
            ProductModel.getproduct objeprodt = new ProductModel.getproduct();

            try
            {

                string OfferProduct = "select Top 1 Product.Id,Product.Name From FreeProduct inner Join Product on Product.Id = FreeProduct.FreeProductId where FreeProduct.IsActive=1 ";
                DataTable dtofferd = dbc.GetDataTable(OfferProduct);
                string whatsappno = "6359408097";
                objeprodt.whatsapp = whatsappno;

                string OfferPrdId = "";
                DataTable dtoffer = new DataTable();
                if (dtofferd != null && dtofferd.Rows.Count > 0)
                {
                    OfferPrdId = dtofferd.Rows[0]["Id"].ToString();



                    string Offerquerymain = "select Product.id as Pid, [Product].[IsQtyFreeze],(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where Product.id=" + OfferPrdId;
                    dtoffer = dbc.GetDataTable(Offerquerymain);

                }



                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [Order].CustomerId=" + CustomerId;


                    string custmob = "select OrderItem.ProductId,[Order].Id as OrderId from [Order] Inner Join OrderItem on OrderItem.OrderId = [Order].Id where OrderItem.ProductId in (select FreeProductId from FreeProduct) " + where;
                    DataTable dtcustmob = dbc.GetDataTable(custmob);
                    //Start 19-02-2020 Code Modified to hide offer
                    //if (dtcustmob == null || dtcustmob.Rows.Count == 0)
                    //{
                    //    objeprodt.IsDisplayOffer = "True";
                    //    objeprodt.IsDisplasyOfferAll = "True";
                    //}
                    //else
                    //{
                    //    objeprodt.IsDisplayOffer = "False";
                    //    objeprodt.IsDisplasyOfferAll = "True";
                    //}
                    if (dtcustmob == null || dtcustmob.Rows.Count == 0)
                    {
                        objeprodt.IsDisplayOffer = "False";
                        objeprodt.IsDisplasyOfferAll = "False";
                    }
                    else
                    {
                        objeprodt.IsDisplayOffer = "False";
                        objeprodt.IsDisplasyOfferAll = "False";
                    }
                    //End 19-02-2020 Code Modified to hide offer
                }
                else
                {
                    objeprodt.IsDisplayOffer = "False";
                    objeprodt.IsDisplasyOfferAll = "True";
                }
                string querymain = "select Product.id as Pid, [Product].[IsQtyFreeze],(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' Order By convert(int,Isnull(Product.DisplayOrder,'0')) ";

                //Darshan Temp Data view
                //string querymain = "select top 1 Product.id as Pid,(select  taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate>='05-Sep-2019' and EndDate<='16-Sep-2019'";


                DataTable dtmain = dbc.GetDataTable(querymain);

                string startdate = dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");

                string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "'";

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='ProductImageUrl'";
                    string queryvid = "select KeyValue from StringResources where KeyName='ProductVideoUrl'";

                    DataTable dtpathimg = dbc.GetDataTable(querydata);
                    DataTable dtpathvid = dbc.GetDataTable(queryvid);
                    string urlpathimg = "";
                    //string urlpathvid = "";
                    if (dtpathimg != null && dtpathimg.Rows.Count > 0)
                    {
                        //Image Path

                        urlpathimg = dtpathimg.Rows[0]["KeyValue"].ToString();
                    }

                    objeprodt.response = CommonString.successresponse;
                    objeprodt.message = CommonString.successmessage;

                    //  objeprodt.ButtonDisplayMessage = "Free Product Available Now!";

                    objeprodt.ButtonDisplayMessage = "Claim your app download gift now!";

                    objeprodt.ProductList = new List<ProductModel.ProductDataList>();

                    objeprodt.OfferProductList = new List<ProductModel.OfferProductList>();

                    for (int i = 0; i < dtoffer.Rows.Count; i++)
                    {

                        string pid = dtoffer.Rows[i]["Pid"].ToString();
                        string pname2 = dtoffer.Rows[i]["Name"].ToString();
                        string pdec2 = dtoffer.Rows[i]["ProductDiscription"].ToString();
                        string pkey2 = dtoffer.Rows[i]["KeyFeatures"].ToString();
                        string ptax2 = dtoffer.Rows[i]["Tax"].ToString();
                        string punit = dtoffer.Rows[i]["DUnit"].ToString();
                        string pnote2 = dtoffer.Rows[i]["Note"].ToString();

                        string price = dtoffer.Rows[i]["Mrp"].ToString();
                        string pvid = dtoffer.Rows[i]["VideoName"].ToString();
                        string poffer = dtoffer.Rows[i]["Offer"].ToString();
                        string person2 = dtoffer.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                        string person5 = dtoffer.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                        string shipp = dtoffer.Rows[i]["FixedShipRate"].ToString();
                        string psold = dtoffer.Rows[i]["sold"].ToString();
                        string pboug = dtoffer.Rows[i]["JustBougth"].ToString();
                        string edate = dtoffer.Rows[i]["edate"].ToString();

                        decimal price1 = 0;
                        decimal.TryParse(price.ToString(), out price1);
                        decimal shippincgcharge = 0;
                        decimal.TryParse(shipp.ToString(), out shippincgcharge);

                        decimal pp2 = 0;
                        decimal.TryParse(person2.ToString(), out pp2);
                        decimal pp5 = 0;
                        decimal.TryParse(person5.ToString(), out pp5);




                        ProductModel.OfferProductList objProduct = new ProductModel.OfferProductList();



                        objProduct.productid = OfferPrdId;
                        objProduct.pname = pname2;
                        objProduct.pdec = pdec2;
                        objProduct.pkey = pkey2;
                        objProduct.pprice = price.ToString();

                        if (urlpathimg != "")
                        {

                            //
                            string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + OfferPrdId;
                            DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                            if (dtdetails != null && dtdetails.Rows.Count > 0)
                            {
                                //objeprodt.ProductImageList = new List<ProductModel.ProductDataImagelist>();

                                string productid3 = OfferPrdId;
                                string proimgid = dtdetails.Rows[0]["id"].ToString();
                                string Imagename = dtdetails.Rows[0]["ImageFileName"].ToString();
                                string pdisorder = dtdetails.Rows[0]["DisplayOrder"].ToString();

                                objProduct.PImgname = urlpathimg + Imagename;


                            }
                        }
                        objeprodt.OfferProductList.Add(objProduct);
                    }

                    for (int i = 0; i < dtmain.Rows.Count; i++)
                    {


                        string minqty = "1";
                        string maxqty = "99";
                        string isqtyfreez = dtmain.Rows[i]["IsQtyFreeze"].ToString();

                        string pid = dtmain.Rows[i]["Pid"].ToString();
                        string pname2 = dtmain.Rows[i]["Name"].ToString();
                        string pdec2 = dtmain.Rows[i]["ProductDiscription"].ToString();
                        string pkey2 = dtmain.Rows[i]["KeyFeatures"].ToString();
                        string ptax2 = dtmain.Rows[i]["Tax"].ToString();
                        string punit = dtmain.Rows[i]["DUnit"].ToString();
                        string pnote2 = dtmain.Rows[i]["Note"].ToString();

                        string price = dtmain.Rows[i]["Mrp"].ToString();
                        string pvid = dtmain.Rows[i]["VideoName"].ToString();
                        string poffer = dtmain.Rows[i]["Offer"].ToString();
                        string person2 = dtmain.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                        string person5 = dtmain.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                        string shipp = dtmain.Rows[i]["FixedShipRate"].ToString();
                        string psold = dtmain.Rows[i]["sold"].ToString();
                        string pboug = dtmain.Rows[i]["JustBougth"].ToString();
                        string edate = dtmain.Rows[i]["edate"].ToString();
                        string whatsappmsg = "Hello!l like this product on Sosho.in and thought of sharing with you! They’ve got some amazing group buy discounts, valid only till " + edate + ". How about we buy it together?";

                        decimal price1 = 0;
                        decimal.TryParse(price.ToString(), out price1);
                        decimal shippincgcharge = 0;
                        decimal.TryParse(shipp.ToString(), out shippincgcharge);

                        decimal pp2 = 0;
                        decimal.TryParse(person2.ToString(), out pp2);
                        decimal pp5 = 0;
                        decimal.TryParse(person5.ToString(), out pp5);




                        //1 peson Price 

                        //decimal price1person = price1 + shippincgcharge;
                        //decimal price2person = price1 - pp2 + shippincgcharge;
                        //decimal price5person = price1 - pp5 + shippincgcharge;


                        ProductModel.ProductDataList objProduct = new ProductModel.ProductDataList();

                        //objProduct.whatsapp = whatsappno;
                        objProduct.whatsappmsg = whatsappmsg;

                        objProduct.minqty = minqty;
                        objProduct.maxqty = maxqty;


                        objProduct.pname = pname2;
                        objProduct.pdec = pdec2;
                        objProduct.pkey = pkey2;
                        objProduct.pnote = pnote2;
                        objProduct.pprice = price.ToString();
                        objProduct.pwight = punit;
                        objProduct.poffer = poffer;
                        objProduct.pbuy2 = pp2.ToString();
                        objProduct.pbuy5 = pp5.ToString();
                        objProduct.shipping = shippincgcharge.ToString();
                        objProduct.pvideo = pvid;
                        objProduct.psold = psold;
                        objProduct.pJustBougth = pboug;
                        objProduct.pgst = ptax2;
                        objProduct.enddate = edate;
                        objProduct.IsQtyFreeze = isqtyfreez;
                        objProduct.productid = pid;


                        //Multiple Image Singal Product details List
                        if (urlpathimg != "")
                        {

                            //
                            string ImageDetails = "SELECT   [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + dtmain.Rows[i]["Pid"].ToString() + " and isnull(Isdeleted,0)=0";

                            //string ImageDetails = "SELECT   [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=1 and isnull(Isdeleted,0)=0";

                            DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                            if (dtdetails != null && dtdetails.Rows.Count > 0)
                            {
                                //objeprodt.ProductImageList = new List<ProductModel.ProductDataImagelist>();
                                for (int j = 0; j < dtdetails.Rows.Count; j++)
                                {
                                    string productid3 = dtdetails.Rows[j]["Productid"].ToString();
                                    string proimgid = dtdetails.Rows[j]["id"].ToString();
                                    string Imagename = dtdetails.Rows[j]["ImageFileName"].ToString();
                                    string pdisorder = dtdetails.Rows[j]["DisplayOrder"].ToString();

                                    objProduct.ProductImageList.Add(new ProductModel.ProductDataImagelist
                                    {
                                        prodid = productid3,
                                        proimagid = proimgid,
                                        PImgname = urlpathimg + Imagename,
                                        //PImgname = Imagename,
                                        PDisOrder = pdisorder,
                                    });
                                }
                            }
                            objeprodt.ProductList.Add(objProduct);
                        }

                    }
                    //objeprodt.ProductList.Add(
                }
                else
                {
                    objeprodt.response = CommonString.DataNotFoundResponse;
                    objeprodt.message = CommonString.DataNotFoundMessage;

                }
                return objeprodt;
            }
            catch (Exception ex)
            {
                objeprodt.response = CommonString.Errorresponse;
                objeprodt.message = ex.StackTrace + " " + ex.Message;
                return objeprodt;
            }

        }

    }
}
