using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Test0555;
using WebApplication1.Order;

public partial class SalebhaiPaymentResponsePaytm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        #region PAYTM RESPONSE
        String paytm_trnid = string.Empty;
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

        #region vaishnavi

        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, "", 2, false, "1.PAYTM:SalebhaiPaymentResponsePaytm", " ");


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
                paytm_trnid = Request.Form["ORDERID"].ToString();

                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, paytm_trnid, 2, false, "2.1.PAYTM:SalebhaiPaymentResponsePaytm GOT TRN ID", sbresponse.ToString());
                if (!String.IsNullOrEmpty(paytm_trnid))
                {

                    int resval = pay_resp.UpdateAndSaveTransactionBeforeOrderGenerated(2, paytm_trnid, sbresponse.ToString(), 2);

                    if (resval > 0)
                    {
                    }
                    else
                    {
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, paytm_trnid, 2, false, "5.PAYTM:SalebhaiPaymentResponsePaytm: TRN NOT UPDATED " + Result_OrderId.ToString(), "Success");
                    }

                    try
                    {
                        Result_OrderId = pay_resp.PaymentResponsePaytm(parameterschk, sbresponse.ToString(), paytm_trnid);
                    }
                    catch (Exception err)
                    {
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, paytm_trnid, 2, false, "3.PAYTM:ERROR  SalebhaiPaymentResponsePaytm IN ORDER GENERATION", "Error: " + err.Message.ToArray());
                        Result_OrderId = 0;
                    }
                    if (Result_OrderId > 0) // OrderId generated
                    {
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, paytm_trnid, 2, false, "5.PAYTM:SalebhaiPaymentResponsePaytm: ORDER GENERATED Id: " + Result_OrderId.ToString(), "Success");
                        //pay_resp.RedirectToApp(Result_OrderId, paytm_trnid, 2,agentname);
                        string sb_val = pay_resp.RedirectToApp(Result_OrderId, paytm_trnid, 2, agentname);
                        if (!String.IsNullOrEmpty(sb_val))
                        {

                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, paytm_trnid, 2, false, "555.SalebhaiPaymentResponsePaytm: ORDER GENERATED APP SCRIPT: ", "Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb_val.ToString());


                            ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_val.ToString());
                        }

                    }
                    else
                    {
                        Result_OrderId = 0;
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, paytm_trnid, 2, false, "5.PAYTM:SalebhaiPaymentResponsePaytm: ORDER NOT GENERATED Id: ", "Fail");
                    }
                }
                else
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, paytm_trnid, 2, false, "4.PAYTM:SalebhaiPaymentResponsePaytm", "NO TRANSACTION ID RESPONSE FOUND ");
                    Result_OrderId = 0;
                }
            }
            catch (Exception err)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, paytm_trnid, 2, false, "3.PAYTM:ERROR  SalebhaiPaymentResponsePaytm", "Error: " + err.Message.ToArray());

                Result_OrderId = 0;
            }
        }
        else
        {
            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, paytm_trnid, 2, false, "4.PAYTM:SalebhaiPaymentResponsePaytm", "NO RESPONSE KEYS FOUND ");
            Result_OrderId = 0;

        }


        #endregion
        string sb_valn = pay_resp.RedirectToApp(Result_OrderId, paytm_trnid, 2, agentname);
        if (!String.IsNullOrEmpty(sb_valn))
        {

            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, paytm_trnid, 2, false, "555.SalebhaiPaymentResponsePaytm: ORDER GENERATED APP SCRIPT: ", "Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb_valn.ToString());


            ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_valn.ToString());
        }





        //ClientScript.RegisterClientScriptBlock(this.GetType(), "Message", sb_val.ToString());

        #endregion




    }
}