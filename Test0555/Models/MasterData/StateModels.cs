using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class StateModels
    {

        public class StateMaster
        {
            public string Response;
            public string Message;

            public List<StateDataList> StateList { get; set; }

        }
        public class StateDataList
        {
            public string Sid;
            public string StateCode;
            public string StateShortName;
            public string stateName;
        }
    }
}