using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models;
using Test0555.Models.ProductManagement;
namespace Test0555.Controllers
{
    public class ProductController : ApiController
    {
        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        [HttpGet]
        //20-08-2020 Developed By :- Hiren
        public ProductModel.getNewproduct GetDashBoardProductDetails(string JurisdictionID, string CategoryId = "", string StartNo = "", string EndNo = "")
        {
            ProductModel.getNewproduct objeprodt = new ProductModel.getNewproduct();

            try
            {

                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                //string querystr = "select top 1 * from IntermediateBanners where IsActive=1 and IsDeleted=0 and TypeId = 1 and StartDate>='" + startdate + "' and StartDate<='" + startend + "' order by Id desc";
                //DataTable dtmain = dbc.GetDataTable(querystr);
                //if (dtmain != null && dtmain.Rows.Count > 0)
                //{
                //    string querydata = "select KeyValue from StringResources where KeyName='TopBannerImageUrl'";
                //    DataTable dtpath = dbc.GetDataTable(querydata);
                //    if (dtpath != null && dtpath.Rows.Count > 0)
                //    {
                //        objeprodt.response = "1";
                //        objeprodt.message = "Successfully";
                //        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                //        //objeprodt.TopBannerImages = new List<ProductModel.TopBannerImage>();
                //        for (int i = 0; i < dtmain.Rows.Count; i++)
                //        {
                //            string Id = dtmain.Rows[i]["Id"].ToString();
                //            string ImageName1 = dtmain.Rows[i]["ImageName"].ToString();
                //            //objeprodt.TopBannerImages.Add(new ProductModel.TopBannerImage
                //            //{
                //            //    bannerURL = urlpath + ImageName1,
                //            //    bannerId = Id
                //            //});
                //            objeprodt.bannerURL = urlpath + ImageName1;
                //            objeprodt.bannerId = Id;
                //        }
                //    }
                //    else
                //    {
                //        objeprodt.response = "0";
                //        objeprodt.message = "Top Banner Image Details Not Found";
                //    }
                //}
                //else
                //{
                //    objeprodt.response = "0";
                //    objeprodt.message = "Top Banner Image Details Not Found";

                //}

                //string subquerystr = "select top 1 * from IntermediateBanners where IsActive=1 and IsDeleted=0 and TypeId = 2 ";
                //subquerystr += "and StartDate>='" + startdate + "' and StartDate<='" + startend + "' order by Id desc";
                //DataTable dtsubmain = dbc.GetDataTable(subquerystr);
                //if (dtsubmain != null && dtsubmain.Rows.Count > 0)
                //{
                //    string subquerydata = "select KeyValue from StringResources where KeyName='SecondBannerImageUrl'";
                //    DataTable dtsecondpath = dbc.GetDataTable(subquerydata);
                //    if (dtsecondpath != null && dtsecondpath.Rows.Count > 0)
                //    {
                //        objeprodt.response = "1";
                //        objeprodt.message = "Successfully";
                //        string urlpath = dtsecondpath.Rows[0]["KeyValue"].ToString();
                //        // objeprodt.SecondBannerImages = new List<ProductModel.SecondBannerImage>();
                //        for (int i = 0; i < dtsubmain.Rows.Count; i++)
                //        {
                //            string Id = dtsubmain.Rows[i]["Id"].ToString();
                //            string ImageName1 = dtsubmain.Rows[i]["ImageName"].ToString();
                //            //objeprodt.SecondBannerImages.Add(new ProductModel.SecondBannerImage
                //            //{
                //            //    bannerURL = urlpath + ImageName1,
                //            //    bannerId = Id
                //            //});
                //            objeprodt.SecondbannerURL = urlpath + ImageName1;
                //            objeprodt.SecondbannerId = Id;
                //        }
                //    }
                //    else
                //    {
                //        objeprodt.response = "0";
                //        objeprodt.message = "Second Banner Image Details Not Found";
                //    }
                //}
                //else
                //{
                //    objeprodt.response = "0";
                //    objeprodt.message = "Second Banner Image Details Not Found";

                //}

                string sWhatappNo = "";
                string subquerystr = "select isnull(Mobile,'') as Mobile From users where JurisdictionID =  " + JurisdictionID;
                DataTable dtsubmain = dbc.GetDataTable(subquerystr);
                if (dtsubmain != null && dtsubmain.Rows.Count > 0)
                {
                    sWhatappNo = dtsubmain.Rows[0]["Mobile"].ToString();
                }
                string querymain = " with pte as ( select ROW_NUMBER() over(order by convert(int,Isnull(Product.DisplayOrder,'0'))) as RowNumber,  Product.id as Pid, [Product].[IsQtyFreeze],";
                querymain += "(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,";
                querymain += " Product.unit+' - '+UnitMaster.UnitName as DUnit,(CONVERT(varchar,EndDate,103)+' '+ CONVERT(varchar,EndDate,108)) as edate,";
                querymain += "CONVERT(varchar(12),EndDate,107)+' '+CONVERT(varchar(20),EndDate,108) as Enddate1, ";
                querymain += " isnull(cat.CategoryName, '') as CategoryName,isnull(Name, '') as [Name],isnull(sold, '') as [sold],isnull(ProductBanner, '') as ProductBanner,";
                querymain += " isnull(DisplayOrder, '') as DisplayOrder,isnull(Recommended,'') as Recommended,isnull(ProductDiscription,'') as ProductDiscription,isnull(Note,'') as Note,isnull(KeyFeatures,'') as KeyFeatures,";
                querymain += " case when isnull(ProductDiscription,'') = '' then 'false' else 'true' end as IsProductDetails,";
                querymain += " case when isnull(ProductTemplateID,'') = '2' then 'true' else 'false' end as Productvariant,";
                querymain += " case when isnull(Recommended,'') = '' then 'false' else 'true' end as IsSoshoRecommended,";
                querymain += "case when isnull(ProductBanner,'') = '' then 'false' else 'true' end as IsSpecialMessage ";
                querymain += "  ,Product.CategoryID,ProductMRP AS mrp,discount,DiscountType,SoshoPrice,MaxQty,MinQty,case when isnull(IsProductDescription,'') = '1' then 'true' else 'false' end as IsProductDescription ";
                querymain += " from Product ";
                querymain += " inner join Unitmaster on Unitmaster.id=Product.UnitId ";
                querymain += " inner join Category cat on cat.CategoryID = Product.CategoryID ";
                querymain += " Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                querymain += "and Product.IsActive = 1 and Product.IsDeleted = 0 and Isnull(Product.IsApproved,'') = 1 and Product.JurisdictionID =" + JurisdictionID;
                if (!string.IsNullOrEmpty(CategoryId))
                {
                    querymain += " and Product.CategoryID =" + CategoryId;
                }
                querymain += " ) select * From pte where RowNumber between " + StartNo + " and " + EndNo;
                //querymain += "Order By convert(int,Isnull(Product.DisplayOrder,'0'))";
                DataTable dtproduct = dbc.GetDataTable(querymain);
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
                    objeprodt.ProductList = new List<ProductModel.NewProductDataList>();
                    string sProductId = "", sMrp = "", sDiscount = "", sEdate = "", sPname = "", sPDiscount = "", sSoshoPrice = "", sSold = "", sProductBanner = "";
                    string sDUnit = "", sDisplayOrder = "", sMaxQty = "", sMinQty = "", sCategoryId = "", sCategory = "", sProductvariant = "", sIsSoshoRecommended = "";
                    string sIsSpecialMessage = "", sProductDiscription = "", sIsProductDetails = "", sRecommended = "", sProductNotes = "", sProductKeyFeatures = "", sIsProductDescription="";
                    decimal dDiscount = 0;
                    Boolean bIsQtyFreeze = false;
                    for (int i = 0; i < dtproduct.Rows.Count; i++)
                    {
                        sProductId = dtproduct.Rows[i]["Pid"].ToString();

                        ProductModel.NewProductDataList objProduct = new ProductModel.NewProductDataList();
                        ProductModel.ProductDataImagelist dataImagelist = new ProductModel.ProductDataImagelist();
                        ProductModel.ProductAttributelist attributelist = new ProductModel.ProductAttributelist();

                        if (urlpathimg != "")
                        {

                            string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + sProductId;
                            DataTable dtdetails = dbc.GetDataTable(ImageDetails);

                            if (dtdetails != null && dtdetails.Rows.Count > 0)
                            {
                                string productid3 = sProductId;
                                string proimgid = dtdetails.Rows[0]["id"].ToString();
                                string Imagename = dtdetails.Rows[0]["ImageFileName"].ToString();
                                string pdisorder = dtdetails.Rows[0]["DisplayOrder"].ToString();

                                dataImagelist.proimagid = proimgid;
                                dataImagelist.PImgname = urlpathimg + Imagename;
                                dataImagelist.prodid = sProductId;
                                dataImagelist.PDisOrder = pdisorder;

                                objProduct.ProductImageList.Add(dataImagelist);
                                //objProduct.ProductImageList = urlpathimg + Imagename;

                                //objProduct.ProductImageList = dataImagelist;
                            }
                        }
                        if (Attribuepathimg != "")
                        {
                            string AttImageDetails = "SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails,*  FROM Product_ProductAttribute_Mapping pam inner join Unitmaster um on um.id=pam.UnitId where pam.productid=" + sProductId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                            DataTable dtAttdetails = dbc.GetDataTable(AttImageDetails);

                            if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                            {
                                string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "", sAPDiscount = "", sisSelected="";
                                Boolean bAisOutOfStock = false, bAisSelected = false;
                                for (int j = 0; j < dtAttdetails.Rows.Count; j++)
                                {
                                    attributelist = new ProductModel.ProductAttributelist();
                                    sApackSizeId = dtAttdetails.Rows[j]["Id"].ToString();
                                    sAMrp = dtAttdetails.Rows[j]["Mrp"].ToString();
                                    sADiscount = dtAttdetails.Rows[j]["Discount"].ToString();
                                    if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "%")
                                        sAPDiscount = sADiscount.ToString() + "% Off";
                                    else if (dtAttdetails.Rows[j]["DiscountType"].ToString() == "Fixed")
                                        sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
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

                                    //if (dtAttdetails.Rows[j]["isSelected"].ToString() == "1")
                                    //    bAisSelected = true;
                                    //else
                                    //    bAisSelected = false;

                                    sisSelected = dtAttdetails.Rows[j]["isSelectedDetails"].ToString();

                                    attributelist.Mrp = sAMrp;
                                    //attributelist.Discount = sADiscount;
                                    attributelist.Discount = sAPDiscount;
                                    attributelist.PackingType = sAPackingType;
                                    attributelist.soshoPrice = sAsoshoPrice;
                                    attributelist.weight = sAweight;
                                    attributelist.AImageName = Attribuepathimg + sAImage;
                                    attributelist.isOutOfStock = bAisOutOfStock.ToString();
                                    attributelist.isSelected = sisSelected;
                                    attributelist.packSizeId = sApackSizeId;
                                    objProduct.ProductAttributesList.Add(attributelist);
                                }

                            }
                        }
                        sCategoryId = dtproduct.Rows[i]["CategoryId"].ToString();
                        sCategory = dtproduct.Rows[i]["CategoryName"].ToString();
                        sMrp = dtproduct.Rows[i]["mrp"].ToString();
                        sDiscount = dtproduct.Rows[i]["Discount"].ToString();

                        if (dtproduct.Rows[i]["DiscountType"].ToString() == "%")
                            sPDiscount = sDiscount.ToString() + "% Off";
                        else if (dtproduct.Rows[i]["DiscountType"].ToString() == "Fixed")
                            sPDiscount = CommonString.rusymbol + " " + sDiscount.ToString() + " Off";
                        else
                            sPDiscount = "";

                        dDiscount = 0;
                        decimal.TryParse(sDiscount.ToString(), out dDiscount);


                        
                        objProduct.MRP = sMrp;
                        //objProduct.Discount = dDiscount.ToString();
                        objProduct.Discount = sPDiscount.ToString();
                        sPname = dtproduct.Rows[i]["Name"].ToString();
                        sEdate = dtproduct.Rows[i]["edate"].ToString();
                        sSoshoPrice = dtproduct.Rows[i]["SoshoPrice"].ToString();
                        sSold = dtproduct.Rows[i]["sold"].ToString();
                        sProductBanner = dtproduct.Rows[i]["ProductBanner"].ToString();
                        sDUnit = dtproduct.Rows[i]["DUnit"].ToString();
                        sDisplayOrder = dtproduct.Rows[i]["DisplayOrder"].ToString();
                        sProductvariant = dtproduct.Rows[i]["Productvariant"].ToString();
                        sIsSoshoRecommended = dtproduct.Rows[i]["IsSoshoRecommended"].ToString();
                        sIsSpecialMessage = dtproduct.Rows[i]["IsSpecialMessage"].ToString();
                        sIsProductDescription = dtproduct.Rows[i]["IsProductDescription"].ToString();
                        sIsProductDetails = dtproduct.Rows[i]["IsProductDetails"].ToString();
                        sRecommended = dtproduct.Rows[i]["Recommended"].ToString();
                        sProductDiscription = dtproduct.Rows[i]["ProductDiscription"].ToString();
                        sProductNotes = dtproduct.Rows[i]["Note"].ToString();
                        sProductKeyFeatures = dtproduct.Rows[i]["KeyFeatures"].ToString();

                        if (dtproduct.Rows[i]["IsQtyFreeze"].ToString() == "1")
                            bIsQtyFreeze = true;
                        else
                            bIsQtyFreeze = false;


                        sMaxQty = dtproduct.Rows[i]["MaxQty"].ToString();
                        sMinQty = dtproduct.Rows[i]["MinQty"].ToString();

                        objProduct.CategoryId = sCategoryId;
                        objProduct.CategoryName = sCategory;
                        objProduct.Name = sPname;
                        objProduct.OfferEndDate = sEdate;
                        objProduct.SellingPrice = sSoshoPrice;
                        objProduct.SoldCount = sSold;
                        objProduct.SpecialMessage = sProductBanner;
                        objProduct.Weight = sDUnit;
                        objProduct.DisplayOrder = sDisplayOrder;
                        //objProduct.IsProductDetails = sIsProductDetails;
                        objProduct.IsProductVariant = sProductvariant;
                        objProduct.IsQtyFreeze = bIsQtyFreeze.ToString();
                        objProduct.SoshoRecommended = sRecommended;
                        objProduct.IsSoshoRecommended = sIsSoshoRecommended;
                        objProduct.IsSpecialMessage = sIsSpecialMessage;
                        objProduct.MaxQty = sMaxQty;
                        objProduct.MinQty = sMinQty;
                        objProduct.IsProductDescription = sIsProductDescription;
                        objProduct.ProductDescription = sProductDiscription;
                        objProduct.ProductNotes = sProductNotes;
                        objProduct.ProductKeyFeatures = sProductKeyFeatures;
                        objProduct.ProductId = sProductId;

                        objeprodt.ProductList.Add(objProduct);
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
