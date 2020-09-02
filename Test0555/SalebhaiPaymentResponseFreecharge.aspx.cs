using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Test0555;
using WebApplication1.Order;

public partial class SalebhaiPaymentResponseFreecharge : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        String fc_trnid = string.Empty;
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
       string  ordrid = "2019091716333734691";
        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, "", 4, false, "1.FC:SalebhaiPaymentResponseFreecharge", " ");
        if (Request.Form.AllKeys.Length > 0)
        {
            #region vaishnaviNew
            try
            {
                Dictionary<string, string> parameterschk = new Dictionary<string, string>();
                StringBuilder sbresponse = new StringBuilder();
                foreach (string key in Request.Form.Keys)
                {
                    parameterschk.Add(key.Trim(), Request.Form[key].Trim());
                    sbresponse.Append(Request.Form[key].Trim() + " , ");
                }
                //fc_trnid = Request.Form["merchantTxnId"];

                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, fc_trnid, 4, false, "2.1.FC:SalebhaiPaymentResponseFreecharge From vaishnavi", sbresponse.ToString());



               // string Status = Request.Form["Status"];
               // string pg_txnId = Request.Form["txnId"];

                //string amount = Request.Form["amount"];
                //string checksum = Request.Form["checksum"];

                string pg_txnId = "";

                string amount = "1";
                string checksum = "";

                try
                {

                    if (!String.IsNullOrEmpty(fc_trnid))
                    {
                        int resval = pay_resp.UpdateAndSaveTransactionBeforeOrderGenerated(4, fc_trnid, sbresponse.ToString(), 2);

                        if (resval > 0)
                        {
                        }
                        else
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, fc_trnid, 4, false, "5.FC:SalebhaiPaymentResponseFreecharge: TRN NOT UPDATED " + Result_OrderId.ToString(), "Success");
                        }
                        Result_OrderId = pay_resp.PaymentResponseFreecharge(parameterschk, sbresponse.ToString(), fc_trnid, amount, pg_txnId);
                    }
                    else
                    {
                        Result_OrderId = 0;
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, fc_trnid, 4, false, "5.FC:SalebhaiPaymentResponseFreecharge: TRN ID NOT FOUND IN RESP: ", "Fail");
                    }


                }
                catch (Exception err)
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, fc_trnid, 4, false, "3.FC:ERROR  SalebhaiPaymentResponseFreecharge", "Error: " + err.Message.ToArray());

                    Result_OrderId = 0;
                }
                if (Result_OrderId > 0) // OrderId generated
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, fc_trnid, 4, false, "5.FC:SalebhaiPaymentResponseFreecharge: ORDER GENERATED Id: " + Result_OrderId.ToString(), "Success");

                    string sb_val = pay_resp.RedirectToApp(Result_OrderId, fc_trnid, 4, agentname);
                    if (!String.IsNullOrEmpty(sb_val))
                    {

                        ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_val.ToString());
                    }
                }
                else
                {
                    Result_OrderId = 0;
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, fc_trnid, 4, false, "5.FC:SalebhaiPaymentResponseFreecharge: ORDER NOT GENERATED Id: ", "Fail");
                }
            }
            catch (Exception err)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, fc_trnid, 4, false, "3.FC:ERROR  SalebhaiPaymentResponseFreecharge", "Error: " + err.Message.ToArray());

                Result_OrderId = 0;
            }

            #endregion

        }
        else
        {
            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, fc_trnid, 4, false, "4.FC:SalebhaiPaymentResponseFreecharge", "NO RESPONSE KEYS FOUND ");
            Result_OrderId = 0;

        }
        // pay_resp.RedirectToApp(Result_OrderId, fc_trnid, 4,agentname);
        string sb_valn = pay_resp.RedirectToApp(Result_OrderId, fc_trnid, 4, agentname);
        if (!String.IsNullOrEmpty(sb_valn))
        {

            ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_valn.ToString());
        }
    }
}