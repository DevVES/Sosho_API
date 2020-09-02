using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.MasterData
{
    public class TagModels
    {
        public class TagMaster
        {
            public string Response;
            public string Message;
            public List<TagMasterDataList> TagList { get; set; }
        
        }

        public class TagMasterDataList
        { 
            public string Tid;
            public string Tname;
            

        
        }
    }
}