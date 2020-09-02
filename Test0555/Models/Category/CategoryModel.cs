using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Category
{

    public class CategoryModel
    {
        public class getCategory
        {
            public List<CategoryDataList> CategoryList { get; set; }
            public string response;
            public string message;
        }

        public class CategoryDataList
        {
            public string Id { get; set; }
            public string CategoryName { get; set; }
            public string CategoryDescription { get; set; }
            public string CategoryImage { get; set; }
        }
    }
}