using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class AreaModels
    {
        public class AreaBuildingList
        {
            public string Response;
            public string Message;

            public List<BuildingDatalist> Buildinglist { get; set; }
        }
        public class BuildingDatalist
        {
            public string AreaId;
            public string AreaName;
            public string BuildingId;
            public string BuildingName;
        }

        public class ZipCodeAreaList
        {
            public string Response;
            public string Message;

            public List<AreaDatalist> Arealist { get; set; }
        }
        public class AreaDatalist
        {
            //public string LocationId;
            //public string Location;
            public string AreaId;
            public string AreaName;
        }
    }
}