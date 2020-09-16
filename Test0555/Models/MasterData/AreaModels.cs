using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class AreaModels
    {
        public class ZipCodeLocationList
        {
            public string Response;
            public string Message;

            public List<LocationDatalist> Locationlist { get; set; }
        }
        public class LocationDatalist
        {
            public string LocationId;
            public string LocationName;
        }

        public class LocationAreaList
        {
            public string Response;
            public string Message;

            public List<AreaDatalist> Arealist { get; set; }
        }
        public class AreaDatalist
        {
            public string LocationId;
            public string Location;
            public string AreaId;
            public string AreaName;
        }
    }
}