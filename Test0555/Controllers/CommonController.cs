using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Test0555.Controllers
{
    public class CommonController : ApiController
    {
        #region Props
        dbConnection dbCon = new dbConnection();
        #endregion
        [HttpGet]
        public ClsCommonResult getRandomMessageTextForPopup(string randomNumber)
        {
            ClsCommonResult objCommonResult = new ClsCommonResult();
            try
            {
                System.Data.DataTable dtRandomMessage= dbCon.GetDataTableWithParams("Select [message] from RandomActionMessages where id=@1", new string[] { randomNumber });
                if (dtRandomMessage != null && dtRandomMessage.Rows.Count > 0)
                {
                    objCommonResult.response = CommonString.successmessage;
                    objCommonResult.message = (dtRandomMessage.Rows[0]["message"] != null ? dtRandomMessage.Rows[0]["message"].ToString() : "Prakash Just Bought!");
                    if (objCommonResult.message.ToLower().Contains("bought"))
                    {
                        string final1 = string.Empty;
                        string qry = "select  Product.Id ,Product.sold from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "'";
                        System.Data.DataTable dt = dbCon.GetDataTable(qry);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            string[] sen = dt.Rows[0]["sold"].ToString().Split(' ');
                            int last = int.Parse(sen[1].ToString());
                            int final = last + 1;
                            final1 = sen[0] + " " + final.ToString() + " " + sen[2];
                            objCommonResult.response = final1;
                        }
                        //string updt = "UPDATE [dbo].[Product] SET [sold] ='" + final1 + "' where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "'";
                        //int ordersoldupdt = dbCon.ExecuteQuery(updt);
                    }
                }
            }
            catch (Exception ex)
            {
                objCommonResult.response = CommonString.Errorresponse;
                objCommonResult.message = "Error: " + ex.Message;
            }
            return objCommonResult;
        }

        public class ClsCommonResult
        {
            public ClsCommonResult()
            {
                response = CommonString.Errorresponse;
                message = string.Empty;
            }
            public string response { get; set; }
            public string message { get; set; }
        }
    }
}
