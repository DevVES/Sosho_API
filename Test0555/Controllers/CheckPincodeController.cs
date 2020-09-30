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
                        objchkpincode.CountryID = "1";
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
       
    }
}
