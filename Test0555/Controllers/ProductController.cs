using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Http;
using Test0555.Models.ProductManagement;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace Test0555.Controllers
{
    public class ProductController : ApiController
    {
        public HttpApplicationState Application
        {
            get;
            //private set;
        }


        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        public enum FilterRate
        {
            LowToHigh = 1,
            HighToLow = 2,
            Discount = 3,
            SoshoRecommended = 4
        }
        public enum BannerActionType
        {
            OpenUrl = 1,
            NavigateToCategory = 2,
            AddToCart = 3,
            None = -1
        }

        [HttpGet]
        //02-10-2020 Developed By :- Vidhi Doshi
        public ProductModel.getNewproduct GetDashBoardProductDetails(string JurisdictionID, int CategoryId = -1, int SubCategoryId = -1, string ProductId = "", string StartNo = "", string EndNo = "", int Filter = 1, string InterBannerid = "",int SearchProductId=-1)
        {
            ProductModel.getNewproduct objeprodt = new ProductModel.getNewproduct();
            try
            {
                int iBannerPosition = (ConfigurationManager.AppSettings["BannerPosition"] != null && ConfigurationManager.AppSettings["BannerPosition"].Trim() != "") ? Convert.ToInt16(ConfigurationManager.AppSettings["BannerPosition"].Trim()) : 0;
                //EndNo = (iBannerPosition + 1).ToString();
                int response = 0;
                EndNo = ((Convert.ToInt32(StartNo)-1)+ iBannerPosition).ToString();
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";
                string sWhatappNo = "";
                string subquerystr = "select isnull(Mobile,'') as Mobile From users where JurisdictionID =  " + JurisdictionID;
                DataTable dtsubmain = dbc.GetDataTable(subquerystr);
                if (dtsubmain != null && dtsubmain.Rows.Count > 0)
                {
                    sWhatappNo = dtsubmain.Rows[0]["Mobile"].ToString();
                }
                objeprodt.BannerPosition = iBannerPosition.ToString();
                objeprodt.ProductList = new List<ProductModel.NewProductDataList>();
                ProductModel.ProductAttributelist attributeHomePagelist = new ProductModel.ProductAttributelist();

                string condstr = "";
                if (JurisdictionID != "" && JurisdictionID != null)
                {
                    condstr = " and JB.JurisdictionId = " + JurisdictionID + " AND JB.BannerType = 'HomePage' ";
                }
                string querystr = " Select ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty," +
                                     "  ISNULL(P.MinQty,0) AS MinQty, " +
                                     " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                                     " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(CL.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                                     " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " +
                                     " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title, " +
                                     " ISNULL(PA.AttributeId,0) AS AttributeId, ISNULL(AC.CategoryName,'') AS ActionCategory,ISNULL(Im.CategoryId,0) AS ActionCategoryId " +
                                     " From HomepageBanner Im " +
                                     " LEFT join tblCategoryBannerLink CL on CL.BannerId = Im.Id " +
                                     " Left join category cg on  cg.categoryId = CL.categoryId " +
                                     " Left join category Ac on  AC.CategoryID = Im.categoryId " +
                                     " Left join Product P on  P.Id = Im.ProductId " +
                                     " Left join UnitMaster U on U.Id = P.UnitId " +
                                     " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                                     " Left join (SELECT ProductId, ISNULL(ID,0) AS AttributeId  FROM Product_ProductAttribute_Mapping WHERE ISSelected = 1 ) PA on  P.Id = PA.ProductId " +
                                     " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate +
                                     "' and Im.StartDate<='" + startend + "'" +
                                     condstr;
                if (CategoryId > 0)
                {
                    querystr += " and CL.CategoryID =" + CategoryId;
                }
                querystr += "  order by Im.Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);
                string Id = "", ImageName1 = "", sAction = "", bCategoryId = "", sCategoryName = "", sopenUrlLink = "";
                string sProductName = "", sUnitName = "", sWeight = "", sSellingPrice = "", sMRP = "", bDiscount = "";
                string sTitle = "", sActionCategoryId = "", sActionCategoryName = "";
                string bMaxQty = "", bMinQty = "", sAttributeId = "";
                int sActionId = 0, bProductId = 0;
                bool sIsQtyFreeze = false;
                if (ProductId == "0" || string.IsNullOrEmpty(ProductId))
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
                                if (!string.IsNullOrEmpty(dtmain.Rows[n]["ActionCategoryId"].ToString()))
                                {
                                    sActionCategoryId = dtmain.Rows[n]["ActionCategoryId"].ToString();
                                    sActionCategoryName = dtmain.Rows[n]["ActionCategory"].ToString();
                                }
                                else
                                {
                                    sActionCategoryId = "0";
                                    sActionCategoryName = "";
                                }
                                if (!string.IsNullOrEmpty(dtmain.Rows[n]["Link"].ToString()))
                                    sopenUrlLink = dtmain.Rows[n]["Link"].ToString();
                                else
                                    sopenUrlLink = "";

                                if (Convert.ToInt32(dtmain.Rows[n]["AttributeId"]) > 0)
                                {
                                    sAttributeId = dtmain.Rows[n]["AttributeId"].ToString();
                                }
                                if (sActionId == BannerActionType.OpenUrl.GetHashCode())
                                {
                                    sAction = BannerActionType.OpenUrl.ToString();
                                }
                                if (sActionId == BannerActionType.NavigateToCategory.GetHashCode())
                                {
                                    sAction = BannerActionType.NavigateToCategory.ToString();
                                }
                                if (sActionId == BannerActionType.AddToCart.GetHashCode())
                                {
                                    sAction = BannerActionType.AddToCart.ToString();
                                }
                                if (sActionId == BannerActionType.None.GetHashCode())
                                {
                                    sAction = BannerActionType.None.ToString();
                                }


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
                                objHomePagebaner.ActionCategoryId = sActionCategoryId;
                                objHomePagebaner.ActionCategoryName = sActionCategoryName;
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
                        response = 1;
                    }
                    
                }
                
                //string querymain = " with pte as ( select TOP " + iBannerPosition + " ROW_NUMBER() over(order by convert(int,Isnull(Product.DisplayOrder,'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                //string querymain = " with pte as ( select TOP " + EndNo + " ROW_NUMBER() over(order by convert(int,Isnull(Product.id,'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                string rownumfield = string.Empty;
                switch (Filter)
                {
                    case (int)FilterRate.LowToHigh:
                        rownumfield = "order by convert(int,Isnull(Product.SoshoPrice,'0'))";
                        break;
                    case (int)FilterRate.HighToLow:
                        rownumfield = "order by convert(int,Isnull(Product.SoshoPrice,'0')) desc";
                        break;
                    default:
                        rownumfield = "order by convert(int,Isnull(Product.Id,'0'))";
                        break;
                }
                //string querymain = " with pte as ( select TOP " + EndNo + " ROW_NUMBER() over(order by convert(int,Isnull("+ rownumfield + ",'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                string querymain = " with pte as ( select TOP " + EndNo + " ROW_NUMBER() over(" + rownumfield + ") as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                querymain += "(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,";
                querymain += " Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,";
                querymain += "CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1, ";
                querymain += " isnull(cat.CategoryName, '') as CategoryName,isnull(Name, '') as [Name],isnull(sold, '') as [sold],isnull(ProductBanner, '') as ProductBanner,";
                querymain += " isnull(DisplayOrder, '') as DisplayOrder,isnull(Recommended,'') as Recommended,isnull(ProductDiscription,'') as ProductDiscription,isnull(Note,'') as Note,isnull(KeyFeatures,'') as KeyFeatures,";
                querymain += " case when isnull(ProductDiscription,'') = '' then 'false' else 'true' end as IsProductDetails,";
                querymain += " case when isnull(ProductTemplateID,'') = '2' then 'true' else 'false' end as Productvariant,";
                querymain += " case when isnull(Recommended,'') = '' then 'false' else 'true' end as IsSoshoRecommended,";
                querymain += "case when isnull(ProductBanner,'') = '' then 'false' else 'true' end as IsSpecialMessage ";
                querymain += "  ,PL.CategoryID,ProductMRP AS mrp,Isnull(cast(cast(discount as decimal(10,2)) AS FLOAT),'') AS discount,DiscountType,SoshoPrice,MaxQty,MinQty,case when isnull(IsProductDescription,'') = '1' then 'true' else 'false' end as IsProductDescription ";
                querymain += " ,case when isnull(IsFreeShipping,'') = '' then 'false' else 'true' end as IsFreeShipping ,PL.SubCategoryID  ";
                querymain += " ,case when isnull(IsFixedShipping,'') = '' then 'false' else 'true' end as IsFixedShipping, FixedShipRate,isnull(Scat.SubCategory, '') as SubCategoryName ";
                querymain += " from Product ";
                querymain += " LEFT join tblCategoryProductLink PL on PL.ProductId = Product.Id ";
                querymain += " inner join Unitmaster on Unitmaster.id=Product.UnitId ";
                querymain += " inner join Category cat on cat.CategoryID = PL.CategoryID ";
                querymain += " inner join tblSubCategory Scat on Scat.Id = PL.SubCategoryID ";
                querymain += " Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                querymain += "and Product.IsActive = 1 and Product.IsDeleted = 0 and Isnull(Product.IsApproved,'') = 1 and Product.JurisdictionID =" + JurisdictionID;
                if (CategoryId > 0)
                {
                    querymain += " and PL.CategoryID =" + CategoryId;
                }
                if (SubCategoryId > 0)
                {
                    querymain += " and PL.SubCategoryID =" + SubCategoryId;
                }
                if(SearchProductId > 0)
                {
                    querymain += " and Product.Id =" + SearchProductId;
                }
                //if (!string.IsNullOrEmpty(ProductId))
                //{
                //    querymain += " and Product.Id >" + ProductId;
                //}
                if (Filter == FilterRate.SoshoRecommended.GetHashCode())
                {
                    querymain += " and ISNULL(Recommended,'') != '' ";
                }
                if (Filter == FilterRate.Discount.GetHashCode())
                {
                    querymain += " and ISNULL(Discount,0) > 0 ";
                }
                if (Filter == FilterRate.LowToHigh.GetHashCode())
                {
                    querymain += " Order by Product.SoshoPrice ";
                }
                if (Filter == FilterRate.HighToLow.GetHashCode())
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

                    response = 1;
                    
                    string sProductId = "", sDiscount = "", sEdate = "", sPname = "", sPDiscount = "", sSold = "", sProductBanner = "";
                    string sDisplayOrder = "", sMaxQty = "", sMinQty = "", sCategoryId = "", sCategory = "", sIsSoshoRecommended = "";
                    string sIsSpecialMessage = "", sProductDiscription = "", sRecommended = "", sProductNotes = "", sProductKeyFeatures = "", sIsProductDescription = "";
                    string sisFreeShipping = "", sisFixedShipping = "", sFixedShipRate = "";
                    decimal dDiscount = 0;
                    sActionCategoryId = ""; sActionCategoryName = "";
                    int prodrowCount = dtproduct.Rows.Count;

                    for (int i = 0; i < dtproduct.Rows.Count; i++)
                    {
                        nPosCtr++;
                        sProductId = dtproduct.Rows[i]["Pid"].ToString();

                        ProductModel.NewProductDataList objProduct = new ProductModel.NewProductDataList();
                        ProductModel.ProductDataImagelist dataImagelist = new ProductModel.ProductDataImagelist();
                        ProductModel.ProductAttributelist attributelist = new ProductModel.ProductAttributelist();

                        if (Attribuepathimg != "")
                        {
                            string AttImageDetails = " SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails, " +
                                                     " Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
                                                     " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice, " +
                                                     " pam.PackingType,pam.ProductImage, pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy," +
                                                     " pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
                                                     " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
                                                     " FROM Product_ProductAttribute_Mapping pam " +
                                                     " inner join Unitmaster um on um.id=pam.UnitId " +
                                                     " where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
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
                        objProduct.ProductName = sPname;
                        objProduct.Title = "";
                        objProduct.bannerId = "0";
                        objProduct.bannerURL = "";
                        objProduct.ActionId = 0;
                        objProduct.action = "";
                        objProduct.openUrlLink = "";
                        objProduct.ActionCategoryId = "0";
                        objProduct.ActionCategoryName = "";
                        objeprodt.ProductList.Add(objProduct);

                    }

                    string cond = string.Empty;
                    if (JurisdictionID != "" && JurisdictionID != null)
                    {
                        cond = " and JB.JurisdictionId = " + JurisdictionID + " AND CL.BannerType = 'Intermediate' ";
                    }
                    string qry = "Select  ISNULL(cg.CategoryName,'') AS CategoryName,ISNULL(P.Name,'') AS ProductName,ISNULL(P.MaxQty,0) AS MaxQty," +
                             " ISNULL(P.MinQty,0) AS MinQty, " +
                             " ISNULL(P.ProductMRP,0) AS MRP, Isnull(cast(cast(P.Discount as decimal(10,2)) AS FLOAT),'') AS Discount," +
                             " ISNULL(P.SoshoPrice,0) AS SellingPrice,Im.Id, Isnull(Im.ActionId,0) As ActionId, ISNULL(Im.Action,'') AS Action, ISNULL(CL.CategoryId,0) AS CategoryId, ISNULL(Im.Link,'') AS Link, " +
                             " ISNULL(Im.ImageName,'') AS ImageName, ISNULL(Im.ProductId, 0) AS ProductId, ISNULL(P.IsQtyFreeze,0) AS  IsQtyFreeze, ISNULL(P.Unit,0) AS Weight, " +
                             " ISNULL(U.UnitName ,'') AS UnitName, ISNULL(Im.Title,'') AS Title, " +
                             " ISNULL(PA.AttributeId,0) AS AttributeId, " +
                             " ISNULL(P.SubCategoryId,0) AS SubCategoryId,ISNULL(SC.SubCategory,'') AS SubCategoryName, " +
                             " ISNULL(AC.CategoryName,'') AS ActionCategory,ISNULL(Im.CategoryId,0) AS ActionCategoryId " +
                             " From IntermediateBanners Im " +
                             " LEFT join tblCategoryBannerLink CL on CL.BannerId = Im.Id " +
                             " Left join category cg on  cg.categoryId = CL.categoryId " +
                             " Left join category Ac on  AC.CategoryID = Im.categoryId " +
                             " Left join Product P on  P.Id = Im.ProductId " +
                             " Left join tblSubCategory SC on SC.Id = P.SubCategoryId  " +
                             " Left join UnitMaster U on U.Id = P.UnitId " +
                             " Left join JurisdictionBanner JB on JB.BannerId = Im.Id " +
                             " Left join (SELECT ProductId, ISNULL(ID,0) AS AttributeId  FROM Product_ProductAttribute_Mapping WHERE ISSelected = 1 ) PA on  P.Id = PA.ProductId " +
                             " where Im.IsActive=1 and Im.IsDeleted=0 and Im.StartDate>='" + startdate +
                             "' and Im.StartDate<='" + startend + "'" +
                             cond;
                    if (CategoryId > 0)
                    {
                        qry += " and CL.CategoryID =" + CategoryId;
                    }
                    if(InterBannerid != "0" && !string.IsNullOrEmpty(InterBannerid))
                    {
                        qry += " and Im.Id NOT IN (" + InterBannerid + ")";
                    }
                    
                    qry += "  order by Im.Id desc";
                    DataTable dtInterBanner = dbc.GetDataTable(qry);
                    ProductModel.NewProductDataList objBannerProduct = new ProductModel.NewProductDataList();
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
                            if (!string.IsNullOrEmpty(dtInterBanner.Rows[j]["ActionCategoryId"].ToString()))
                            {
                                sActionCategoryId = dtInterBanner.Rows[j]["ActionCategoryId"].ToString();
                                sActionCategoryName = dtInterBanner.Rows[j]["ActionCategory"].ToString();
                            }
                            else
                            {
                                sActionCategoryId = "0";
                                sActionCategoryName = "";
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
                            objIntermediateBanner.ActionCategoryId = sActionCategoryId;
                            objIntermediateBanner.ActionCategoryName = sActionCategoryName;
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
                            objIntermediateBanner.ProductAttributesList = objAttrList;
                            break;
                        }
                    }
                }
                
                if (response == 1)
                {
                    objeprodt.response = "1";
                    objeprodt.message = "Successfully";
                }
                else
                {
                    objeprodt.response = "0";
                    objeprodt.message = "Details Not Found";
                }
                objeprodt.WhatsAppNo = sWhatappNo;
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

        //08-10-2020 Search Related API
        [HttpGet]
        public string[] GetResultsBySearch(string Searchname1)
        {
            if (Searchname1.Contains('\''))
            {
                Searchname1 = Searchname1.Replace("'", "");
            }

            string Searchname = "", Searchnamewithoutspace = "";
            bool isSpaceStringSame = false;
            DataTable dtResult = new DataTable();
            string[] returnSearch = new string[2];
            try
            {
                if (Searchname1 != null && Searchname1.Trim() != "")
                {
                    char[] ch = { ' ' };
                    string[] splt = Searchname1.Split(ch, StringSplitOptions.RemoveEmptyEntries);
                    if (splt.Length > 1)
                    {

                    }
                    else
                        Searchname = splt[0].Trim();

                    if (Searchname != "")
                    {
                        Searchnamewithoutspace = Searchname.Replace(" ", "").Trim();
                        if (Searchname.Equals(Searchnamewithoutspace))
                            isSpaceStringSame = true;
                    }
                    int Max_Seller = 10, Max_Category = 10, Max_Product = 10;
                    DataTable dtCategory = new DataTable();
                    DataTable dtProduct = new DataTable();
                    DataTable dtSubCategory = new DataTable();

                    DataTable dtCategoryFinal = new DataTable();
                    DataTable dtSubCategoryFinal = new DataTable();
                    DataTable dtProductFinal = new DataTable();

                    DataTable dtCategory_reverse = new DataTable();
                    DataTable dtProduct_reverse = new DataTable();
                    DataTable dtSubCategory_reverse = new DataTable();

                    try
                    {
                        dtResult.Columns.Add("ID", typeof(System.Int64));
                        dtResult.Columns.Add("Name");
                        dtResult.Columns.Add("Link");
                        dtResult.Columns.Add("Type", typeof(System.Int64));//1=Seller 2=Category 3=Product
                        dtResult.Columns.Add("Total_Word", typeof(System.Int64));
                        dtResult.Columns.Add("SearchType", typeof(System.Int64));//1=Direct Word  2=Word without space
                        dtResult.Columns.Add("CategoryId"); //CategoryId

                        if (Searchname.Trim() != "" || Searchnamewithoutspace.Trim() != "")
                        {
                            GetProductSearchResult(Searchname, Searchnamewithoutspace, isSpaceStringSame, dtResult, ref dtCategory, ref dtSubCategory, ref dtProduct);
                        }
                        else
                        {
                            dtCategory = dtResult.Clone();
                            dtProduct = dtResult.Clone();
                        }
                        int intCategory = dtCategory.Rows.Count;
                        int intProduct = dtProduct.Rows.Count;
                        int intSubCategory = dtSubCategory.Rows.Count;

                        int intCategory_reverse = dtCategory_reverse.Rows.Count;
                        int intProduct_reverse = dtProduct_reverse.Rows.Count;
                        int intSubCategory_reverse = dtSubCategory_reverse.Rows.Count;

                        dtCategoryFinal = dtCategory.Clone();
                        //dtSubCategoryFinal = dtCategory.Clone();
                        dtSubCategoryFinal = dtSubCategory.Clone();
                        dtProductFinal = dtProduct.Clone();

                        // SqlConnection sqlcon1 = GetConnection();
                        AddUniqueSearchResult(dtCategoryFinal, dtCategory, Max_Category);

                        AddUniqueSearchResult(dtSubCategoryFinal, dtSubCategory, Max_Category);
                        //AddSubCategoryUniqueSearchResult(dtSubCategoryFinal, dtSubCategory, Max_Category);

                        //if (dtCategoryFinal.Rows.Count != Max_Category)
                        //{
                        //    //AddUniqueSearchResult(dtCategoryFinal, dtCategory_reverse, Max_Category);
                        //    AddCategoryUniqueSearchResult(dtCategoryFinal, dtCategory, Max_Category, distinctcategoryid);
                        //}

                        AddUniqueSearchResult(dtProductFinal, dtProduct, Max_Product);

                        if (dtProductFinal.Rows.Count != Max_Product)
                        {
                            AddUniqueSearchResult(dtProductFinal, dtProduct_reverse, Max_Product);
                        }

                        StringBuilder sb = new StringBuilder();

                        //AppendSearchResultString(dtCategoryFinal, sb);
                        AppendCategorySearchResultString(dtCategoryFinal, sb);
                        AppendSubCategorySearchResultString(dtSubCategoryFinal, sb);
                        AppendSearchResultString(dtProductFinal, sb);

                        string finalstr = sb.ToString();
                        string[] arrstr = { "@@@@" };
                        returnSearch = finalstr.Split(arrstr, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return returnSearch;
        }
        private static void AddUniqueSearchResult(DataTable dtSellerFinal, DataTable dtSeller, int Max_Seller)
        {
            foreach (DataRow dr in dtSeller.Rows)
            {
                if (Max_Seller == -1 || dtSellerFinal.Rows.Count < Max_Seller)
                {
                    string id = dr["Id"].ToString();
                    string Type = dr["Type"].ToString();

                    DataRow[] drfindRec = dtSellerFinal.Select("ID=" + id + " and Type=" + Type);
                    if (drfindRec.Length == 0)
                    {
                        dtSellerFinal.ImportRow(dr);
                    }
                }
                else
                {
                    break;
                }
            }
        }
        private void AddSubCategoryUniqueSearchResult(DataTable dtSellerFinal, DataTable dtSeller, int Max_Seller)
        {

            //foreach (DataRow dr in dtSeller.Rows)
            //{
            // var distinctcategoryid = (from row in dtProduct.AsEnumerable() select row.Field<string>("CategoryId")).Distinct().ToList();
            //var subcategoryname = (from row in dtSeller.AsEnumerable() select row.Field<string>("Id")).ToList();
            if (Max_Seller == -1 || dtSellerFinal.Rows.Count < Max_Seller)
            {
                //string id = dr["Id"].ToString();
                //string Type = dr["Type"].ToString();

                //foreach (var item in distinctcategoryid)
                //{
                //string id = item;
                string id = "";
                if (dtSeller != null)
                {
                    if (dtSeller.Rows.Count > 0)
                    {
                        DataTable dtSub = new DataTable();

                        //SqlConnection sqlcon = GetConnection();
                        //SqlCommand sqlcmd = new SqlCommand();
                        //sqlcmd.Connection = sqlcon;

                        string query = "";
                        query = "select *,(replace(replace((isnull(SubCategory,'')), '''', ''),'’','')) as Name,(isnull(SubCategory,'')) as Name1 from tblSubCategory where CategoryId =" + id;
                        //SqlDataAdapter sqladapter = new SqlDataAdapter(query, sqlcon);
                        //sqladapter.Fill(dtSub);
                        //DataRow[] drfindRec = dtSellerFinal.Select("ID=" + id + " and Type=" + Type);
                        dtSub = dbc.GetDataTable(query);

                        DataRow[] drfindRec = dtSub.Select("CategoryId=" + id);
                        foreach (DataRow dr in drfindRec)
                        {
                            dtSellerFinal.ImportRow(dr);
                        }
                    }
                }
                // DataRow[] drfindRec = dtSeller.Select("ROWID=" + id);
                //if (drfindRec.Length == 0)
                //{
                //foreach (DataRow dr in drfindRec)
                //{
                //    string id1 = dr["RowId"].ToString();
                //    string Type1 = dr["Link"].ToString();


                //  //  dtSellerFinal.ImportRow(dr);

                //    DataRow[] drfindRec1 = dtSeller.Select("Link='" + Type1 + "'");

                //    foreach(DataRow dr1 in drfindRec1)
                //    {
                //        dtSellerFinal.ImportRow(dr1);
                //    }
                //}

                // }
                //}
            }
            //else
            //{
            //    break;
            //}
            //}
        }

        private void GetProductSearchResult(string Searchname, string Searchnamewithoutspace, bool isSpaceStringSame, DataTable dtResult, ref DataTable dtCategory, ref DataTable dtSubCategory, ref DataTable dtProduct)
        {
            //FOR GETTING CATEGORY FROM SEARCH
            string[] arrCategory = { "CategoryName" };

            string strCAtegorySearch = "";
            string strCAtegorySearchWordStart = "", strCAtegorySearchWordStartInner = "";

            foreach (string cname in arrCategory)
            {
                strCAtegorySearch += cname + " like '%" + Searchname + "%' or ";
            }
            strCAtegorySearch = strCAtegorySearch.Trim().TrimEnd('o', 'r');

            string strCAtegorySearchwithoutspace = "";
            string strCAtegorySearchWordStartwithoutspace = "", strCAtegorySearchWordStartInnerwithoutspace = "";

            foreach (string cname in arrCategory)
            {
                strCAtegorySearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
            }
            strCAtegorySearchwithoutspace = strCAtegorySearchwithoutspace.Trim().TrimEnd('o', 'r');

            string strCAtegoryTableName = "Category";
            dtCategory = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrCategory, strCAtegorySearch, strCAtegorySearchwithoutspace, strCAtegoryTableName, 2, strCAtegorySearchWordStart, strCAtegorySearchWordStartInner, strCAtegorySearchWordStartwithoutspace, strCAtegorySearchWordStartInnerwithoutspace, 2).Copy();


            //FOR GETTING SUB CATEGORY FROM SEARCH

            string[] arrSubCategory = { "SubCategory" };

            string strsubCAtegorySearch = "";
            string strsCAtegorySearchWordStart = "", strsCAtegorySearchWordStartInner = "";

            foreach (string cname in arrSubCategory)
            {
                strsubCAtegorySearch += cname + " like '%" + Searchname + "%' or ";
            }
            strsubCAtegorySearch = strsubCAtegorySearch.Trim().TrimEnd('o', 'r');

            string strsubCAtegorySearchwithoutspace = "";
            string strsCAtegorySearchWordStartwithoutspace = "", strsCAtegorySearchWordStartInnerwithoutspace = "";
            foreach (string cname in arrSubCategory)
            {
                strsubCAtegorySearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
            }
            strsubCAtegorySearchwithoutspace = strsubCAtegorySearchwithoutspace.Trim().TrimEnd('o', 'r');

            string strsubCAtegoryTableName = "tblSubCategory";
            dtSubCategory = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrSubCategory, strsubCAtegorySearch, strsubCAtegorySearchwithoutspace, strsubCAtegoryTableName, 4, strsCAtegorySearchWordStart, strsCAtegorySearchWordStartInner, strsCAtegorySearchWordStartwithoutspace, strsCAtegorySearchWordStartInnerwithoutspace, 2).Copy();



            //FOR GETTING PRODUCTS FROM SEARCH

            string[] arrProduct = { "Name" };

            string strProductSearch = "";
            string strProductSearchWordStart = "", strProductSearchWordStartInner = "";

            foreach (string cname in arrProduct)
            {
                strProductSearch += cname + " like '%" + Searchname + "%' or ";
                //strProductSearchWordStart += cname + " like '" + Searchname + "%' or ";
                //strProductSearchWordStartInner += cname + " like '% " + Searchname + "%' or ";
            }
            strProductSearch = strProductSearch.Trim().TrimEnd('o', 'r');
            //strProductSearchWordStart = strProductSearchWordStart.Trim().TrimEnd('o', 'r');
            //strProductSearchWordStartInner = strProductSearchWordStartInner.Trim().TrimEnd('o', 'r');

            string strProductSearchwithoutspace = "";
            string strProductSearchWordStartwithoutspace = "", strProductSearchWordStartInnerwithoutspace = "";

            foreach (string cname in arrProduct)
            {
                strProductSearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
                //strProductSearchWordStartwithoutspace += cname + " like '" + Searchnamewithoutspace + "%' or ";
                //strProductSearchWordStartInnerwithoutspace += cname + " like '% " + Searchnamewithoutspace + "%' or ";
            }
            strProductSearchwithoutspace = strProductSearchwithoutspace.Trim().TrimEnd('o', 'r');
            //strProductSearchWordStartwithoutspace = strProductSearchWordStartwithoutspace.Trim().TrimEnd('o', 'r');
            //strProductSearchWordStartInnerwithoutspace = strProductSearchWordStartInnerwithoutspace.Trim().TrimEnd('o', 'r');


            string strProductTableName = "Product";
            dtProduct = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrProduct, strProductSearch, strProductSearchwithoutspace, strProductTableName, 3, strProductSearchWordStart, strProductSearchWordStartInner, strProductSearchWordStartwithoutspace, strProductSearchWordStartInnerwithoutspace, 2).Copy();


        }

        private DataTable GetProductSearchTable(string Searchname, DataTable dtResult, string Searchnamewithoutspace, bool isSpaceStringSame, string[] arrSeller, string strSellerSearch, string strSellerSearchwithoutspace, string strTableName, int Type, string strSellerSearchWordStart, string strSellerSearchWordStartInner, string strSellerSearchWordStartwithoutspace, string strSellerSearchWordStartInnerwithoutspace, int Searchfrom)//Searchfrom = 1 for search button 2 for display list on key press
        {
            DataTable dtSeller = new DataTable();
            string tablename = strTableName;
            try
            {
                if (Searchfrom == 1)
                {
                    strTableName = strTableName + "btn";
                }

                //if (Application[strTableName] != null)
                //{
                //    dtSeller = (DataTable)Application[strTableName];
                //    if (dtSeller.Rows.Count == 0)
                //    {
                //        dtSeller = GetAllData(tablename, arrSeller);
                //        Application[strTableName] = dtSeller;
                //    }
                //}
                //else
                //{
                dtSeller = GetAllData(tablename, arrSeller);
                //Application[strTableName] = dtSeller;
                //}

            }
            catch (Exception E)
            {
                dtSeller = GetAllData(tablename, arrSeller);
                Application[strTableName] = dtSeller;
            }

            if (dtSeller != null && dtSeller.Rows.Count > 0)
            {
                // if (strTableName != "tblCategoryMaster")
                //{
                //It will search word start with string
                SearchWordStartWith(Searchname, dtResult, strSellerSearchWordStart, Type, dtSeller, 1);

                //It will search word start within string
                SearchWordStartWithInner(Searchname, dtResult, strSellerSearchWordStartInner, Type, dtSeller, 2);
                //if (Searchfrom != 1) //1=search button 
                //{
                //    //It will search same word
                //    SearchSameWord(Searchname, dtResult, strSellerSearch, Type, dtSeller, 3);
                //}
                //It will search wihtout space word start with string
                //SearchWordStartWith(Searchname, dtResult, strSellerSearchWordStartwithoutspace, Type, dtSeller, 4);

                //It will search wihtout space word start within string
                // SearchWordStartWithInner(Searchname, dtResult, strSellerSearchWordStartInnerwithoutspace, Type, dtSeller, 5);

                if (Searchfrom != 1) //1=search button 
                {
                    //It will search wihtout space same word
                    SearchSameWordRemoveAllSpaceAvailability(Searchname, dtResult, Type, dtSeller, 6);
                }

                //It will search reverse string of search word
                SearchReverseofSearchWord(Searchname, dtResult, Searchnamewithoutspace, isSpaceStringSame, strSellerSearchwithoutspace, Type, dtSeller, 7);

                // if (Searchfrom == 1)
                {
                    //It will search all words availability in the string
                    SearchAllWordsAvailability(Searchname, dtResult, Type, dtSeller, 8);
                }
                DataView dv = dtResult.DefaultView;
                dv.Sort = "SearchType asc";//,Total_Word desc
                dtResult = dv.ToTable();
                // }
                //else
                // {
                //     DataView dv = dtSeller.DefaultView;
                //     dtResult = dv.ToTable();
                // }

            }
            return dtResult;
        }

        public DataTable GetAllData(string Tablename, string[] arrSeller)
        {
            //DataTable dt = new DataTable();
            try
            {
                string[] filter_for_daiplay = { "producttag" };
                //SqlConnection sqlcon = GetConnection();
                //SqlCommand sqlcmd = new SqlCommand();
                //sqlcmd.Connection = sqlcon;

                string append = "";
                string append1 = "";

                int display_count = 0;
                foreach (string cname in arrSeller)
                {
                    if (!filter_for_daiplay.Contains(cname.ToLower()))
                    {
                        display_count++;
                    }
                }

                int count = 0;
                foreach (string cname in arrSeller)
                {
                    count++;
                    if (count != arrSeller.Length)
                    {
                        append += "replace(replace((isnull(" + cname + ",'')), '''', ''),'’','')+ ', ' +";
                    }
                    else
                    {
                        append += "replace(replace((isnull(" + cname + ",'')), '''', ''),'’','')";
                    }

                    if (!filter_for_daiplay.Contains(cname.ToLower()))
                    {
                        if (count != display_count)
                        {
                            append1 += "isnull(" + cname + ",'')+ ', ' +";
                        }
                        else
                        {
                            append1 += "isnull(" + cname + ",'')";
                        }
                    }
                }

                append = ",(" + append + ") as Name";
                //append1 = ",(" + append1 + ") as Name1";
                string query = "";
                if (Tablename == "Product")
                {
                    //append1 = ",(" + append1 + ")" + " + ' (' + cm.CategoryName + ')' as Name1";
                    //query = "select *" + append + append1 + " from " + Tablename + " pm INNER JOIN Category cm on pm.CategoryId = cm.CategoryID ";

                    //append1 = ",(" + append1 + ")" + " + ' (' + cm.CategoryName + ')' as Name1";
                    append1 = ",(" + append1 + ") as Name1";
                    query = "select *" + append + append1 + " from " + Tablename + " pm INNER JOIN tblCategoryProductLink cpl on pm.Id = cpl.ProductId ";
                }
                else
                {
                    append1 = ",(" + append1 + ") as Name1";
                query = "select *" + append + append1 + " from " + Tablename;
                }
                //SqlDataAdapter sqladapter = new SqlDataAdapter(query, sqlcon);
                //sqladapter.Fill(dt);

                DataTable dt = dbc.GetDataTable(query);
                if (Tablename == "Product")
                {
                    dt.Columns["Id"].ColumnName = "ROWID";
                    //dt.Columns["Name2"].ColumnName = "Link";
                    dt.Columns["SubCategoryId1"].ColumnName = "Link";
                }

                if (Tablename == "Category")
                {
                    dt.Columns["CategoryID"].ColumnName = "ROWID";
                    dt.Columns["CategoryName"].ColumnName = "Link";
                }
                if (Tablename == "tblSubCategory")
                {
                    dt.Columns["ID"].ColumnName = "ROWID";
                    dt.Columns["SubCategory"].ColumnName = "Link";
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return dt;
        }
        private static void SearchWordStartWith(string Searchname, DataTable dtResult, string strSellerSearch, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindsearch = null;

            if (Searchname.Trim() != "")
            {
                DataRow[] drfind = dtSeller.Select(strSellerSearch);
                drfindsearch = drfind;
            }
            if (drfindsearch != null)
            {
                foreach (DataRow dr in drfindsearch)
                {
                    //int SearchType = 0; //same search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void SearchWordStartWithInner(string Searchname, DataTable dtResult, string strSellerSearch, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindsearch = null;

            if (Searchname.Trim() != "")
            {
                DataRow[] drfind = dtSeller.Select(strSellerSearch);
                drfindsearch = drfind;
            }
            if (drfindsearch != null)
            {
                foreach (DataRow dr in drfindsearch)
                {
                    //int SearchType = 1; //same search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void AddResultRow(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, DataRow dr, int SearchType)
        {
            DataRow drnew = dtResult.NewRow();

            //drnew["ID"] = dr["ROWID"].ToString();
            drnew["ID"] = dr[0].ToString();
            string data = dr["Name1"].ToString().Trim();
            data = data.Trim().TrimEnd(',');
            data = data.Trim().TrimEnd(',');

            drnew["Name"] = data;
            drnew["Link"] = dr["Link"].ToString();
            //if(SearchType == 1)
            drnew["Type"] = Type;
            //else
            //    drnew["Type"] = Convert.ToInt64(dr[1].ToString());
            drnew["SearchType"] = SearchType;
            //if (SearchType != 1)
            if(Type ==3)
                drnew["CategoryId"] = dr[55].ToString();
            else
            drnew["CategoryId"] = dr[1].ToString();
            //else if (SearchType == 1)
            //    drnew["CategoryId"] = Convert.ToInt64(dr[0].ToString());
            string strAppend = "";
            foreach (DataColumn dc in dtSeller.Columns)
            {
                string cname = dc.ColumnName;
                if (cname == "Name")
                    strAppend += dr[cname].ToString();
            }
            var matches = Regex.Matches(strAppend, Searchname, RegexOptions.IgnoreCase);
            drnew["Total_Word"] = matches.Count;
            if (matches.Count > 0)
                dtResult.Rows.Add(drnew);
        }
        private static void SearchSameWordRemoveAllSpaceAvailability(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, int SearchType)
        {
            // char[] ch = { ' ' };
            // string[] spltSearch = Searchname.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            // if (spltSearch.Length > 1)
            // {
            foreach (DataRow dr in dtSeller.Rows)
            {
                string searchwords = dr["Name"].ToString();
                searchwords = searchwords.Replace(" ", "");
                bool isAllow = true;
                int findindex = searchwords.ToLower().IndexOf(Searchname.ToLower());

                if (findindex == -1)
                {
                    isAllow = false;
                }

                if (isAllow)
                {
                    //int SearchType = 4;//Remove All Space from search word and check search keyword available in the search phrase
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
            //}
        }
        private static void SearchAllWordsAvailability(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, int SearchType)
        {
            char[] ch = { ' ' };
            string[] spltSearch = Searchname.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (spltSearch.Length > 1)
            {
                foreach (DataRow dr in dtSeller.Rows)
                {
                    string searchwords = dr["Name"].ToString();
                    int indexno = 0;
                    bool isAllow = true;
                    foreach (string strcheck in spltSearch)
                    {
                        int findindex = searchwords.ToLower().IndexOf(strcheck.ToLower());

                        if (findindex != -1 && findindex == 0)
                        {

                        }
                        else
                        {
                            string strcheck1 = " " + strcheck;
                            findindex = searchwords.ToLower().IndexOf(strcheck1.ToLower());

                            if (findindex == -1)
                            {
                                isAllow = false;
                                break;
                            }
                        }
                    }

                    if (isAllow)
                    {
                        //int SearchType = 3;//All Space word available in the search phrase
                        AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                    }
                }
            }
        }
        private static void SearchReverseofSearchWord(string Searchname, DataTable dtResult, string Searchnamewithoutspace, bool isSpaceStringSame, string strSellerSearchwithoutspace, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindwithoutspace = null;

            if (!isSpaceStringSame && Searchnamewithoutspace.Trim() != "")
            {
                DataRow[] drfind1 = dtSeller.Select(strSellerSearchwithoutspace);
                drfindwithoutspace = drfind1;
            }
            if (drfindwithoutspace != null)
            {
                foreach (DataRow dr in drfindwithoutspace)
                {
                    //int SearchType = 2;//reverse of search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void AppendCategorySearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["Link"].ToString());
                sb.Append(recordSplitPattern);

                //sb.Append(drnew["RowID"].ToString());
                sb.Append("2");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
        private static void AppendSubCategorySearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                // sb.Append(drnew["SubCategoryName"].ToString());
                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                //sb.Append(drnew["SubCategoryName"].ToString());
                //sb.Append(drnew["Link"].ToString());
                sb.Append(drnew["CategoryId"].ToString());
                sb.Append(recordSplitPattern);

                //sb.Append(drnew["RowID"].ToString());
                sb.Append("4");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
        private static void AppendSearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["Link"].ToString());
                sb.Append(recordSplitPattern);

                //sb.Append(drnew["Type"].ToString());
                sb.Append("3");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["CategoryId"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
    }
}
