
using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Services;

namespace Test0555.Controllers.Order
{
    public class CancelOrderController : ApiController
    {
       [HttpGet]
        public CancelOrderModel OrderCancel1(string Id)
        {
            dbConnection dbc1 = new dbConnection();
            CancelOrderModel cancelOrder = new CancelOrderModel();
            try
            {
                if (dbc1.ExecuteQuery("Update [order] set OrderStatusId=90 where id=" + Id) > 0)
                {
                    string qry = "SELECT Customer.Mobile, [Order].Id From [Order] INNER JOIN Customer ON [Order].CustomerId = Customer.Id where [Order].Id=" + Id;

                    DataTable dt = dbc1.GetDataTable(qry);
                    string mono = "";
                    string sms = "";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string oid = dt.Rows[0]["Id"].ToString();
                        mono = dt.Rows[0]["Mobile"].ToString();
                        sms = "Your SoSho order number " + oid + " has been cancelled. We would like to see you around again. Get more discounts and offers only on https://sosho.in.";
                    }
                    dbc1.SendSMS(mono, sms);
                }
                cancelOrder.Response = "1";
                cancelOrder.Message = "Success";
            }
            catch (Exception ex)
            {
                cancelOrder.Response = "0";
                cancelOrder.Message = "Fail";
            }
            return cancelOrder;
        }

    }
}