using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using InquiryManageAPI.Controllers;
using Test0555.Models.Category;
using System.Data;

namespace Test0555.Controllers
{
    public class CategoryController : ApiController
    {
        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        [HttpGet]
        //20-08-2020 Developed By :- Hiren
        public CategoryModel.getCategory GetDashBoardCategoryDetails()
        {
            CategoryModel.getCategory objeprodt = new CategoryModel.getCategory();

            try
            {

                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";
                string querystr = "select  * from Category where IsActive=1 and IsDeleted=0  and Createdon>='" + startdate + "' and Createdon<='" + startend + "' order by CategoryID desc";
                DataTable dtmain = dbc.GetDataTable(querystr);
                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='CategoryImageUrl'";
                    DataTable dtpath = dbc.GetDataTable(querydata);
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        objeprodt.response = "1";
                        objeprodt.message = "Successfully";
                        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                        objeprodt.CategoryList = new List<CategoryModel.CategoryDataList>();
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string Id = dtmain.Rows[i]["CategoryId"].ToString();
                            string CategoryName = dtmain.Rows[i]["CategoryName"].ToString();
                            string CategoryDescription = dtmain.Rows[i]["CategoryDescription"].ToString();
                            string ImageName1 = dtmain.Rows[i]["CategoryImage"].ToString();
                            objeprodt.CategoryList.Add(new CategoryModel.CategoryDataList
                            {
                                
                                Id = Id,
                                CategoryName = CategoryName,
                                CategoryDescription = CategoryDescription,
                                CategoryImage = urlpath + ImageName1,
                            });
                        }
                    }
                    else
                    {
                        objeprodt.response = "0";
                        objeprodt.message = "Category Details Not Found";
                    }
                }
                else
                {
                    objeprodt.response = "0";
                    objeprodt.message = "Category Details Not Found";

                }
                return objeprodt;
            }
            catch (Exception ex)
            {
                objeprodt.response = CommonString.Errorresponse;
                objeprodt.message = ex.StackTrace + " " + ex.Message;
                return objeprodt;
            }
        }
    }
}