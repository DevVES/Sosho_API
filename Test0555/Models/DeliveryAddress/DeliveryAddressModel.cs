using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.DeliveryAddress
{
    public class DeliveryAddressModel
    {

        public class CustAddress
        {
            public string Response;
            public string Message;
            public string LastId;
            
        }

        public class CustAddressDataList
        {
            public string Custid;
            public string fname;
            public string lname;
            public string tagname;
            public string countryName;
            public string statename;
            public string cityname;
            public string addr;
            public string email;
            public string pcode;
            public string mob;
            public string CustomerAddressId;
            public string AreaId;
            public string Area;
            public string BuildingId;
            public string Building;
            public string BuildingNo;
            public string LandMark;
            public string OtherDetail;


        }

        public class CustAddressDetailsList
        {
            public string Response;
            public string Message;
            public List<CustAddressDataList> CustAddressList { get; set; }
        
        }


        public class CustEditAddress
        {
            public string Response;
            public string Message;

        }

        public class UpdateCustomerProfile
        {
            public string Response;
            public string Message;

        }
    }

}