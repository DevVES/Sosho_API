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
                string querystr = "select  * from Category where IsActive=1 and IsDeleted=0  and Createdon>='" + startdate + "' and Createdon<='" + startend + "' order by ISDefault desc";
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
                           // objeprodt.SubCategoryList = new List<CategoryModel.SubCategoryDataList>();

                            CategoryModel.CategoryDataList objCategory = new CategoryModel.CategoryDataList();

                            string Id = dtmain.Rows[i]["CategoryId"].ToString();
                            string CategoryName = dtmain.Rows[i]["CategoryName"].ToString();
                            string CategoryDescription = dtmain.Rows[i]["CategoryDescription"].ToString();
                            string ImageName1 = dtmain.Rows[i]["CategoryImage"].ToString();
                            objCategory.Id = Id;
                            objCategory.CategoryName = CategoryName;
                            objCategory.CategoryDescription = CategoryDescription;
                            objCategory.CategoryImage = urlpath + ImageName1;

                            objeprodt.CategoryList.Add(objCategory);
                            //objeprodt.CategoryList.Add(new CategoryModel.CategoryDataList
                            //{

                            //    Id = Id,
                            //    CategoryName = CategoryName,
                            //    CategoryDescription = CategoryDescription,
                            //    CategoryImage = urlpath + ImageName1,
                            //});
                            CategoryModel.SubCategoryDataList objSubCategory = new CategoryModel.SubCategoryDataList();
                            objCategory.SubCategoryList = new List<CategoryModel.SubCategoryDataList>();
                            string subquerystr = "select  Id AS SubCategoryId, SubCategory,Description, CategoryId from tblSubCategory where IsActive=1 and IsDeleted=0  and CategoryId = " + Id;
                            DataTable subdtmain = dbc.GetDataTable(subquerystr);
                            if (subdtmain != null && subdtmain.Rows.Count > 0)
                            {
                                for (int j = 0; j < subdtmain.Rows.Count; j++)
                                {
                                    objSubCategory = new CategoryModel.SubCategoryDataList();
                                    string categoryId = subdtmain.Rows[j]["CategoryId"].ToString();
                                    string subcategoryId = subdtmain.Rows[j]["SubCategoryId"].ToString();
                                    string SubCategoryName = subdtmain.Rows[j]["SubCategory"].ToString();
                                    string SubCategoryDescription = subdtmain.Rows[j]["Description"].ToString();
                                    objSubCategory.CategoryId = categoryId;
                                    objSubCategory.SubCategoryId = subcategoryId;
                                    objSubCategory.SubCategoryName = SubCategoryName;
                                    objSubCategory.SubCategoryDescription = SubCategoryDescription;
                                    objCategory.SubCategoryList.Add(objSubCategory);
                                    
                                }
                                //objeprodt.SubCategoryList = objCategory.SubCategoryList;
                            }
                            
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