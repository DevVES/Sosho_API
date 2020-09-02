using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Test0555;
using WebApplication1.Order;

public partial class SalebhaiPaymentResponseBillDesk : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        #region Billdesk
        int Result_OrderId = 0;
        String BD_trnid = string.Empty;
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

        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, "", 6, false, "1.BD:SalebhaiPaymentResponseBillDesk", " ");


        if (Request.Form.AllKeys.Length > 0)
        {
            try
            {
                String MsgStatusStr = Request.Form["msg"];

                string[] spltrstr = MsgStatusStr.Split('|');
                BD_trnid = spltrstr[1];

                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, BD_trnid, 6, false, "2.1.BD:SalebhaiPaymentResponseBillDesk From Vaishnavi", MsgStatusStr.ToString());

                try
                {
                    if (!String.IsNullOrEmpty(BD_trnid))
                    {
                        int resval = pay_resp.UpdateAndSaveTransactionBeforeOrderGenerated(6, BD_trnid, MsgStatusStr.ToString(), 2);

                        if (resval > 0)
                        {
                        }
                        else
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, BD_trnid, 6, false, "5.BD:SalebhaiPaymentResponsePaytm: TRN NOT UPDATED " + Result_OrderId.ToString(), "Success");
                        }
                        Result_OrderId = pay_resp.PaymentResponseBillDesk(spltrstr, MsgStatusStr.ToString(), BD_trnid);
                    }
                    else
                    {
                        Result_OrderId = 0;
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, BD_trnid, 6, false, "5.BD:SalebhaiPaymentResponseBillDesk: TRN ID NOT FOUND IN RESP: ", "Fail");
                    }

                }
                catch (Exception err)
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, BD_trnid, 6, false, "3.BD:ERROR  SalebhaiPaymentResponseBillDesk", "Error: " + err.Message.ToArray());

                    Result_OrderId = 0;
                }
                if (Result_OrderId > 0) // OrderId generated
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, BD_trnid, 6, false, "5.BD:SalebhaiPaymentResponseBillDesk: ORDER GENERATED Id: " + Result_OrderId.ToString(), "Success");

                    string sb_val = pay_resp.RedirectToApp(Result_OrderId, BD_trnid, 6, agentname);
                    if (!String.IsNullOrEmpty(sb_val))
                    {

                        ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_val.ToString());
                    }
                }
                else
                {
                    Result_OrderId = 0;
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, BD_trnid, 6, false, "5.BD:SalebhaiPaymentResponseBillDesk: ORDER NOT GENERATED Id: ", "Fail");
                }

            }
            catch (Exception err)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, BD_trnid, 6, false, "3.BD:ERROR  SalebhaiPaymentResponseBillDesk", "Error: " + err.Message.ToArray());

                Result_OrderId = 0;
            }
        }


        else
        {
            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, BD_trnid, 6, false, "4.PAYTM:SalebhaiPaymentResponseBillDesk", "NO RESPONSE KEYS FOUND ");
            Result_OrderId = 0;

        }



        string sb_valn = pay_resp.RedirectToApp(Result_OrderId, BD_trnid, 6, agentname);
        if (!String.IsNullOrEmpty(sb_valn))
        {

            ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_valn.ToString());
        }
        #endregion
        #endregion
    }
}