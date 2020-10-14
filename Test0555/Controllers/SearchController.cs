using InquiryManageAPI.Controllers;
using System;
using System.Data;
using System.Web.Http;
using Test0555.Models.ProductManagement;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
namespace Test0555.Controllers
{
    public class SearchController : ApiController
    {
        public HttpApplicationState Application
        {
            get;
        }


        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        [HttpGet]
        //14-10-2020 Search Related API New
        public ProductModel.SearchList SearchHintListV1(string SearchName, string JurisdictionID)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            var objectToSerialize = new ProductModel.SearchList();
            if (SearchName.Contains('\''))
            {
                SearchName = SearchName.Replace("'", "");
            }
            string Searchname = "", Searchnamewithoutspace = "";
            bool isSpaceStringSame = false;
            DataTable dtResult = new DataTable();
            string[] returnSearch = new string[2];
            try
            {
                if (SearchName != null && SearchName.Trim() != "")
                {
                    char[] ch = { ' ' };
                    string[] splt = SearchName.Split(ch, StringSplitOptions.RemoveEmptyEntries);
                    if (splt.Length > 1)
                    {
                        Searchname = "";
                        Searchname = Searchname.Trim();
                    }
                    else
                    {
                        Searchname = splt[0].Trim();
                    }

                    if (Searchname != "")
                    {
                        Searchnamewithoutspace = Searchname.Replace(" ", "").Trim();

                        if (Searchname.Equals(Searchnamewithoutspace))
                        {
                            isSpaceStringSame = true;
                        }
                    }
                    else
                    {
                        objectToSerialize.resultflag = "0";
                        objectToSerialize.Message = "Service is not available";
                    }
                }
                else
                {
                    objectToSerialize.resultflag = "0";
                    objectToSerialize.Message = "Service is not available";
                }

                DataTable dtSellerFinal = new DataTable();
                DataTable dtCategoryFinal = new DataTable();
                DataTable dtProductFinal = new DataTable();
                DataTable dtSubCategoryFinal = new DataTable();

                DataTable dtSeller = new DataTable();
                DataTable dtCategory = new DataTable();
                DataTable dtProduct = new DataTable();
                DataTable dtSubCategory = new DataTable();

                DataTable dtSeller_reverse = new DataTable();
                DataTable dtCategory_reverse = new DataTable();
                DataTable dtProduct_reverse = new DataTable();
                DataTable dtSubCategory_reverse = new DataTable();

                int Max_Seller = 10;
                int Max_Category = 10;
                int Max_Product = 10;
                int Max_SubCategory = 10;
                try
                {
                    dtResult.Columns.Add("ID", typeof(System.Int64));
                    dtResult.Columns.Add("Name");
                    dtResult.Columns.Add("Link");
                    dtResult.Columns.Add("Type", typeof(System.Int64));//1=Seller 2=Category 3=Product
                    dtResult.Columns.Add("Total_Word", typeof(System.Int64));
                    dtResult.Columns.Add("SearchType", typeof(System.Int64));//1=Direct Word  2=Word without space
                    dtResult.Columns.Add("CategoryId"); //CategoryId

                    if (Searchname.Trim() != "" || Searchnamewithoutspace.Trim() != "")
                    {
                        GetProductSearchResult(Searchname, Searchnamewithoutspace, isSpaceStringSame, dtResult, ref dtCategory, ref dtSubCategory, ref dtProduct, JurisdictionID);
                    }
                    else
                    {
                        dtCategory = dtResult.Clone();
                        dtProduct = dtResult.Clone();
                    }
                    int intCategory = dtCategory.Rows.Count;
                    int intProduct = dtProduct.Rows.Count;
                    int intSubCategory = dtSubCategory.Rows.Count;

                    int intCategory_reverse = dtCategory_reverse.Rows.Count;
                    int intProduct_reverse = dtProduct_reverse.Rows.Count;
                    int intSubCategory_reverse = dtSubCategory_reverse.Rows.Count;

                    dtCategoryFinal = dtCategory.Clone();
                    dtSubCategoryFinal = dtSubCategory.Clone();
                    dtProductFinal = dtProduct.Clone();

                    AddUniqueSearchResult(dtCategoryFinal, dtCategory, Max_Category);

                    if (dtCategoryFinal.Rows.Count != Max_Category)
                    {
                        AddUniqueSearchResult(dtCategoryFinal, dtCategory_reverse, Max_Category);
                    }

                    AddUniqueSearchResult(dtSubCategoryFinal, dtSubCategory, Max_Category);
                    if (dtSubCategoryFinal.Rows.Count != Max_Category)
                    {
                        AddUniqueSearchResult(dtSubCategoryFinal, dtSubCategory_reverse, Max_SubCategory);
                    }

                    AddUniqueSearchResult(dtProductFinal, dtProduct, Max_Product);

                    if (dtProductFinal.Rows.Count != Max_Product)
                    {
                        AddUniqueSearchResult(dtProductFinal, dtProduct_reverse, Max_Product);
                    }


                    if (dtCategoryFinal != null && dtCategoryFinal.Rows.Count > 0)
                    {
                        var Category = new ProductModel.SearchDetails();
                        Category.Header = "Category";
                        Category.Name = "";
                        Category.PageType = "";
                        Category.CategoryId = "";
                        Category.SubCategoryId = "";
                        Category.ProductId = "";

                        objectToSerialize.resultflag = "1";
                        objectToSerialize.Message = CommonString.successmessage;
                        objectToSerialize.ProductSearch.Add(Category);
                        foreach (DataRow dr in dtCategoryFinal.Rows)
                        {
                            var Categorysearch = new ProductModel.SearchDetails();
                            Categorysearch.Name = dr["Name"].ToString();
                            Categorysearch.PageType = "1";
                            Categorysearch.CategoryId = dr["ID"].ToString();
                            Categorysearch.SubCategoryId = "";
                            Categorysearch.ProductId = "";

                            objectToSerialize.resultflag = "1";
                            objectToSerialize.Message = CommonString.successmessage;

                            objectToSerialize.ProductSearch.Add(Categorysearch);
                        }
                    }
                    if (dtSubCategoryFinal != null && dtSubCategory.Rows.Count > 0)
                    {
                        var SubCategory = new ProductModel.SearchDetails();
                        SubCategory.Header = "SubCategory";
                        SubCategory.Name = "";
                        SubCategory.PageType = "";
                        SubCategory.CategoryId = "";
                        SubCategory.SubCategoryId = "";
                        SubCategory.ProductId = "";

                        objectToSerialize.resultflag = "1";
                        objectToSerialize.Message = CommonString.successmessage;
                        objectToSerialize.ProductSearch.Add(SubCategory);
                        foreach (DataRow dr in dtSubCategoryFinal.Rows)
                        {
                            var SubCategorysearch = new ProductModel.SearchDetails();
                            SubCategorysearch.Name = dr["Name"].ToString();
                            SubCategorysearch.PageType = "2";
                            SubCategorysearch.CategoryId = dr["CategoryId"].ToString();
                            SubCategorysearch.SubCategoryId = dr["ID"].ToString();
                            SubCategorysearch.ProductId = "";

                            objectToSerialize.resultflag = "1";
                            objectToSerialize.Message = CommonString.successmessage;

                            objectToSerialize.ProductSearch.Add(SubCategorysearch);
                        }
                    }
                    if (dtProductFinal != null && dtProductFinal.Rows.Count > 0)
                    {
                        var Product = new ProductModel.SearchDetails();
                        Product.Header = "Product";
                        Product.Name = "";
                        Product.PageType = "";
                        Product.CategoryId = "";
                        Product.SubCategoryId = "";
                        Product.ProductId = "";

                        objectToSerialize.resultflag = "1";
                        objectToSerialize.Message = CommonString.successmessage;
                        objectToSerialize.ProductSearch.Add(Product);
                        foreach (DataRow dr in dtProductFinal.Rows)
                        {
                            var Productsearch = new ProductModel.SearchDetails();
                            Productsearch.Name = dr["Name"].ToString();
                            Productsearch.PageType = "3";
                            Productsearch.CategoryId = dr["CategoryId"].ToString();
                            Productsearch.SubCategoryId = dr["ID"].ToString();
                            Productsearch.ProductId = dr["Link"].ToString();

                            objectToSerialize.resultflag = "1";
                            objectToSerialize.Message = CommonString.successmessage;
                            objectToSerialize.ProductSearch.Add(Productsearch);

                        }
                    }


                }
                catch (Exception err)
                {
                    objectToSerialize.resultflag = "0";
                    objectToSerialize.Message = "Service is not available";
                }
                
            }
            catch (Exception err)
            {
                objectToSerialize.resultflag = "0";
                objectToSerialize.Message = "Service is not available";
            }
            return objectToSerialize;
        }
        //08-10-2020 Search Related API
        [HttpGet]
        //public string[] GetResultsBySearch(string Searchname1)
        public ProductModel.getSearchproduct GetResultsBySearch(string Searchname1, string JurisdictionID)
        {
            if (Searchname1.Contains('\''))
            {
                Searchname1 = Searchname1.Replace("'", "");
            }

            string Searchname = "", Searchnamewithoutspace = "";
            bool isSpaceStringSame = false;
            DataTable dtResult = new DataTable();
            string[] returnSearch = new string[2];

            ProductModel.getSearchproduct objsearchproduct = new ProductModel.getSearchproduct();
            try
            {
                if (Searchname1 != null && Searchname1.Trim() != "")
                {
                    char[] ch = { ' ' };
                    string[] splt = Searchname1.Split(ch, StringSplitOptions.RemoveEmptyEntries);
                    if (splt.Length > 1)
                    {

                    }
                    else
                        Searchname = splt[0].Trim();

                    if (Searchname != "")
                    {
                        Searchnamewithoutspace = Searchname.Replace(" ", "").Trim();
                        if (Searchname.Equals(Searchnamewithoutspace))
                            isSpaceStringSame = true;
                    }
                    int Max_Seller = 10, Max_Category = 10, Max_Product = 10;
                    DataTable dtCategory = new DataTable();
                    DataTable dtProduct = new DataTable();
                    DataTable dtSubCategory = new DataTable();

                    DataTable dtCategoryFinal = new DataTable();
                    DataTable dtSubCategoryFinal = new DataTable();
                    DataTable dtProductFinal = new DataTable();

                    DataTable dtCategory_reverse = new DataTable();
                    DataTable dtProduct_reverse = new DataTable();
                    DataTable dtSubCategory_reverse = new DataTable();

                    try
                    {
                        dtResult.Columns.Add("ID", typeof(System.Int64));
                        dtResult.Columns.Add("Name");
                        dtResult.Columns.Add("Link");
                        dtResult.Columns.Add("Type", typeof(System.Int64));//1=Seller 2=Category 3=Product
                        dtResult.Columns.Add("Total_Word", typeof(System.Int64));
                        dtResult.Columns.Add("SearchType", typeof(System.Int64));//1=Direct Word  2=Word without space
                        dtResult.Columns.Add("CategoryId"); //CategoryId

                        if (Searchname.Trim() != "" || Searchnamewithoutspace.Trim() != "")
                        {
                            GetProductSearchResult(Searchname, Searchnamewithoutspace, isSpaceStringSame, dtResult, ref dtCategory, ref dtSubCategory, ref dtProduct, JurisdictionID);
                        }
                        else
                        {
                            dtCategory = dtResult.Clone();
                            dtProduct = dtResult.Clone();
                        }
                        int intCategory = dtCategory.Rows.Count;
                        int intProduct = dtProduct.Rows.Count;
                        int intSubCategory = dtSubCategory.Rows.Count;

                        int intCategory_reverse = dtCategory_reverse.Rows.Count;
                        int intProduct_reverse = dtProduct_reverse.Rows.Count;
                        int intSubCategory_reverse = dtSubCategory_reverse.Rows.Count;

                        dtCategoryFinal = dtCategory.Clone();
                        dtSubCategoryFinal = dtSubCategory.Clone();
                        dtProductFinal = dtProduct.Clone();

                        AddUniqueSearchResult(dtCategoryFinal, dtCategory, Max_Category);
                        AddUniqueSearchResult(dtSubCategoryFinal, dtSubCategory, Max_Category);
                        AddUniqueSearchResult(dtProductFinal, dtProduct, Max_Product);

                        if (dtProductFinal.Rows.Count != Max_Product)
                        {
                            AddUniqueSearchResult(dtProductFinal, dtProduct_reverse, Max_Product);
                        }

                        StringBuilder sb = new StringBuilder();

                        AppendCategorySearchResultString(dtCategoryFinal, sb);
                        AppendSubCategorySearchResultString(dtSubCategoryFinal, sb);
                        AppendSearchResultString(dtProductFinal, sb);

                        string finalstr = sb.ToString();
                        string[] arrstr = { "@@@@" };
                        returnSearch = finalstr.Split(arrstr, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (Exception ex)
                    {
                        objsearchproduct.response = "0";
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                objsearchproduct.response = CommonString.Errorresponse;

                throw ex;
            }
            objsearchproduct.response = "1";
            objsearchproduct.message = returnSearch;
            //return returnSearch;
            return objsearchproduct;
        }
        private static void AddUniqueSearchResult(DataTable dtSellerFinal, DataTable dtSeller, int Max_Seller)
        {
            foreach (DataRow dr in dtSeller.Rows)
            {
                if (Max_Seller == -1 || dtSellerFinal.Rows.Count < Max_Seller)
                {
                    string id = dr["Id"].ToString();
                    string Type = dr["Type"].ToString();

                    DataRow[] drfindRec = dtSellerFinal.Select("ID=" + id + " and Type=" + Type);
                    if (drfindRec.Length == 0)
                    {
                        dtSellerFinal.ImportRow(dr);
                    }
                }
                else
                {
                    break;
                }
            }
        }
        private void AddSubCategoryUniqueSearchResult(DataTable dtSellerFinal, DataTable dtSeller, int Max_Seller)
        {

            if (Max_Seller == -1 || dtSellerFinal.Rows.Count < Max_Seller)
            {
                string id = "";
                if (dtSeller != null)
                {
                    if (dtSeller.Rows.Count > 0)
                    {
                        DataTable dtSub = new DataTable();
                        string query = "";
                        query = "select *,(replace(replace((isnull(SubCategory,'')), '''', ''),'’','')) as Name,(isnull(SubCategory,'')) as Name1 from tblSubCategory where CategoryId =" + id;
                        dtSub = dbc.GetDataTable(query);

                        DataRow[] drfindRec = dtSub.Select("CategoryId=" + id);
                        foreach (DataRow dr in drfindRec)
                        {
                            dtSellerFinal.ImportRow(dr);
                        }
                    }
                }
            }
        }

        private void GetProductSearchResult(string Searchname, string Searchnamewithoutspace, bool isSpaceStringSame, DataTable dtResult, ref DataTable dtCategory, ref DataTable dtSubCategory, ref DataTable dtProduct, string JurisdictionID)
        {
            //FOR GETTING CATEGORY FROM SEARCH
            string[] arrCategory = { "CategoryName" };

            string strCAtegorySearch = "";
            string strCAtegorySearchWordStart = "", strCAtegorySearchWordStartInner = "";

            foreach (string cname in arrCategory)
            {
                strCAtegorySearch += cname + " like '%" + Searchname + "%' or ";
            }
            strCAtegorySearch = strCAtegorySearch.Trim().TrimEnd('o', 'r');

            string strCAtegorySearchwithoutspace = "";
            string strCAtegorySearchWordStartwithoutspace = "", strCAtegorySearchWordStartInnerwithoutspace = "";

            foreach (string cname in arrCategory)
            {
                strCAtegorySearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
            }
            strCAtegorySearchwithoutspace = strCAtegorySearchwithoutspace.Trim().TrimEnd('o', 'r');

            string strCAtegoryTableName = "Category";
            dtCategory = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrCategory, strCAtegorySearch, strCAtegorySearchwithoutspace, strCAtegoryTableName, 2, strCAtegorySearchWordStart, strCAtegorySearchWordStartInner, strCAtegorySearchWordStartwithoutspace, strCAtegorySearchWordStartInnerwithoutspace, 2, JurisdictionID).Copy();


            //FOR GETTING SUB CATEGORY FROM SEARCH

            string[] arrSubCategory = { "SubCategory" };

            string strsubCAtegorySearch = "";
            string strsCAtegorySearchWordStart = "", strsCAtegorySearchWordStartInner = "";

            foreach (string cname in arrSubCategory)
            {
                strsubCAtegorySearch += cname + " like '%" + Searchname + "%' or ";
            }
            strsubCAtegorySearch = strsubCAtegorySearch.Trim().TrimEnd('o', 'r');

            string strsubCAtegorySearchwithoutspace = "";
            string strsCAtegorySearchWordStartwithoutspace = "", strsCAtegorySearchWordStartInnerwithoutspace = "";
            foreach (string cname in arrSubCategory)
            {
                strsubCAtegorySearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
            }
            strsubCAtegorySearchwithoutspace = strsubCAtegorySearchwithoutspace.Trim().TrimEnd('o', 'r');

            string strsubCAtegoryTableName = "tblSubCategory";
            dtSubCategory = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrSubCategory, strsubCAtegorySearch, strsubCAtegorySearchwithoutspace, strsubCAtegoryTableName, 4, strsCAtegorySearchWordStart, strsCAtegorySearchWordStartInner, strsCAtegorySearchWordStartwithoutspace, strsCAtegorySearchWordStartInnerwithoutspace, 2, JurisdictionID).Copy();



            //FOR GETTING PRODUCTS FROM SEARCH

            string[] arrProduct = { "Name" };

            string strProductSearch = "";
            string strProductSearchWordStart = "", strProductSearchWordStartInner = "";

            foreach (string cname in arrProduct)
            {
                strProductSearch += cname + " like '%" + Searchname + "%' or ";
            }
            strProductSearch = strProductSearch.Trim().TrimEnd('o', 'r');

            string strProductSearchwithoutspace = "";
            string strProductSearchWordStartwithoutspace = "", strProductSearchWordStartInnerwithoutspace = "";

            foreach (string cname in arrProduct)
            {
                strProductSearchwithoutspace += cname + " like '%" + Searchnamewithoutspace + "%' or ";
            }
            strProductSearchwithoutspace = strProductSearchwithoutspace.Trim().TrimEnd('o', 'r');


            string strProductTableName = "Product";
            dtProduct = GetProductSearchTable(Searchname, dtResult.Copy(), Searchnamewithoutspace, isSpaceStringSame, arrProduct, strProductSearch, strProductSearchwithoutspace, strProductTableName, 3, strProductSearchWordStart, strProductSearchWordStartInner, strProductSearchWordStartwithoutspace, strProductSearchWordStartInnerwithoutspace, 2, JurisdictionID).Copy();


        }

        private DataTable GetProductSearchTable(string Searchname, DataTable dtResult, string Searchnamewithoutspace, bool isSpaceStringSame, string[] arrSeller, string strSellerSearch, string strSellerSearchwithoutspace, string strTableName, int Type, string strSellerSearchWordStart, string strSellerSearchWordStartInner, string strSellerSearchWordStartwithoutspace, string strSellerSearchWordStartInnerwithoutspace, int Searchfrom, string JurisdictionID)//Searchfrom = 1 for search button 2 for display list on key press
        {
            DataTable dtSeller = new DataTable();
            string tablename = strTableName;
            try
            {
                if (Searchfrom == 1)
                {
                    strTableName = strTableName + "btn";
                }

                dtSeller = GetAllData(tablename, arrSeller, JurisdictionID);

            }
            catch (Exception E)
            {
                dtSeller = GetAllData(tablename, arrSeller, JurisdictionID);
                Application[strTableName] = dtSeller;
            }

            if (dtSeller != null && dtSeller.Rows.Count > 0)
            {
                //It will search word start with string
                SearchWordStartWith(Searchname, dtResult, strSellerSearchWordStart, Type, dtSeller, 1);

                //It will search word start within string
                SearchWordStartWithInner(Searchname, dtResult, strSellerSearchWordStartInner, Type, dtSeller, 2);

                if (Searchfrom != 1) //1=search button 
                {
                    //It will search wihtout space same word
                    SearchSameWordRemoveAllSpaceAvailability(Searchname, dtResult, Type, dtSeller, 6);
                }

                //It will search reverse string of search word
                SearchReverseofSearchWord(Searchname, dtResult, Searchnamewithoutspace, isSpaceStringSame, strSellerSearchwithoutspace, Type, dtSeller, 7);

                // if (Searchfrom == 1)
                {
                    //It will search all words availability in the string
                    SearchAllWordsAvailability(Searchname, dtResult, Type, dtSeller, 8);
                }
                DataView dv = dtResult.DefaultView;
                dv.Sort = "SearchType asc";//,Total_Word desc
                dtResult = dv.ToTable();

            }
            return dtResult;
        }

        public DataTable GetAllData(string Tablename, string[] arrSeller, string JurisdictionID)
        {
            try
            {
                string[] filter_for_daiplay = { "producttag" };
                string append = "";
                string append1 = "";

                int display_count = 0;
                foreach (string cname in arrSeller)
                {
                    if (!filter_for_daiplay.Contains(cname.ToLower()))
                    {
                        display_count++;
                    }
                }

                int count = 0;
                foreach (string cname in arrSeller)
                {
                    count++;
                    if (count != arrSeller.Length)
                    {
                        append += "replace(replace((isnull(" + cname + ",'')), '''', ''),'’','')+ ', ' +";
                    }
                    else
                    {
                        append += "replace(replace((isnull(" + cname + ",'')), '''', ''),'’','')";
                    }

                    if (!filter_for_daiplay.Contains(cname.ToLower()))
                    {
                        if (count != display_count)
                        {
                            append1 += "isnull(" + cname + ",'')+ ', ' +";
                        }
                        else
                        {
                            append1 += "isnull(" + cname + ",'')";
                        }
                    }
                }

                append = ",(" + append + ") as Name";
                string query = "";
                if (Tablename == "Product")
                {
                    append1 = ",(" + append1 + ") as Name1";
                    query = "select *" + append + append1 + " from " + Tablename + " pm INNER JOIN tblCategoryProductLink cpl on pm.Id = cpl.ProductId Where pm.JurisdictionID=" + JurisdictionID;
                }
                else
                {
                    append1 = ",(" + append1 + ") as Name1";
                    query = "select *" + append + append1 + " from " + Tablename;
                }

                DataTable dt = dbc.GetDataTable(query);
                if (Tablename == "Product")
                {
                    dt.Columns["Id"].ColumnName = "ROWID";
                    dt.Columns["SubCategoryId1"].ColumnName = "Link";
                }

                if (Tablename == "Category")
                {
                    dt.Columns["CategoryID"].ColumnName = "ROWID";
                    dt.Columns["CategoryName"].ColumnName = "Link";
                }
                if (Tablename == "tblSubCategory")
                {
                    dt.Columns["ID"].ColumnName = "ROWID";
                    dt.Columns["SubCategory"].ColumnName = "Link";
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void SearchWordStartWith(string Searchname, DataTable dtResult, string strSellerSearch, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindsearch = null;

            if (Searchname.Trim() != "")
            {
                DataRow[] drfind = dtSeller.Select(strSellerSearch);
                drfindsearch = drfind;
            }
            if (drfindsearch != null)
            {
                foreach (DataRow dr in drfindsearch)
                {
                    //int SearchType = 0; //same search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void SearchWordStartWithInner(string Searchname, DataTable dtResult, string strSellerSearch, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindsearch = null;

            if (Searchname.Trim() != "")
            {
                DataRow[] drfind = dtSeller.Select(strSellerSearch);
                drfindsearch = drfind;
            }
            if (drfindsearch != null)
            {
                foreach (DataRow dr in drfindsearch)
                {
                    //int SearchType = 1; //same search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void AddResultRow(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, DataRow dr, int SearchType)
        {
            DataRow drnew = dtResult.NewRow();

            drnew["ID"] = dr[0].ToString();
            string data = dr["Name1"].ToString().Trim();
            data = data.Trim().TrimEnd(',');
            data = data.Trim().TrimEnd(',');

            drnew["Name"] = data;
            drnew["Link"] = dr["Link"].ToString();
            drnew["Type"] = Type;
            drnew["SearchType"] = SearchType;
            if (Type == 3)
                drnew["CategoryId"] = dr[55].ToString();
            else
                drnew["CategoryId"] = dr[1].ToString();
            string strAppend = "";
            foreach (DataColumn dc in dtSeller.Columns)
            {
                string cname = dc.ColumnName;
                if (cname == "Name")
                    strAppend += dr[cname].ToString();
            }
            var matches = Regex.Matches(strAppend, Searchname, RegexOptions.IgnoreCase);
            drnew["Total_Word"] = matches.Count;
            if (matches.Count > 0)
                dtResult.Rows.Add(drnew);
        }
        private static void SearchSameWordRemoveAllSpaceAvailability(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, int SearchType)
        {
            foreach (DataRow dr in dtSeller.Rows)
            {
                string searchwords = dr["Name"].ToString();
                searchwords = searchwords.Replace(" ", "");
                bool isAllow = true;
                int findindex = searchwords.ToLower().IndexOf(Searchname.ToLower());

                if (findindex == -1)
                {
                    isAllow = false;
                }

                if (isAllow)
                {
                    //int SearchType = 4;//Remove All Space from search word and check search keyword available in the search phrase
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
            //}
        }
        private static void SearchAllWordsAvailability(string Searchname, DataTable dtResult, int Type, DataTable dtSeller, int SearchType)
        {
            char[] ch = { ' ' };
            string[] spltSearch = Searchname.Split(ch, StringSplitOptions.RemoveEmptyEntries);
            if (spltSearch.Length > 1)
            {
                foreach (DataRow dr in dtSeller.Rows)
                {
                    string searchwords = dr["Name"].ToString();
                    int indexno = 0;
                    bool isAllow = true;
                    foreach (string strcheck in spltSearch)
                    {
                        int findindex = searchwords.ToLower().IndexOf(strcheck.ToLower());

                        if (findindex != -1 && findindex == 0)
                        {

                        }
                        else
                        {
                            string strcheck1 = " " + strcheck;
                            findindex = searchwords.ToLower().IndexOf(strcheck1.ToLower());

                            if (findindex == -1)
                            {
                                isAllow = false;
                                break;
                            }
                        }
                    }

                    if (isAllow)
                    {
                        //int SearchType = 3;//All Space word available in the search phrase
                        AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                    }
                }
            }
        }
        private static void SearchReverseofSearchWord(string Searchname, DataTable dtResult, string Searchnamewithoutspace, bool isSpaceStringSame, string strSellerSearchwithoutspace, int Type, DataTable dtSeller, int SearchType)
        {
            DataRow[] drfindwithoutspace = null;

            if (!isSpaceStringSame && Searchnamewithoutspace.Trim() != "")
            {
                DataRow[] drfind1 = dtSeller.Select(strSellerSearchwithoutspace);
                drfindwithoutspace = drfind1;
            }
            if (drfindwithoutspace != null)
            {
                foreach (DataRow dr in drfindwithoutspace)
                {
                    //int SearchType = 2;//reverse of search word
                    AddResultRow(Searchname, dtResult, Type, dtSeller, dr, SearchType);
                }
            }
        }
        private static void AppendCategorySearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["Link"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append("2");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
        private static void AppendSubCategorySearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["CategoryId"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append("4");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
        private static void AppendSearchResultString(DataTable dtSeller, StringBuilder sb)
        {
            string newRecordPattern = "@@@@";
            string recordSplitPattern = "#TM#TM#TM#TM";

            foreach (DataRow drnew in dtSeller.Rows)
            {

                sb.Append(drnew["Name"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["Link"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append("3");
                sb.Append(recordSplitPattern);

                sb.Append(drnew["CategoryId"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(drnew["ID"].ToString());
                sb.Append(recordSplitPattern);

                sb.Append(newRecordPattern);
            }
        }
    }
}