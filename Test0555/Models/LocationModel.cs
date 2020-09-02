using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GarageXAPINEW.Models
{
    public class LocationModel 
    {
        #region Classes For Location
        public class GeneralObject
        {
            public String response = "";
            public String response_message = "";
            public String message = "";
            public String Name = "";
            public String Id = "";
            public String OTP = "";
        }
        public class clsCommonResponce
        {
            public clsCommonResponce()
            {
                Message = "Fail";
                ResultFlag = "0";
            }
            public string ResultFlag { get; set; }
            public string Message { get; set; }
        }

        public class clsLatestLocation
        {
            public clsLatestLocation()
            {
                Message = "Fail";
                ResultFlag = "0";
                Latitude = "";
                Longitude = "";
                LastUpdatedOn = "";
            }
            public string ResultFlag { get; set; }
            public string Message { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string LastUpdatedOn { get; set; }
        }
        public class clsPickUpRequest
        {
            public clsPickUpRequest()
            {
                Message = "Fail";
                ResultFlag = "0";
                SourceLatitude = "";
                SourceLongitude = "";
                DestinationLatitude = "";
                DestinationLongitude = "";
                RequestedOn = "";
            }
            public string ResultFlag { get; set; }
            public string Message { get; set; }
            public string SourceLatitude { get; set; }
            public string SourceLongitude { get; set; }
            public string DestinationLatitude { get; set; }
            public string DestinationLongitude { get; set; }
            public string RequestedOn { get; set; }
        }
        #endregion

        //public class GeneralObject
        //{
        //    public String response = "";
        //    public String response_message = "";
        //    public String message = "";
        //    public String Name = "";
        //    public String Id = "";
        //    public String OTP = "";
        //}

        public class LocationRegionNeighborObject
        {
            public String response = "";
            public String response_message = "";
            public String message = "";
            public String Name = "";
            public String Id = "";
            public List<NeighborObject> NeighborList { get; set; }
        }

        public class NeighborObject
        {
            public String Name = "";
            public String Id = "";
            public String Nearest = "";
        }
    }
}