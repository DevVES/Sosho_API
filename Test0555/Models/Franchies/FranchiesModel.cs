using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Frenchies
{
    public class FranchiesModel
    {
        public class AddFranchiesResponse
        {
            public string response;
            public string message;
            public String FranchieId = "";
        }
        public class AddFranchies
        {
            public string Name;
            public string Email;
            public string Mobile;
            public string PinCode;
            public string Address;
            public string CreatedBy;
        }
    }
}