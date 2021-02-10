using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using Test0555.Models.DeliveryAddress;

namespace Test0555.Controllers
{
    public class CustomerAddressController : ApiController
    {
        dbConnection dbc = new dbConnection();

        #region AddNewAddress
        [HttpGet]
        public DeliveryAddressModel.CustAddress AddAddress(string custid, string fname1, string lname, string tagid1, string Countryid1, string sid, string cid, string addr1, string pinid1, string mobile1, string Email)
        {
            DeliveryAddressModel.CustAddress objadd = new DeliveryAddressModel.CustAddress();
            try
            {

                string Insertdata = "Insert into CustomerAddress ([CustomerId] ,[FirstName] ,[LastName] ,[TagId] ,[CountryId] ,[StateId] ,[CityId] ,[Address] ,[MobileNo] ,[PinCode] ,[DOC] ,[DOM] ,[IsDeleted] ,[IsActive],[Email]) Values ('" + custid + "','" + fname1 + "','" + lname + "','" + tagid1 + "','" + Countryid1 + "','" + sid + "','" + cid + "','" + addr1 + "','" + mobile1 + "','" + pinid1 + "','" + dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") + "','" + dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") + "','0','1','" + Email + "')select SCOPE_IDENTITY();";

                string dtdata = dbc.ExecuteSQLScaler(Insertdata).ToString();
                int idlast = 0;
                int.TryParse(dtdata.ToString(), out idlast);

                if (idlast > 0)
                {
                    objadd.Response = CommonString.successresponse;
                    objadd.Message = CommonString.successmessage;
                    objadd.LastId = idlast.ToString();
                }
                else
                {
                    objadd.Response = CommonString.DataNotFoundResponse;
                    objadd.Message = CommonString.DataNotFoundMessage;
                    objadd.LastId = "";

                }

                return objadd;
            }
            catch (Exception ee)
            {
                objadd.Response = CommonString.Errorresponse;
                objadd.Message = ee.StackTrace;
                return objadd;
            }
        }
        #endregion

        #region AddNewAddressV4
        [HttpGet]
        public DeliveryAddressModel.CustAddress AddAddressV4(string custid, string name, string tagId, string countryId, string sid, string cid, string pincode, string mobile, string Email, string areaid, string area, string buildingid, string building, string buildingNo, string landmark, string others = "")
        {
            DeliveryAddressModel.CustAddress objadd = new DeliveryAddressModel.CustAddress();
            try
            {
                area = area.Replace("'", "''");
                building = building.Replace("'", "''");
                landmark = landmark.Replace("'", "''");
                others = others == null ? string.Empty : others.Replace("'", "''");
                name = name.Replace("'", "''");
                buildingNo = buildingNo.Replace("'", "''");

                string StateName = "";
                string getState = "SELECT StateName FROM [dbo].[StateMaster] WHERE Id = " + sid;
                DataTable dtState = dbc.GetDataTable(getState);
                if (dtState.Rows.Count > 0)
                {
                    StateName = dtState.Rows[0]["StateName"].ToString();
                }
                string CityName = "";
                string getCity = "SELECT CityName FROM [dbo].[CityMaster] WHERE Id = " + cid;
                DataTable dtCity = dbc.GetDataTable(getCity);
                if (dtState.Rows.Count > 0)
                {
                    CityName = dtCity.Rows[0]["CityName"].ToString();
                }
                if (areaid == "-1")
                {
                    string[] para1 = { area, pincode, StateName, CityName, "0", "1", dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") };
                    string areadata = "Insert into ZipCode ([Area] ,[zipcode] ,[State] ,[District] ,[IsDeleted] ,[IsActive],[CreatedOn]) Values (@1,@2,@3,@4,@5,@6,@7)select SCOPE_IDENTITY();";
                    int Val = dbc.ExecuteQueryWithParamsId(areadata, para1);
                    areaid = Val.ToString();
                }
                if (buildingid == "-1")
                {
                    string zipcodeid = "";
                    string getZipCodeId = "SELECT Id FROM [dbo].[ZipCode] WHERE ZipCode = '" + pincode + "' AND Area=" + "'" + area + "'";
                    DataTable dtZipCodeId = dbc.GetDataTable(getZipCodeId);
                    if (dtZipCodeId.Rows.Count > 0)
                    {
                        zipcodeid = dtZipCodeId.Rows[0]["Id"].ToString();
                        string[] para2 = { building, pincode, area, zipcodeid, "0", "1", dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") };
                        string areadata = "Insert into tblBuilding ([Building] ,[zipcode] ,[Area] ,[ZipCodeId] ,[IsDeleted] ,[IsActive],[CreatedOn]) Values (@1,@2,@3,@4,@5,@6,@7)select SCOPE_IDENTITY();";
                        int Val = dbc.ExecuteQueryWithParamsId(areadata, para2);
                        buildingid = Val.ToString();
                    }
                }
                string Insertdata = "Insert into CustomerAddress ([CustomerId] ,[FirstName],[TagId] ,[CountryId] ,[StateId] ,[CityId] ,[MobileNo] ,[PinCode] ," +
                                    " [DOC] ,[DOM] ,[IsDeleted] ,[IsActive],[Email],[AreaId],[BuildingId],[BuildingNo],[LandMark],[OtherDetail]) " +
                                    " Values ('" + custid + "','" + name + "','" + tagId + "','" + countryId + "','" + sid + "','" +
                                    cid + "','" + mobile + "','" + pincode + "','" +
                                    dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") + "','" +
                                    dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") + "','0','1','" + Email + "'," +
                                    areaid + "," + buildingid + ",'" + buildingNo + "','" + landmark + "','" + others + "')select SCOPE_IDENTITY();";

                string dtdata = dbc.ExecuteSQLScaler(Insertdata).ToString();
                int idlast = 0;
                int.TryParse(dtdata.ToString(), out idlast);


                if (idlast > 0)
                {


                    objadd.Response = CommonString.successresponse;
                    objadd.Message = CommonString.successmessage;
                    objadd.LastId = idlast.ToString();
                }
                else
                {
                    objadd.Response = CommonString.DataNotFoundResponse;
                    objadd.Message = CommonString.DataNotFoundMessage;
                    objadd.LastId = "";

                }

                return objadd;
            }
            catch (Exception ee)
            {
                objadd.Response = CommonString.Errorresponse;
                objadd.Message = ee.StackTrace;
                return objadd;
            }
        }
        #endregion
        #region ListOfAddress
        [HttpGet]
        public DeliveryAddressModel.CustAddressDetailsList AddressDairy(string custid)
        {
            DeliveryAddressModel.CustAddressDetailsList Objlistaddr = new DeliveryAddressModel.CustAddressDetailsList();
            try
            {

                string Insertdata = "select (select Tagname from TagMaster where TagMaster.Id=CustomerAddress.TagId) as Tagname,(Select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId) as statename,(Select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId) as CountryName,(Select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId) as CityName,* from CustomerAddress where IsActive=1 and IsDeleted=0 ";

                DataTable dtdata = dbc.GetDataTable(Insertdata);

                if (dtdata != null && dtdata.Rows.Count > 0)
                {
                    Objlistaddr.Response = CommonString.successresponse;
                    Objlistaddr.Message = CommonString.successmessage;
                    Objlistaddr.CustAddressList = new List<DeliveryAddressModel.CustAddressDataList>();

                    for (int i = 0; i < dtdata.Rows.Count; i++)
                    {

                        string custid1 = (dtdata.Rows[i]["CustomerId"] != null ? dtdata.Rows[i]["CustomerId"].ToString() : "");
                        string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                        string lname1 = "";// dtdata.Rows[i]["LastName"].ToString();
                        string tagname1 = (dtdata.Rows[i]["Tagname"] != null ? dtdata.Rows[i]["Tagname"].ToString() : "");
                        string country1 = (dtdata.Rows[i]["CountryName"] != null ? dtdata.Rows[i]["CountryName"].ToString() : "");
                        string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                        string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                        string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                        string mob1 = (dtdata.Rows[i]["MobileNo"] != null ? dtdata.Rows[i]["MobileNo"].ToString() : "");
                        string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");

                        Objlistaddr.CustAddressList.Add(new DeliveryAddressModel.CustAddressDataList
                        {
                            Custid = custid1,
                            fname = fnaem1,
                            lname = lname1,
                            tagname = tagname1,
                            countryName = country1,
                            statename = state1,
                            cityname = city1,
                            addr = addr1,
                            mob = mob1,
                            pcode = pin1
                        });
                    }

                }
                else
                {
                    Objlistaddr.Response = CommonString.DataNotFoundResponse;
                    Objlistaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objlistaddr;
            }
            catch (Exception ee)
            {
                Objlistaddr.Response = CommonString.Errorresponse;
                Objlistaddr.Message = ee.StackTrace;
                return Objlistaddr;
            }
        }

        [HttpGet]
        public DeliveryAddressModel.CustAddressDetailsList AddressDairy_V1(string custid)
        {
            DeliveryAddressModel.CustAddressDetailsList Objlistaddr = new DeliveryAddressModel.CustAddressDetailsList();
            try
            {

                string Insertdata = "select isnull((select Tagname from TagMaster where TagMaster.Id=CustomerAddress.TagId),'') as Tagname,isnull((Select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId),'') as statename,isnull((Select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId),'') as CountryName,isnull((Select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId),'') as CityName,* from CustomerAddress where IsActive=1 and IsDeleted=0 and CustomerAddress.CustomerId=" + custid;

                DataTable dtdata = dbc.GetDataTable(Insertdata);

                if (dtdata != null && dtdata.Rows.Count > 0)
                {
                    Objlistaddr.Response = CommonString.successresponse;
                    Objlistaddr.Message = CommonString.successmessage;
                    Objlistaddr.CustAddressList = new List<DeliveryAddressModel.CustAddressDataList>();

                    for (int i = 0; i < dtdata.Rows.Count; i++)
                    {
                        string custaddid = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["Id"].ToString() : "");
                        string custid1 = (dtdata.Rows[i]["CustomerId"] != null ? dtdata.Rows[i]["CustomerId"].ToString() : "");
                        string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                        string lname1 = "";// dtdata.Rows[i]["LastName"].ToString();
                        string tagname1 = (dtdata.Rows[i]["Tagname"] != null ? dtdata.Rows[i]["Tagname"].ToString() : "");
                        string country1 = (dtdata.Rows[i]["CountryName"] != null ? dtdata.Rows[i]["CountryName"].ToString() : "");
                        string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                        string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                        string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                        string mob1 = (dtdata.Rows[i]["MobileNo"] != null ? dtdata.Rows[i]["MobileNo"].ToString() : "");
                        string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");
                        ;

                        Objlistaddr.CustAddressList.Add(new DeliveryAddressModel.CustAddressDataList
                        {
                            CustomerAddressId = custaddid,
                            Custid = custid1,
                            fname = fnaem1,
                            lname = lname1,
                            tagname = tagname1,
                            countryName = country1,
                            statename = state1,
                            cityname = city1,
                            addr = addr1,
                            mob = mob1,
                            pcode = pin1
                        });
                    }

                }
                else
                {
                    Objlistaddr.Response = CommonString.DataNotFoundResponse;
                    Objlistaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objlistaddr;
            }
            catch (Exception ee)
            {
                Objlistaddr.Response = CommonString.Errorresponse;
                Objlistaddr.Message = ee.Message;
                return Objlistaddr;
            }
        }

        [HttpGet]
        public DeliveryAddressModel.CustAddressDetailsList AddressDairy_V4(string custid)
        {
            DeliveryAddressModel.CustAddressDetailsList Objlistaddr = new DeliveryAddressModel.CustAddressDetailsList();
            try
            {

                string Insertdata = "select isnull((select Tagname from TagMaster where TagMaster.Id=CustomerAddress.TagId),'') as Tagname, " +
                                    " isnull((Select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId),'') as statename, " +
                                    " isnull((Select Area from ZipCode where ZipCode.Id=CustomerAddress.AreaId),'') as Area, " +
                                    " isnull((Select Building from tblBuilding where tblBuilding.Id=CustomerAddress.BuildingId),'') as Building, " +
                                    " isnull((Select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId),'') as CountryName, " +
                                    " isnull((Select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId),'') as CityName,*" +
                                    " from CustomerAddress where IsActive=1 and IsDeleted=0 and CustomerAddress.CustomerId=" + custid;

                DataTable dtdata = dbc.GetDataTable(Insertdata);
                Objlistaddr.CustAddressList = new List<DeliveryAddressModel.CustAddressDataList>();
                if (dtdata != null && dtdata.Rows.Count > 0)
                {
                    Objlistaddr.Response = CommonString.successresponse;
                    Objlistaddr.Message = CommonString.successmessage;


                    for (int i = 0; i < dtdata.Rows.Count; i++)
                    {
                        string custaddid = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["Id"].ToString() : "");
                        string custid1 = (dtdata.Rows[i]["CustomerId"] != null ? dtdata.Rows[i]["CustomerId"].ToString() : "");
                        string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                        string lname1 = "";// dtdata.Rows[i]["LastName"].ToString();
                        string tagname1 = (dtdata.Rows[i]["Tagname"] != null ? dtdata.Rows[i]["Tagname"].ToString() : "");
                        string country1 = (dtdata.Rows[i]["CountryName"] != null ? dtdata.Rows[i]["CountryName"].ToString() : "");
                        string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                        string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                        string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                        string mob1 = (dtdata.Rows[i]["MobileNo"] != null ? dtdata.Rows[i]["MobileNo"].ToString() : "");
                        string email = (dtdata.Rows[i]["Email"] != null ? dtdata.Rows[i]["Email"].ToString() : "");
                        string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");
                        string buildingId = (dtdata.Rows[i]["BuildingId"] != null ? dtdata.Rows[i]["BuildingId"].ToString() : "");
                        string building = (dtdata.Rows[i]["Building"] != null ? dtdata.Rows[i]["Building"].ToString() : "");
                        string AreaId = (dtdata.Rows[i]["AreaId"] != null ? dtdata.Rows[i]["AreaId"].ToString() : "");
                        string Area = (dtdata.Rows[i]["Area"] != null ? dtdata.Rows[i]["Area"].ToString() : "");
                        string buildingNo = (dtdata.Rows[i]["BuildingNo"] != null ? dtdata.Rows[i]["BuildingNo"].ToString() : "");
                        string landmark = (dtdata.Rows[i]["LandMark"] != null ? dtdata.Rows[i]["LandMark"].ToString() : "");
                        string otherdetail = (dtdata.Rows[i]["OtherDetail"] != null ? dtdata.Rows[i]["OtherDetail"].ToString() : "");
                        string stateId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["StateId"].ToString() : "");
                        string cityId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["CityId"].ToString() : "");
                        string countryId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["CountryId"].ToString() : "");
                        ;



                        Objlistaddr.CustAddressList.Add(new DeliveryAddressModel.CustAddressDataList
                        {
                            CustomerAddressId = custaddid,
                            Custid = custid1,
                            fname = fnaem1,
                            lname = lname1,
                            tagname = tagname1,
                            countryId = countryId,
                            countryName = country1,
                            stateId = stateId,
                            statename = state1,
                            cityId = cityId,
                            cityname = city1,
                            addr = addr1,
                            email = email,
                            mob = mob1,
                            pcode = pin1,
                            AreaId = AreaId,
                            Area = Area,
                            BuildingId = buildingId,
                            Building = building,
                            BuildingNo = buildingNo,
                            LandMark = landmark,
                            OtherDetail = otherdetail

                        });
                    }

                }
                else
                {
                    Objlistaddr.Response = CommonString.DataNotFoundResponse;
                    Objlistaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objlistaddr;
            }
            catch (Exception ee)
            {
                Objlistaddr.Response = CommonString.Errorresponse;
                Objlistaddr.Message = ee.Message;
                return Objlistaddr;
            }
        }

        [HttpGet]
        public DeliveryAddressModel.CustAddressDetailsList CustomenrInfo(string custid)
        {
            DeliveryAddressModel.CustAddressDetailsList Objlistaddr = new DeliveryAddressModel.CustAddressDetailsList();
            try
            {

                // string Insertdata = "select isnull((select Tagname from TagMaster where TagMaster.Id=CustomerAddress.TagId),'') as Tagname,isnull((Select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId),'') as statename,isnull((Select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId),'') as CountryName,isnull((Select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId),'') as CityName,* from CustomerAddress where IsActive=1 and IsDeleted=0 and CustomerAddress.CustomerId=" + custid;

                string Insertdata = "SELECT ISNULL ((SELECT StateName FROM  dbo.StateMaster WHERE (StateMaster.Id = dbo.Customer.StateId)), '') AS statename, ISNULL ((SELECT CityName FROM dbo.CityMaster WHERE (CityMaster.Id = dbo.Customer.CityId)), '') AS CityName, * FROM  dbo.Customer WHERE Customer.Id = " + custid;
                DataTable dtdata = dbc.GetDataTable(Insertdata);

                if (dtdata != null && dtdata.Rows.Count > 0)
                {
                    Objlistaddr.Response = CommonString.successresponse;
                    Objlistaddr.Message = CommonString.successmessage;
                    Objlistaddr.CustAddressList = new List<DeliveryAddressModel.CustAddressDataList>();

                    for (int i = 0; i < dtdata.Rows.Count; i++)
                    {

                        //string custid1 = (dtdata.Rows[i]["CustomerId"] != null ? dtdata.Rows[i]["CustomerId"].ToString() : "");
                        //string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                        //string lname1 = "";// dtdata.Rows[i]["LastName"].ToString();
                        //string tagname1 = (dtdata.Rows[i]["Tagname"] != null ? dtdata.Rows[i]["Tagname"].ToString() : "");
                        //string country1 = (dtdata.Rows[i]["CountryName"] != null ? dtdata.Rows[i]["CountryName"].ToString() : "");
                        //string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                        //string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                        //string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                        //string mob1 = (dtdata.Rows[i]["MobileNo"] != null ? dtdata.Rows[i]["MobileNo"].ToString() : "");
                        //string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");



                        string custid1 = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["Id"].ToString() : "");
                        string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                        string lname1 = (dtdata.Rows[i]["LastName"] != null ? dtdata.Rows[i]["LastName"].ToString() : "");
                        string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                        string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                        string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                        string mob1 = (dtdata.Rows[i]["Mobile"] != null ? dtdata.Rows[i]["Mobile"].ToString() : "");
                        string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");
                        string emailid = (dtdata.Rows[i]["Email"] != null ? dtdata.Rows[i]["Email"].ToString() : "");

                        Objlistaddr.CustAddressList.Add(new DeliveryAddressModel.CustAddressDataList
                        {
                            Custid = custid1,
                            fname = fnaem1,
                            lname = lname1,
                            statename = state1,
                            email = emailid,
                            cityname = city1,
                            addr = addr1,
                            mob = mob1,
                            pcode = pin1
                        });
                    }

                }
                else
                {
                    Objlistaddr.Response = CommonString.DataNotFoundResponse;
                    Objlistaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objlistaddr;
            }
            catch (Exception ee)
            {
                Objlistaddr.Response = CommonString.Errorresponse;
                Objlistaddr.Message = ee.Message;
                return Objlistaddr;
            }
        }


        #endregion

        #region UpdateAddress
        [HttpGet]
        public DeliveryAddressModel.CustEditAddress EditAddress(string custid2, string addrid2, string fname2, string lname2, string tagid2, string Countryid2, string sid2, string cid2, string addr2, string pinid2, string mobile2, string Emailid)
        {
            DeliveryAddressModel.CustEditAddress Objeditaddr = new DeliveryAddressModel.CustEditAddress();
            try
            {
                string Insertdata = "Update CustomerAddress set Email='" + Emailid + "',FirstName='" + fname2 + "',LastName='" + lname2 + "',TagId='" + tagid2 + "',CountryId='" + Countryid2 + "',StateId='" + sid2 + "',CityId='" + cid2 + "',Address='" + addr2 + "',MobileNo='" + mobile2 + "',PinCode='" + pinid2 + "',DOM='" + dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") + "' where CustomerId='" + custid2 + "' and Id='" + addrid2 + "'";

                int dtdata = dbc.ExecuteQuery(Insertdata);

                if (dtdata > 0)
                {
                    Objeditaddr.Response = CommonString.successresponse;
                    Objeditaddr.Message = CommonString.successmessage;
                }
                else
                {
                    Objeditaddr.Response = CommonString.DataNotFoundResponse;
                    Objeditaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objeditaddr;
            }
            catch (Exception ee)
            {
                Objeditaddr.Response = CommonString.Errorresponse;
                Objeditaddr.Message = ee.StackTrace;
                return Objeditaddr;
            }
        }

        [HttpGet]
        //public DeliveryAddressModel.CustAddress AddAddressV4(string custid, string name, string tagId, string countryId, string sid, string cid, string pincode, string mobile, string Email, string areaid, string area, string buildingid, string building, string buildingNo, string landmark, string others)
        public DeliveryAddressModel.CustEditAddress EditAddressV4(string custid, string addrid2, string name, string tagId, string countryId, string sid, string cid, string pincode, string mobile, string Email, string areaid, string area, string buildingid, string building, string buildingNo, string landmark, string others)
        {
            DeliveryAddressModel.CustEditAddress Objeditaddr = new DeliveryAddressModel.CustEditAddress();
            try
            {
                area = area.Replace("'", "''");
                building = building.Replace("'", "''");
                landmark = landmark.Replace("'", "''");
                others = others == null ? string.Empty : others.Replace("'", "''");
                name = name.Replace("'", "''");
                buildingNo = buildingNo.Replace("'", "''");


                string StateName = "";
                string getState = "SELECT StateName FROM [dbo].[StateMaster] WHERE Id = " + sid;
                DataTable dtState = dbc.GetDataTable(getState);
                if (dtState.Rows.Count > 0)
                {
                    StateName = dtState.Rows[0]["StateName"].ToString();
                }
                string CityName = "";
                string getCity = "SELECT CityName FROM [dbo].[CityMaster] WHERE Id = " + cid;
                DataTable dtCity = dbc.GetDataTable(getCity);
                if (dtState.Rows.Count > 0)
                {
                    CityName = dtCity.Rows[0]["CityName"].ToString();
                }
                if (areaid == "-1")
                {
                    string[] para1 = { area, pincode, StateName, CityName, "0", "1", dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") };
                    string areadata = "Insert into ZipCode ([Area] ,[zipcode] ,[State] ,[District] ,[IsDeleted] ,[IsActive],[CreatedOn]) Values (@1,@2,@3,@4,@5,@6,@7)select SCOPE_IDENTITY();";
                    int Val = dbc.ExecuteQueryWithParamsId(areadata, para1);
                    areaid = Val.ToString();
                }
                if (buildingid == "-1")
                {
                    string zipcodeid = "";
                    string getZipCodeId = "SELECT Id FROM [dbo].[ZipCode] WHERE ZipCode = '" + pincode + "' AND Area=" + "'" + area + "'";
                    DataTable dtZipCodeId = dbc.GetDataTable(getZipCodeId);
                    if (dtZipCodeId.Rows.Count > 0)
                    {
                        zipcodeid = dtZipCodeId.Rows[0]["Id"].ToString();
                        string[] para2 = { building, pincode, area, zipcodeid, "0", "1", dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") };
                        string areadata = "Insert into tblBuilding ([Building] ,[zipcode] ,[Area] ,[ZipCodeId] ,[IsDeleted] ,[IsActive],[CreatedOn]) Values (@1,@2,@3,@4,@5,@6,@7)select SCOPE_IDENTITY();";
                        int Val = dbc.ExecuteQueryWithParamsId(areadata, para2);
                        buildingid = Val.ToString();
                    }
                }

                string Insertdata = "Update CustomerAddress set Email='" + Email + "',FirstName='" + name + "',TagId='" + tagId +
                                    "',CountryId='" + countryId + "',StateId='" + sid + "',CityId='" + cid + "',MobileNo='" + mobile +
                                    "',PinCode='" + pincode + "',DOM='" + dbc.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss") +
                                    "',BuildingId = " + buildingid + ", AreaId = " + areaid + ",BuildingNo = '" + buildingNo +
                                    "',LandMark = '" + landmark + "',OtherDetail = '" + others +
                                    "' where CustomerId='" + custid + "' and Id='" + addrid2 + "'";
                int dtdata = dbc.ExecuteQuery(Insertdata);

                if (dtdata > 0)
                {
                    Objeditaddr.Response = CommonString.successresponse;
                    Objeditaddr.Message = CommonString.successmessage;
                }
                else
                {
                    Objeditaddr.Response = CommonString.DataNotFoundResponse;
                    Objeditaddr.Message = CommonString.DataNotFoundMessage;
                }

                return Objeditaddr;
            }
            catch (Exception ee)
            {
                Objeditaddr.Response = CommonString.Errorresponse;
                Objeditaddr.Message = ee.StackTrace;
                return Objeditaddr;
            }
        }
        #endregion

        #region CustomerProfileUpdate
        [HttpGet]
        public DeliveryAddressModel.UpdateCustomerProfile UpdateCustomerProfile(string custid, string fname, string lname, string email, string sex, string mobile)
        {
            DeliveryAddressModel.UpdateCustomerProfile objupdpro = new DeliveryAddressModel.UpdateCustomerProfile();
            try
            {

                string Insertdata = "Update Customer set FirstName='" + fname + "',LastName='" + lname + "',Email='" + email + "',Sex='" + sex + "',Mobile='" + mobile + "' where Id='" + custid + "'";

                int dtdata = dbc.ExecuteQuery(Insertdata);

                if (dtdata > 0)
                {
                    objupdpro.Response = CommonString.successresponse;
                    objupdpro.Message = CommonString.successmessage;
                }
                else
                {
                    objupdpro.Response = CommonString.DataNotFoundResponse;
                    objupdpro.Message = CommonString.DataNotFoundMessage;
                }

                return objupdpro;
            }
            catch (Exception ee)
            {
                objupdpro.Response = CommonString.Errorresponse;
                objupdpro.Message = ee.StackTrace;
                return objupdpro;
            }
        }

        [HttpGet]
        public DeliveryAddressModel.UpdateCustomerProfile UpdateCustomerProfile_V1(string custid, string fname, string lname, string email, string mobile, string address, string cityid, string stateid, string pincode)
        {
            DeliveryAddressModel.UpdateCustomerProfile objupdpro = new DeliveryAddressModel.UpdateCustomerProfile();
            try
            {

                string Insertdata = "Update Customer set FirstName='" + fname + "',LastName='" + lname + "',Address='" + address + "',Email='" + email + "',CityId='" + cityid + "',StateId='" + stateid + "',Pincode='" + pincode + "',Mobile='" + mobile + "' where Id='" + custid + "'";

                int dtdata = dbc.ExecuteQuery(Insertdata);

                if (dtdata > 0)
                {
                    objupdpro.Response = CommonString.successresponse;
                    objupdpro.Message = CommonString.successmessage;
                }
                else
                {
                    objupdpro.Response = CommonString.DataNotFoundResponse;
                    objupdpro.Message = CommonString.DataNotFoundMessage;
                }

                return objupdpro;
            }
            catch (Exception ee)
            {
                objupdpro.Response = CommonString.Errorresponse;
                objupdpro.Message = ee.StackTrace;
                return objupdpro;
            }
        }
        #endregion
    }
}
