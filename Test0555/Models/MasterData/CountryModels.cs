using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class CountryModels
    {
        public class CountryMaster
        {
            public string Response;
            public string Message;

            public List<CountryDatalist> Countrylist { get; set; }
                
        }
        public class CountryDatalist
        {
            public string Countryid;
            public string CountryCode;
            public string Countryshortname;
            public string Countryname;
        }
    }
}