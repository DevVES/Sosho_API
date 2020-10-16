using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Controllers;
using Test0555.Models.Order;

namespace Test0555.Controllers.Order
{
    public class OrderSummeryController : ApiController
    {
        dbConnection dbc = new dbConnection();
        public OrderSummeryModels.OrderSummery GetOrderSummery(string custid, string AddressId, string BuyFlag)
        {
            
            OrderSummeryModels.OrderSummery objordresum = new OrderSummeryModels.OrderSummery();
            { 
              try 
	            {
                    string Querydata = "select CustomerAddress.Id as addrid,CustomerAddress.Address,CustomerAddress.CustomerId as custid,CustomerAddress.FirstName as fname,CustomerAddress.LastName as lname,(select tagname from TagMaster where TagMaster.id=CustomerAddress.TagId) as tagname,(select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId) as Countyname,CustomerAddress.MobileNo,(select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId) as statename,(select cityname from CityMaster where CityMaster.Id=CustomerAddress.CityId) as Cname,AreaId,BuildingId,BuildingNo,LandMark,OtherDetail,(select Area from Zipcode where Zipcode.IsActive=1 and Zipcode.id=CustomerAddress.AreaId) as AreaName,(select Building from tblBuilding where tblBuilding.IsActive=1 and tblBuilding.id=CustomerAddress.BuildingId) as BuildingName from CustomerAddress where CustomerAddress.CustomerId=" + custid + " and CustomerAddress.Id=" + AddressId + "";

                    DataTable dtmain = dbc.GetDataTable(Querydata);

                    
                      //temp Query data

                    //string ProductQuery = "select top 1 * from Product where StartDate<='21-Sep-2019 01:00:00' and EndDate>='20-sep-2019 02:59:59' and IsActive=1 and IsDeleted=0";
                    //DataTable dt = dbc.GetDataTable(ProductQuery);


                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        objordresum.Response = CommonString.successresponse;
                        objordresum.Message = CommonString.successmessage;
                        objordresum.OrderCustomerList = new List<OrderSummeryModels.OrderCustDataList>();

                        //for (int i = 0; i < dtmain.Rows.Count; i++)
                        //{
                        //string Custid = dtmain.Rows[i]["custid"].ToString();
                        string Custid = dtmain.Rows[0]["custid"].ToString();
                        string AddrId = dtmain.Rows[0]["addrid"].ToString();
                        string Fname = dtmain.Rows[0]["fname"].ToString();
                        string Lname = dtmain.Rows[0]["lname"].ToString();
                        string TagName = dtmain.Rows[0]["tagname"].ToString();
                        string CountryName = dtmain.Rows[0]["Countyname"].ToString();
                        string StateName = dtmain.Rows[0]["statename"].ToString();
                        string CityName = dtmain.Rows[0]["Cname"].ToString();
                        string MobileNo = dtmain.Rows[0]["MobileNo"].ToString();
                        string addr = dtmain.Rows[0]["Address"].ToString();
                        string areaid = dtmain.Rows[0]["AreaId"].ToString();
                        string buildingid = dtmain.Rows[0]["BuildingId"].ToString();
                        string buildingname = dtmain.Rows[0]["BuildingName"].ToString();
                        string areaname = dtmain.Rows[0]["AreaName"].ToString();
                        string buildingno = dtmain.Rows[0]["BuildingNo"].ToString();
                        string landmark = dtmain.Rows[0]["LandMark"].ToString();
                        string otherDetails = dtmain.Rows[0]["OtherDetail"].ToString();

                        objordresum.OrderCustomerList.Add(new OrderSummeryModels.OrderCustDataList
                        {
                            cid=custid,
                            Caddrid = AddrId,
                            Cfname=Fname,
                            Clname=Lname,
                            addr=addr,
                            tag=TagName,
                            Countryname=CountryName,
                            statename=StateName,
                            Cityname=CityName,
                            cph=MobileNo,
                            AreaId = areaid,
                            BuildingId =  buildingid,
                            AreaName = areaname,
                            BuildingName = buildingname,
                            BuildingNo = buildingno,
                            Landmark = landmark,
                            OtherDetails = otherDetails

                        });
    
                            
                        //}
                    }
                  else
                    {
                        objordresum.Response = CommonString.DataNotFoundResponse;
                        objordresum.Message = CommonString.DataNotFoundMessage;
                    }
                       

                    return objordresum;
            	}
	            catch (Exception ee)
	            {
                    objordresum.Response = CommonString.Errorresponse;
                    objordresum.Message = ee.StackTrace;
	            	return objordresum;
             	}
             }

        }

        //Buy Only One Flag Pass 1
        //Buy Only One with Frined Total 2 Flag pass 2
        //Buy Only One with 5 Frined Total 6 Flag pass 6

        public OrderSummeryModels.getproduct GetProductDetails(string BuyFlag)
        {
            OrderSummeryModels.getproduct objproductflag = new OrderSummeryModels.getproduct();

            try
            {
                int flagval = 0;
                int.TryParse(BuyFlag.ToString(),out flagval);

                string querymain = "select Product.id as Pid,(select top 1 taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";


                //iif(("+BuyFlag+"=1),Product.Mrp,iif(("+BuyFlag+"=2),Product.BuyWith1FriendExtraDiscount,iif(("+BuyFlag+"=6),Product.BuyWith5FriendExtraDiscount,'Data Not Found')))as FlagPrice,

                //Darshan Temp Data view
                //string querymain = "select  Product.id as Pid,(select  taxvalue from GstTaxCategory where GstTaxCategory.id=Product.GstTaxId)as Tax,Product.unit+' - '+UnitMaster.UnitName as DUnit,* from Product inner join Unitmaster on Unitmaster.id=Product.UnitId Where StartDate>='05-Sep-2019' and EndDate<='16-Sep-2019'";

                DataTable dtmain = dbc.GetDataTable(querymain);



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

                    //if (dtpathvid != null && dtpathvid.Rows.Count > 0)
                    //{
                    //    //vid Path

                    //    urlpathvid = dtpathimg.Rows[0]["KeyValue"].ToString();
                    //}

                    objproductflag.response = CommonString.successresponse;
                    objproductflag.message = CommonString.successmessage;
                    objproductflag.ProductList = new List<OrderSummeryModels.ProductDataList>();

                    for (int i = 0; i < dtmain.Rows.Count; i++)
                    {
                        string pid = dtmain.Rows[i]["Pid"].ToString();
                        string pname2 = dtmain.Rows[i]["Name"].ToString();
                        string pdec2 = dtmain.Rows[i]["MetaDesc"].ToString();
                        string pkey2 = dtmain.Rows[i]["KeyFeatures"].ToString();
                        string ptax2 = dtmain.Rows[i]["Tax"].ToString();
                        string punit = dtmain.Rows[i]["DUnit"].ToString();
                        string pnote2 = dtmain.Rows[i]["Note"].ToString();
                        string price = dtmain.Rows[i]["Mrp"].ToString();
                        //string price = dtmain.Rows[i]["FlagPrice"].ToString();
                        string pvid = dtmain.Rows[i]["VideoName"].ToString();
                        string poffer = dtmain.Rows[i]["Offer"].ToString();
                        string person2 = dtmain.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                        string person6 = dtmain.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                        string shipp = dtmain.Rows[i]["FixedShipRate"].ToString();
                        string psold = dtmain.Rows[i]["sold"].ToString();
                        string pboug = dtmain.Rows[i]["JustBougth"].ToString();
                        string Dunitdata = dtmain.Rows[i]["Dunit"].ToString();

                        decimal price1 = 0;
                        decimal.TryParse(price.ToString(), out price1);
                        decimal shippincgcharge = 0;
                        decimal.TryParse(shipp.ToString(), out shippincgcharge);


                        decimal pp2 = 0;
                        decimal.TryParse(person2.ToString(), out pp2);
                        decimal pp5 = 0;
                        decimal.TryParse(person6.ToString(), out pp5);


                        //1 peson Price 
                        int noofperson = 0;
                        int.TryParse(BuyFlag.ToString(), out noofperson);

                        string price1person = "";
                        if (noofperson == 1)
                        {
                            price1person = price1.ToString();
                        }
                        if (noofperson == 2)
                        {
                            price1person = person2;
                        }
                        if (noofperson == 6)
                        {
                            price1person = person6;
                        }




                        OrderSummeryModels.ProductDataList objProduct = new OrderSummeryModels.ProductDataList();
                        objProduct.pname = pname2;
                        objProduct.pdec = pdec2;
                        objProduct.pkey = pkey2;
                        objProduct.pnote = pnote2;
                        objProduct.pprice = price1person.ToString();
                        objProduct.pwight = punit;
                        objProduct.poffer = poffer;
                        //objProduct.pbuy2 = price2person.ToString();
                        //objProduct.pbuy5 = price5person.ToString();
                        objProduct.shipping = shippincgcharge.ToString();
                        objProduct.pvideo = pvid;
                        objProduct.psold = psold;
                        objProduct.pJustBougth = pboug;
                        objProduct.pJustBougth = Dunitdata;
                        objProduct.pgst = ptax2;
                      
                        //Multiple Image Singal Product details List
                        if (urlpathimg != "")
                        {
                            string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid=" + dtmain.Rows[i]["Pid"].ToString() + " and isnull(Isdeleted,0)=0";

                            //string ImageDetails = "SELECT top 1  [Id] ,[ImageFileName] ,Productid,DisplayOrder  FROM ProductImages where productid='1' and isnull(Isdeleted,0)=0";

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

                                    objProduct.ProductImageList.Add(new OrderSummeryModels.ProductDataImagelist
                                    {
                                        prodid = productid3,
                                        proimagid = proimgid,
                                        // PImgname = urlpathimg + Imagename,
                                        PImgname = Imagename,
                                        PDisOrder = pdisorder,
                                    });
                                }
                            }
                            objproductflag.ProductList.Add(objProduct);                            
                        }

                    }
                    
                }
                else
                {
                    objproductflag.response = CommonString.DataNotFoundResponse;
                    objproductflag.message = CommonString.DataNotFoundMessage;

                }
                return objproductflag;
            }
            catch (Exception)
            {
                objproductflag.response = CommonString.Errorresponse;
                objproductflag.message = "Somthing Wrong";
                return objproductflag;
            }

        }



     
    }
}

