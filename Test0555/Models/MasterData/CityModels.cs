using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class CityModels
    {
        public class CityMaster
        {
            public string Response;
            public string Message;

            public List<CityDataList> CityList { get; set; }

        }
        public class CityDataList
        {
            public string CityId;
            
            public string Cityshortname;
            public string Cityname;
        }
    }
}