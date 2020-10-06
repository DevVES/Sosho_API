using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.CheckPincode
{
    public class CheckPincodeModel
    {
        public String resultflag = "";
        public String Message = "";
        public string JurisdictionID = "";
        public string CountryID = "";
        public string CountryName = "";
        public string StateID = "";
        public string StateName = "";
        public string CityId = "";
        public string CityName = "";

        public class PinCodeDetailList
        {
            public string Response;
            public string Message;
            public string CountryID = "";
            public string CountryName = "";
            public string StateID = "";
            public string StateName = "";
            public string CityId = "";
            public string CityName = "";
        }
    }
}