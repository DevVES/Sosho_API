using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.CheckPincode;

namespace Test0555.Controllers
{
    public class CheckPincodeController : ApiController
    {

        dbConnection dbCon = new dbConnection();

        [HttpGet]
        public CheckPincodeModel CheckPincode(String Pincode)
        {
            CheckPincodeModel objchkpincode = new CheckPincodeModel();
            DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
            //int pincode = 0;
            try
            {
                if (Pincode != null && Pincode != "" && Pincode.Length == 6)
                {
                    foreach (DataRow dr in ShipperList.Rows)
                    {
                        int ShipperId = int.Parse(dr["Id"].ToString());
                        int pincode = int.Parse(Pincode);

                        var value = dbCon.CheckAvailability(pincode, ShipperId);

                        if (value == 0)
                        {
                            objchkpincode.resultflag = "0";
                            objchkpincode.Message = "Delivery is not available in this area.";
                            objchkpincode.JurisdictionID = "0";
                            
                        }
                        else
                        {
                            DataTable dtJurisdiction = dbCon.GetAllJurisdiction(pincode);
                            if(dtJurisdiction.Rows.Count > 0)
                            {
                                objchkpincode.resultflag = "1";
                                objchkpincode.Message = "Service available at " + pincode;
                                objchkpincode.JurisdictionID = dtJurisdiction.Rows[0]["JurisdictionID"].ToString();
                            }
                            else
                            {
                                objchkpincode.resultflag = "0";
                                objchkpincode.Message = "Delivery is not available in this area.";
                                objchkpincode.JurisdictionID = "0";
                            }
                        }
                    }
                    string Querydata = " SELECT Top 1  Z.Id, C.Id AS CityId, C.CityName, S.Id AS StateId, S.StateName " +
                                   " from ZipCode Z " +
                                   " LEFT JOIN CityMaster C ON C.CityName = Z.District " +
                                   " LEFT JOIN StateMaster S ON S.StateName = Z.State " +
                                   " where Z.IsActive = 1 AND Z.Zipcode = " + Pincode +
                                   " Order by 1 desc ";
                    DataTable dtPincode = dbCon.GetDataTable(Querydata);
                    if (dtPincode != null && dtPincode.Rows.Count > 0)
                    {
                        string cityId = dtPincode.Rows[0]["CityId"].ToString();
                        string cityname = dtPincode.Rows[0]["CityName"].ToString();
                        string stateid = dtPincode.Rows[0]["StateId"].ToString();
                        string Statename = dtPincode.Rows[0]["StateName"].ToString();
                        objchkpincode.CountryID = "1";
                        objchkpincode.CountryName = "India";
                        objchkpincode.StateID = stateid;
                        objchkpincode.StateName = Statename;
                        objchkpincode.CityId = cityId;
                        objchkpincode.CityName = cityname;
                    }
                    }
                else
                {
                    objchkpincode.resultflag = "0";
                    objchkpincode.Message = "Enter Valid Pincode";
                    objchkpincode.JurisdictionID = "0";

                }

            }
            catch (Exception ex)
            {

            }
            return objchkpincode;
        }

        [HttpGet]
        public CheckPincodeModel.PinCodeDetailList GetStateCityDetails(string pincode)
        {
            CheckPincodeModel.PinCodeDetailList objpincode = new CheckPincodeModel.PinCodeDetailList();
            try
            {
                string Querydata = " SELECT Top 1  Z.Id, C.Id AS CityId, C.CityName, S.Id AS StateId, S.StateName " +
                                   " from ZipCode Z " +
                                   " LEFT JOIN CityMaster C ON C.CityName = Z.District " +
                                   " LEFT JOIN StateMaster S ON S.StateName = Z.State " +
                                   " where Z.IsActive = 1 AND Z.Zipcode = " + pincode +
                                   " Order by 1 desc ";
                DataTable dtPincode = dbCon.GetDataTable(Querydata);
                if (dtPincode != null && dtPincode.Rows.Count > 0)
                {
                    objpincode.Response = CommonString.successresponse;
                    objpincode.Message = CommonString.successmessage;
                    string cityId = dtPincode.Rows[0]["CityId"].ToString();
                    string cityname = dtPincode.Rows[0]["CityName"].ToString();
                    string stateid = dtPincode.Rows[0]["StateId"].ToString();
                    string Statename = dtPincode.Rows[0]["StateName"].ToString();
                    objpincode.CountryID = "1";
                    objpincode.CountryName = "India";
                    objpincode.StateID = stateid;
                    objpincode.StateName = Statename;
                    objpincode.CityId = cityId;
                    objpincode.CityName = cityname;
                    
                }
                else
                {
                    objpincode.Response = CommonString.DataNotFoundResponse;
                    objpincode.Message = CommonString.DataNotFoundMessage;
                }
                return objpincode;
            }
            catch (Exception ee)
            {
                objpincode.Response = CommonString.Errorresponse;
                objpincode.Message = ee.StackTrace;
                return objpincode;
            }
        }

        [HttpGet]
        public string SendWpMsgByServiceblePinCode()
        {
            try
            {
                DataTable dtpin = dbCon.GetDataTable("select distinct c.Mobile, ca.PinCode from Customer C inner join CustomerAddress CA on CA.CustomerId = C.Id inner join JurisdictionDetail d on d.PinCodeID = ca.PinCode where len(ltrim(rtrim(isnull(c.Mobile, '')))) >= 10 and ltrim(rtrim(isnull(c.Mobile, ''))) not in ('0000000000', '1234567688') and len(ltrim(rtrim(ca.PinCode))) = 6 and d.IsActive = 1 order by Pincode");
                if (dtpin.Rows.Count > 0)
                {
                    for (int i = 0; i < dtpin.Rows.Count; i++)
                    {
                        var zipcode = dtpin.Rows[i]["PinCode"].ToString();
                        var mobile = dtpin.Rows[i]["Mobile"].ToString();
                        DataTable dtwpurl = dbCon.GetDataTable("select Url FROM WhatsappUrls  where zipcode=" + zipcode);
                        string wpurl = string.Empty;
                        if (dtwpurl.Rows.Count > 0)
                        {
                            wpurl = dtwpurl.Rows[0]["Url"].ToString();
                            //string smstxt = "Join us to receive exciting offer from sosho click here " + wpurl;
                            string smstxt = "Fortune sunflower Rs2095 (2850)15ltr " + Environment.NewLine + "Gulab Groundnut Rs2195(3350)15ltr " + Environment.NewLine + "Amul ghee Rs438 (450)1ltr " + Environment.NewLine + "Madhur Sugar Rs212(270)5kg "  + Environment.NewLine + "COD.Free shipping  " + Environment.NewLine + "http://www.sosho.in";
                            dbCon.SendSMS(mobile, smstxt);
                        }

                    }
                }
                return "SMS Sent";

            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        [HttpGet]
        public string SendWpMsgByNonServiceblePinCode()
        {
            try
            {
                DataTable dtpin = dbCon.GetDataTable("select distinct c.Mobile, ca.PinCode from Customer C inner join CustomerAddress CA on CA.CustomerId = C.Id where len(ltrim(rtrim(isnull(c.Mobile, '')))) >= 10 and ltrim(rtrim(isnull(c.Mobile, ''))) not in ('0000000000', '1234567688') and len(ltrim(rtrim(ca.PinCode))) = 6 and ltrim(rtrim(ca.PinCode)) not in (select distinct ca.PinCode from Customer C inner join CustomerAddress CA on CA.CustomerId = C.Id inner join JurisdictionDetail d on d.PinCodeID = ca.PinCode where len(ltrim(rtrim(isnull(c.Mobile, '')))) >= 10 and ltrim(rtrim(isnull(c.Mobile, ''))) not in ('0000000000', '1234567688') and len(ltrim(rtrim(ca.PinCode))) = 6 and d.IsActive = 1) order by Pincode");
                if (dtpin.Rows.Count > 0)
                {
                    for (int i = 0; i < dtpin.Rows.Count; i++)
                    {
                        var zipcode = dtpin.Rows[i]["PinCode"].ToString();
                        var mobile = dtpin.Rows[i]["Mobile"].ToString();
                        //DataTable dtwpurl = dbCon.GetDataTable("select Url FROM WhatsappUrls  where zipcode=" + zipcode);
                        string wpurl = string.Empty;
                        //if (dtwpurl.Rows.Count > 0)
                        //{
                            wpurl = "https://chat.whatsapp.com/BvJLY7GP8Ss982etC669yS";
                            string smstxt = "Join us to receive exciting offer from sosho click here " + wpurl;
                            dbCon.SendSMS(mobile, smstxt);
                        //}

                    }
                }
                return "SMS Sent";

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        [HttpGet]
        public CheckJurisdictionModel CheckJurisdiction(string JurisdictionId,string PinCode)
        {
            CheckJurisdictionModel model = new CheckJurisdictionModel();
            try
            {
                if(!string.IsNullOrEmpty(JurisdictionId) && !string.IsNullOrEmpty(PinCode) && PinCode.Length >= 6 && JurisdictionId != "0")
                {
                    DataTable dt = dbCon.GetDataTable("Select * from JurisdictionDetail where JurisdictionID =" + JurisdictionId + " AND PinCodeID=" + PinCode);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        model.Response = CommonString.successresponse;
                        model.Message = CommonString.successmessage;
                    }
                    else
                    {
                        model.Response = CommonString.DataNotFoundResponse;
                        model.Message = "There might be some changes in price and availability of the product for the pin code in the selected address." +
                            "You will have to first enter this pin code in the Deliver To box at the top of the home screen and select the products again.";
                    }

                }
                else
                {
                    model.Response = CommonString.DataNotFoundResponse;
                    model.Message = "Invalid JurisdictionId or PinCode";
                }
                
            }
            catch(Exception ex)
            {
                model.Response = CommonString.Errorresponse;
                model.Message = ex.StackTrace;
            }
            return model;
        }

    }
}
