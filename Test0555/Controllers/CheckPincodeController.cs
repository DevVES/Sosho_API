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

    }
}
