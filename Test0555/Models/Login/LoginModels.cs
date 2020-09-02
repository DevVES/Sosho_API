using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models
{
    public class LoginModels
    {
        public class MobileLogin
        {
            public string response;
            public string message;
            //public string Otp;
        }

        public class LoginOtp
        {
            public string response;
            public string message;

            public string userid;
            public string FirstName;
            public string LastName;
            public string MobileNo;            
            public string Email;
            public string Sex;          
        }
        public class VersionDataObject1
        {
            public string response { get; set; }
            public string response_message { get; set; }
            public string message { get; set; }
            public string Version = "";
            public string VersionCode = "";
            public bool IsForceUpdate = false;
            public string PlayStoreURL1 = "https://play.google.com/store/apps/details?id=com.sosho.android";
            public string PlayStoreURL2 = "market://details?id=com.sosho.android";
        }

    }
}