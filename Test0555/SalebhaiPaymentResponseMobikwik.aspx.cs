using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Test0555;
using WebApplication1.Order;

public partial class SalebhaiPaymentResponseMobikwik : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        String mobikwik_trnid = string.Empty;
            int Result_OrderId = 0;

            AllPaymentResponse pay_resp = new AllPaymentResponse();
            var agentname = "";
            try
            {
                agentname = Request.UserAgent.ToLower();
            }
            catch (Exception E)
            {
                agentname = "";
            }
            #region VaishnaviNew

            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, "", 3, false, "1.MOBIKWIK:SalebhaiPaymentResponseMobikwik", " ");


            if (Request.Form.AllKeys.Length > 0)
            {
                
                try
                {

                    Dictionary<string, string> parameterschk = new Dictionary<string, string>();
                    StringBuilder sbresponse = new StringBuilder();
                    foreach (string key in Request.Form.Keys)
                    {
                        parameterschk.Add(key.Trim(), Request.Form[key].Trim());
                        sbresponse.Append(Request.Form[key].Trim() + " , ");
                    }
                    
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, mobikwik_trnid, 3, false, "2.1.MOBIKWIK:SalebhaiPaymentResponseMobikwik From vaishnavi", sbresponse.ToString());


                    String statuscode = string.Empty;
                    String amount_rcvd = string.Empty;
                    String mid = string.Empty;
                    String statusmessage = string.Empty;
                    String pg_txnId = string.Empty;
                    String checksum = string.Empty;
                   // String Mer_Id = "MBK7769";

                    try
                    {
                        statuscode = Request.Form["statuscode"];
                        amount_rcvd = Request.Form["amount"];
                        mid = Request.Form["mid"];
                        statusmessage = Request.Form["statusmessage"];
                        pg_txnId = Request.Form["refid"];
                        checksum = Request.Form["checksum"];
                        mobikwik_trnid = Request.Form["orderid"];
                        //statuscode = "";
                        //amount_rcvd = "1";
                        //mid = Mer_Id;
                        //statusmessage = "";
                        //pg_txnId = "";
                    }
                    catch (Exception err)
                    {
                    }

                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, mobikwik_trnid, 3, false, "2.1.MOBIKWIK:SalebhaiPaymentResponseMobikwik generate order method call:", sbresponse.ToString() + "Response Fileds: " + sbresponse.ToString() + "****statuscode**" + statuscode + "****amount_rcvd**" + amount_rcvd + "****mid**" + mid + "****statusmessage**" + statusmessage + "****pg_txnId**" + pg_txnId + "****checksum**" + checksum);

                    if (!String.IsNullOrEmpty(mobikwik_trnid))
                    {
                        Result_OrderId = pay_resp.PaymentResponseMobikwik(parameterschk, sbresponse.ToString(), mobikwik_trnid, statuscode, amount_rcvd, mid, statusmessage, checksum, pg_txnId);
                    }
                    else
                    {
                        Result_OrderId = 0;
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, mobikwik_trnid, 3, false, "5.MOBI:SalebhaiPaymentResponseMobikwik: TRN ID NOT FOUND IN RESP: ", "Fail");
                    }




                }
                catch (Exception err)
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, mobikwik_trnid, 3, false, "3.MOBIKWIK:ERROR  SalebhaiPaymentResponseMobikwik", "Error: " + err.Message.ToArray());

                    // Result_OrderId = 0;
                }
                if (Result_OrderId > 0) // OrderId generated
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, mobikwik_trnid, 3, false, "5.MOBIKWIK:SalebhaiPaymentResponseMobikwik: ORDER GENERATED Id: " + Result_OrderId.ToString(), "Success");

                    Server.Transfer("http://localhost:4415/final.aspx?PlaceOrderId='" + Result_OrderId + "'", false);

                  

                    //string sb_val = pay_resp.RedirectToApp(Result_OrderId, mobikwik_trnid, 3, agentname);
                    //if (!String.IsNullOrEmpty(sb_val))
                    //{

                    //    ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_val.ToString());
                    //}
                }
                else
                {
                    Result_OrderId = 0;
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, mobikwik_trnid, 3, false, "5.MOBIKWIK:SalebhaiPaymentResponseMobikwik: ORDER NOT GENERATED Id: ", "Fail");
                }

            }
            else
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, mobikwik_trnid, 3, false, "4.MOBIKWIK:SalebhaiPaymentResponseMobikwik", "NO RESPONSE KEYS FOUND ");
                Result_OrderId = 0;

            }


            #endregion

            string sb_valn = pay_resp.RedirectToApp(Result_OrderId, mobikwik_trnid, 3, agentname);
            if (!String.IsNullOrEmpty(sb_valn))
            {

                ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_valn.ToString());
            }

    }

}