using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using InquiryManageAPI.Controllers;
using Test0555.Models.MasterData;

namespace Test0555.Controllers
{
    public class MasterDataController : ApiController
    {
        dbConnection dbc = new dbConnection();

        #region CountryList

        [HttpGet]
        public CountryModels.CountryMaster countrylist()
        {
            CountryModels.CountryMaster objcontry = new CountryModels.CountryMaster();
            try
            {
                string Querydata = "select * from countrymaster where IsActive=1";
                DataTable dtcountry = dbc.GetDataTable(Querydata);
                if (dtcountry != null && dtcountry.Rows.Count > 0)
                {
                    objcontry.Response = CommonString.successresponse;
                    objcontry.Message = CommonString.successmessage;
                    objcontry.Countrylist = new List<CountryModels.CountryDatalist>();
                    for (int i = 0; i < dtcountry.Rows.Count; i++)
                    {
                        string cid = dtcountry.Rows[i]["Id"].ToString();
                        string Ccode = dtcountry.Rows[i]["CountryCode"].ToString();
                        string csname = dtcountry.Rows[i]["CountryShortName"].ToString();
                        string cname = dtcountry.Rows[i]["CountryName"].ToString();

                        objcontry.Countrylist.Add(new CountryModels.CountryDatalist
                        {
                            Countryid = cid,
                            CountryCode = Ccode,
                            Countryshortname = csname,
                            Countryname = cname
                        });
                    }

                }
                else
                {
                    objcontry.Response = CommonString.DataNotFoundResponse;
                    objcontry.Message = CommonString.DataNotFoundMessage;


                }
                return objcontry;
            }
            catch (Exception ee)
            {
                objcontry.Response = CommonString.Errorresponse;
                objcontry.Message = ee.StackTrace;
                return objcontry;
            }
        }

        #endregion

        #region StateList

        [HttpGet]
        public StateModels.StateMaster Statelist(string userid)
        {
            StateModels.StateMaster objstate = new StateModels.StateMaster();
            try
            {
                string Querydata = "select * from StateMaster where IsActive=1";
                DataTable dtcountry = dbc.GetDataTable(Querydata);
                if (dtcountry != null && dtcountry.Rows.Count > 0)
                {
                    objstate.Response = CommonString.successresponse;
                    objstate.Message = CommonString.successmessage;
                    objstate.StateList = new List<StateModels.StateDataList>();
                    for (int i = 0; i < dtcountry.Rows.Count; i++)
                    {
                        string cid = dtcountry.Rows[i]["Id"].ToString();
                        string Ccode = dtcountry.Rows[i]["StateCode"].ToString();
                        string csname = dtcountry.Rows[i]["StateShortName"].ToString();
                        string cname = dtcountry.Rows[i]["StateName"].ToString();
                        objstate.StateList.Add(new StateModels.StateDataList
                        {
                            Sid = cid,
                            StateCode = Ccode,
                            StateShortName = csname,
                            stateName = cname
                        });
                    }

                }
                else
                {
                    objstate.Response = CommonString.DataNotFoundResponse;
                    objstate.Message = CommonString.DataNotFoundMessage;


                }
                return objstate;
            }
            catch (Exception ee)
            {
                objstate.Response = CommonString.Errorresponse;
                objstate.Message = ee.StackTrace;
                return objstate;
            }
        }

        #endregion

        #region CityList


        [HttpGet]
        public CityModels.CityMaster Citylist(string userid1)
        {
            CityModels.CityMaster Objcity = new CityModels.CityMaster();
            try
            {
                string Querydata = "select * from CityMaster where IsActive=1";
                DataTable dtcountry = dbc.GetDataTable(Querydata);
                if (dtcountry != null && dtcountry.Rows.Count > 0)
                {
                    Objcity.Response = CommonString.successresponse;
                    Objcity.Message = CommonString.successmessage;
                    Objcity.CityList = new List<CityModels.CityDataList>();
                    for (int i = 0; i < dtcountry.Rows.Count; i++)
                    {
                        string cid = dtcountry.Rows[i]["Id"].ToString();

                        string csname = dtcountry.Rows[i]["CityShortName"].ToString();
                        string cname = dtcountry.Rows[i]["CityName"].ToString();
                        Objcity.CityList.Add(new CityModels.CityDataList
                        {
                            CityId = cid,
                            Cityname = cname,
                            Cityshortname = csname
                        });
                    }

                }
                else
                {
                    Objcity.Response = CommonString.DataNotFoundResponse;
                    Objcity.Message = CommonString.DataNotFoundMessage;


                }
                return Objcity;
            }
            catch (Exception ee)
            {
                Objcity.Response = CommonString.Errorresponse;
                Objcity.Message = ee.StackTrace;
                return Objcity;
            }
        }

        #endregion

        #region TagList


        [HttpGet]
        public TagModels.TagMaster Taglist(string userid2)
        {
            TagModels.TagMaster objtag = new TagModels.TagMaster();
            try
            {
                string Querydata = "select * from Tagmaster where IsActive=1";
                DataTable dtcountry = dbc.GetDataTable(Querydata);
                if (dtcountry != null && dtcountry.Rows.Count > 0)
                {
                    objtag.Response = CommonString.successresponse;
                    objtag.Message = CommonString.successmessage;
                    objtag.TagList = new List<TagModels.TagMasterDataList>();
                    for (int i = 0; i < dtcountry.Rows.Count; i++)
                    {
                        string cid = dtcountry.Rows[i]["Id"].ToString();

                        string csname = dtcountry.Rows[i]["TagName"].ToString();

                        objtag.TagList.Add(new TagModels.TagMasterDataList
                        {
                            Tid = cid,
                            Tname = csname
                        });
                    }

                }
                else
                {
                    objtag.Response = CommonString.DataNotFoundResponse;
                    objtag.Message = CommonString.DataNotFoundMessage;


                }
                return objtag;
            }
            catch (Exception ee)
            {
                objtag.Response = CommonString.Errorresponse;
                objtag.Message = ee.StackTrace;
                return objtag;
            }
        }

        #endregion

        #region URL
        [HttpGet]
        public OfferCodeCheckModel.OfferCodecheck CheckURL(string Link, string OfferCode, string CustId = "")
        {
            OfferCodeCheckModel.OfferCodecheck objocode = new OfferCodeCheckModel.OfferCodecheck();
            try
            {

                if (Link != null && Link != "" && OfferCode != null && OfferCode != "")
                {
                    string Querydata = "SELECT top 1 [Order].[Id],[OrderItem].ProductId FROM [Order] Inner join OrderItem on OrderItem.OrderId=[Order].Id  where OrderItem.CustOfferCode='" + OfferCode + "'";
                    DataTable dtcountry = dbc.GetDataTable(Querydata);
                   // dbc.InsertLogs(LOGS.LogLevel.Error, "CheckUrl", "1");
                    if (dtcountry != null && dtcountry.Rows.Count > 0)
                    {

                        //dbc.InsertLogs(LOGS.LogLevel.Error, "CheckUrl", "2");
                        string proid = dtcountry.Rows[0]["ProductId"].ToString();
                        string where = "";
                        if (proid != "")
                        {
                            where = " and  Product.id=" + proid;
                        }

                        string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' " + where;

                        DataTable dtproduct = dbc.GetDataTable(expprostr);

                        if (dtproduct.Rows.Count == 0)
                        {
                            objocode.IsActive = "false";
                        }
                        else
                        {
                            objocode.IsActive = "true";
                        }
                        objocode.Response = CommonString.successresponse;
                        objocode.Message = CommonString.successmessage;
                    }
                    else
                    {
                        objocode.IsActive = "false";
                        objocode.Response = CommonString.DataNotFoundResponse;
                        objocode.Message = CommonString.DataNotFoundMessage;
                    }
                }

            }
            catch (Exception ee)
            {
                objocode.IsActive = "false";
                objocode.Response = CommonString.Errorresponse;
                objocode.Message = ee.StackTrace;
            }
            return objocode;
        }
        public static string getProductExpiredOnDate(string prodid)
        {
            dbConnection dbc = new dbConnection();
            string where = "";
            if (prodid == "")
            {
                where = "where Product.StartDate<='" + dbc.getindiantime().ToString("dd-MMM-yyyy") + " 00:00:00' AND EndDate>='" + dbc.getindiantime().ToString("dd-MMM-yyyy") + " 23:59:59'";
            }
            else
            {
                where = "where Product.Id=" + prodid + "";
            }

            string date = "select CONVERT(varchar,EndDate,106) as dateprod, CONVERT(varchar,EndDate,100)as timeprod from Product " + where + " ";
            DataTable dt = dbc.GetDataTable(date);
            string edate = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                edate = dt.Rows[0]["dateprod"].ToString();

                string timeeeee = "";

                string timeee = dt.Rows[0]["timeprod"].ToString();
                string[] time = timeee.Split(' ');
                int t1 = time.Length;
                timeeeee = time[t1 - 1];
                edate = edate + ' ' + timeeeee;
            }
            return edate;
        }
        #endregion

        #region Whatsappmsg
        [HttpGet]
        public static string HomepageMsg(string Msg, string Key)
        {
            dbConnection dbc = new dbConnection();
            string responce = "";
            try
            {

                string Message = Msg;
                string KeyVal = Key;
                if (Key != "" && Key != null)
                {
                    string Update = "UPDATE [dbo].[WhatsAppMsg] SET [Value] = '" + Message + "' WHERE [Key] ='" + KeyVal + "'";
                    int res = dbc.ExecuteQuery(Update);
                    if (res > 0)
                    {
                        return responce = "Success";
                    }
                    else
                    {
                        return responce = "Fail";
                    }
                }
                return responce = "Fail";
            }
            catch (Exception)
            {
                return responce = "Fail";
            }
        }


        [HttpGet]
        public ReturnMessageModel.MessageModel ReturnMessage(string Key)
        {
            ReturnMessageModel.MessageModel objmsg = new ReturnMessageModel.MessageModel();
            try
            {

                string givekey = "select top 1 Id,[Value] from WhatsAppMsg where [Key] = '" + Key + "'";
                DataTable dtgetkey = dbc.GetDataTable(givekey);

                if (dtgetkey != null && dtgetkey.Rows.Count > 0)
                {
                    objmsg.Message = dtgetkey.Rows[0]["Value"].ToString();
                    objmsg.Status = "Success";
                    objmsg.Response = "1";
                }
                else
                {
                    objmsg.Message = "";
                    objmsg.Status = "Fail";
                    objmsg.Response = "0";
                }


            }
            catch (Exception)
            {
                objmsg.Message = "";
                objmsg.Status = "Fail";
                objmsg.Response = "0";

            }

            return objmsg;
        }


        #endregion

        #region AREA
        [HttpGet]
        public AreaModels.ZipCodeLocationList LocationList(string zipcode)
        {
            AreaModels.ZipCodeLocationList objLocation = new AreaModels.ZipCodeLocationList();
            try
            {
                objLocation.Locationlist = new List<AreaModels.LocationDatalist>();
                string Querydata = "SELECT Id, Location from Zipcode where IsActive=1 AND ZipCode = '" + zipcode + "'";
                DataTable dtArea = dbc.GetDataTable(Querydata);
                if (dtArea != null && dtArea.Rows.Count > 0)
                {
                    objLocation.Response = CommonString.successresponse;
                    objLocation.Message = CommonString.successmessage;
                    
                    for (int i = 0; i < dtArea.Rows.Count; i++)
                    {
                        string id = dtArea.Rows[i]["Id"].ToString();
                        string location = dtArea.Rows[i]["Location"].ToString();

                        objLocation.Locationlist.Add(new AreaModels.LocationDatalist
                        {
                            LocationId = id,
                            LocationName = location
                        });
                    }
                }
                else
                {
                    objLocation.Response = CommonString.DataNotFoundResponse;
                    objLocation.Message = CommonString.DataNotFoundMessage;
                }
                return objLocation;
            }
            catch (Exception ee)
            {
                objLocation.Response = CommonString.Errorresponse;
                objLocation.Message = ee.StackTrace;
                return objLocation;
            }
        }

        [HttpGet]
        public AreaModels.LocationAreaList AreaList(string locationid)
        {
            AreaModels.LocationAreaList objArea = new AreaModels.LocationAreaList();
            try
            {
                objArea.Arealist = new List<AreaModels.AreaDatalist>();
                string Querydata = "SELECT Id, Area, Location from tblArea where IsActive=1 AND ZipCodeId = " + locationid;
                DataTable dtArea = dbc.GetDataTable(Querydata);
                if (dtArea != null && dtArea.Rows.Count > 0)
                {
                    objArea.Response = CommonString.successresponse;
                    objArea.Message = CommonString.successmessage;
                    
                    for (int i = 0; i < dtArea.Rows.Count; i++)
                    {
                        string id = dtArea.Rows[i]["Id"].ToString();
                        string Area = dtArea.Rows[i]["Area"].ToString();
                        string location = dtArea.Rows[i]["Location"].ToString();
                        objArea.Arealist.Add(new AreaModels.AreaDatalist
                        {
                            LocationId = locationid,
                            Location = location,
                            AreaId = id,
                            AreaName = Area
                        });
                    }
                }
                else
                {
                    objArea.Response = CommonString.DataNotFoundResponse;
                    objArea.Message = CommonString.DataNotFoundMessage;
                }
                return objArea;
            }
            catch (Exception ee)
            {
                objArea.Response = CommonString.Errorresponse;
                objArea.Message = ee.StackTrace;
                return objArea;
            }
        }
        #endregion

        #region Frenchies

        #endregion  



    }
}

