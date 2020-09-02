using System.Collections.Generic;
using System.Data;
using paytm;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Web.UI;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using InquiryManageAPI.Controllers;
using Test0555;
using System;

namespace WebApplication1.Order
{
    public class AllPaymentResponse : System.Web.UI.Page
    {
        dbConnection dbCon = new dbConnection();
        public int PaymentResponsePaytm(Dictionary<string, string> parameterschk, string WholeResponse, string txnId)
        {

            #region Wallet Response
            string RespCode = string.Empty;
            string pg_txnId = string.Empty;
            string statusmsg = string.Empty;
            string status_API_Response = string.Empty;
            string time = string.Empty;
            string amount = string.Empty;


            #region PayTm Integration
            string paytmChecksum = "";
            if (parameterschk.ContainsKey("CHECKSUMHASH"))
            {
                paytmChecksum = parameterschk["CHECKSUMHASH"];
                parameterschk.Remove("CHECKSUMHASH");

                if (CheckSum.verifyCheckSum(Constant.Message.PAYTM_merchantKey, parameterschk, paytmChecksum))
                {
                    parameterschk.Add("IS_CHECKSUM_VALID", "Y");
                    txnId = parameterschk["ORDERID"];

                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, txnId, 2, false, "1.Paytm Trn", WholeResponse);

                    #region PAYTM

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    string MID = Constant.Message.PAYTM_MID;

                    parameters.Add("MID", MID);
                    parameters.Add("ORDERID", txnId);

                    try
                    {
                        string paytmChecksum_trnstatus = dbCon.genCheckSum(parameters, Constant.Message.PAYTM_merchantKey);
                        paytmChecksum_trnstatus = System.Web.HttpUtility.UrlEncode(paytmChecksum_trnstatus);
                        //string jsondata_val = "'MID':'" + MID + "','ORDERID':'" + txnId + "','CHECKSUMHASH':'" + paytmChecksum_trnstatus + "'";
                        string jsondata_val = "\"MID\":\"" + MID + "\",\"ORDERID\":\"" + txnId + "\",\"CHECKSUMHASH\":\"" + paytmChecksum_trnstatus + "\"";
                        string chkstatus_URL = "https://secure.paytm.in/oltp/HANDLER_INTERNAL/getTxnStatus?JsonData={" + jsondata_val + "}";
                        HttpWebRequest webReq = WebRequest.Create(chkstatus_URL) as HttpWebRequest;
                        webReq.Method = "GET";
                        webReq.Accept = "application/json";
                        HttpWebResponse myResp = (HttpWebResponse)webReq.GetResponse();
                        var httpResponsedetails = (HttpWebResponse)webReq.GetResponse();
                        System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                        string responseString = respStreamReader.ReadToEnd();

                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, txnId, 2, false, "1.Paytm Trn: STATUS API RESPONSE: ", responseString);

                        status_API_Response = responseString;
                        try
                        {
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            paytmResponse objreponse = jsonSerializer.Deserialize<paytmResponse>(responseString);
                            RespCode = objreponse.RESPCODE;
                            statusmsg = objreponse.RESPMSG;
                            time = objreponse.TXNDATE;
                            amount = objreponse.TXNAMOUNT;
                            pg_txnId = objreponse.TXNID;
                        }
                        catch (Exception err)
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "Paytm:Error in Trn update" + txnId, err.Message.ToString());
                        }

                        WholeResponse = statusmsg + " ::: " + WholeResponse;
                        if (parameterschk["RESPCODE"] == "01" && Convert.ToInt32(RespCode) == 01)
                        {

                            #region SUCCESS TRANSACTION

                            try
                            {
                                String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_PaymentRecivedByCitrus] = @1 ,[Cit_pgTxnId] = @2 ,[IsPaymentSuccess] = @3,[PaymentConfirmationReceived]=@4  WHERE [TxnId] =  '" + txnId + "'";
                                //String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_pgTxnId] = @1 ,[IsPaymentSuccess] = @2,[PaymentConfirmationReceived]=@3  WHERE [TxnId] =  '" + txnId + "'";
                                //string[] parm_new = { amount, pg_txnId, "1", "1" };
                                string[] parm_new = { amount, txnId, "1", "1" };
                                int SuccesstrnUpdate = dbCon.ExecuteQueryWithParams(str_new, parm_new);

                                
                                if (SuccesstrnUpdate > 0)
                                {

                                }
                                else
                                {
                                    // ISSUE IN TRANSACTION UPDATE
                                    return 0;
                                }
                                #region Genererate the Order

                                try
                                {
                                    // Generate the order

                                    string getAmount = "select Top 1 OrderAmount from CitrusPayment where TxnId='" + txnId + "'";
                                    DataTable dtAMount = dbCon.GetDataTable(getAmount);
                                    amount = dtAMount.Rows[0][0].ToString();
                                    int OrderId = 0;
                                    GenerateOrder create_order = new GenerateOrder();

                                    OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amount);


                                    if (OrderId > 0)
                                    {
                                         //if success, Update the transaction

                                        #region Update Transaction

                                        try
                                        {
                                            String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction] = DATEADD(MINUTE, 330, GETUTCDATE()),[Statuse] = @2 WHERE [TxnId] =  '" + txnId + "'";

                                            string[] parms_new = { OrderId.ToString(), "3" };

                                            int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);


                                            if (resvaltrn > 0)
                                            {
                                                String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + txnId + "'";
                                                string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                                if (resvaltrn1 > 0)
                                                {
                                                    String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + txnId + "'";
                                                    string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                    int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                    if (resvaltrn2 > 0)
                                                    {
                                                    }
                                                }
                                                return OrderId;
                                            }
                                            else
                                            {
                                                // ISSUE IN TRANSACTION UPDATE

                                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Paytm : ISSUE IN TRANSACTION UPDATE : ", "Result: " + resvaltrn.ToString());
                                                return 0;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            return 0;
                                            // ERROR IN TRANSACTION UPDATE
                                        }

                                        #endregion
                                      //   send sms to the customer  
                                    }
                                    
                                    else
                                    {
                                        return 0;
                                    }
                                }
                                catch (Exception err)
                                {
                                    // ERROR IN GENERATE ORDER
                                    return 0;
                                }
                                #endregion
                            }
                            catch (Exception err)
                            {
                                // ERROR IN TRANSACTION UPDATE
                                return 0;
                            }
                            #endregion
                        }
                        else
                        {
                            // When I cancelled TRN, 227 code came 
                            return 0;
                        }
                    }
                    catch (Exception err)
                    {
                        // ERROR WHILE CALLING STATUS API
                        return 0;
                    }

                    #endregion
                }
                else
                {
                    // CHECKSUM NOT VARIFIED
                    parameterschk.Add("IS_CHECKSUM_VALID", "N");
                    return 0;
                }
            }
            else
            {
                return 0;
                // RESPONSE DOES NOT CONTAINS CHECKSUM 
            }
            #endregion

            #endregion
            //return 0;
        }

        public int PaymentResponseMobikwik(Dictionary<string, string> parameterschk, string WholeResponse, string txnId, string statuscode, string amount_rcvd, string mid, string statusmessage, string checksum, string pg_txnId = "nopgid")
        {
            int OrderId = 0;

            try
            {
                String checksumString = null;

                checksumString = "'" + statuscode + "''" + txnId + "''" + amount_rcvd + "''" + statusmessage + "''" + mid + "'";
                checksumString = checksumString.Trim();
                String secretKey = "oCGlWYFyeSzuEabKfamimyGVHP9E";
                string rct_api = "https://walletapi.mobikwik.com/checkstatus";
                String merchant_id = "MBK7769";
                Boolean checksumMatch = verifyChecksum(secretKey, checksumString, checksum);

                if (checksumMatch)
                {
                    try
                    {
                        if (statuscode == "0")
                        {
                            try
                            {
                                Dictionary<String, String> returnDict = verifyTransaction(merchant_id, txnId, amount_rcvd.ToString(), secretKey, rct_api);
                                if (returnDict["flag"] == "true")
                                {

                                    #region SUCCESS TRANSACTION

                                    try
                                    {
                                        string txtid = "nopgid";
                                        String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_PaymentRecivedByCitrus] = @1 ,[Cit_pgTxnId] = @2 ,[IsPaymentSuccess] = @3,[PaymentConfirmationReceived]=@4  WHERE [TxnId] =  '" + txnId + "'";
                                        if (!String.IsNullOrEmpty(pg_txnId))
                                        {
                                            txtid = pg_txnId;
                                        }

                                        string[] parm_new = { amount_rcvd, txtid, "1", "1" };
                                        int resvalnew = dbCon.ExecuteQueryWithParams(str_new, parm_new);
                                        if (resvalnew > 0)
                                        {
                                            #region Genererate the Order

                                            try
                                            {
                                                // Generate the order


                                                GenerateOrder create_order = new GenerateOrder();
                                                OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amount_rcvd);


                                                if (OrderId > 0)
                                                {
                                                    // if success, Update the transaction

                                                    #region Update Transaction

                                                    try
                                                    {
                                                        String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction] = DATEADD(MINUTE, 330, GETUTCDATE()),[Statuse] = @2 WHERE [TxnId] =  '" + txnId + "'";

                                                        string[] parms_new = { OrderId.ToString(), "3" };

                                                        int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);


                                                        if (resvaltrn > 0)
                                                        {
                                                            String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + txnId + "'";
                                                            string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                            int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                                            if (resvaltrn1 > 0)
                                                            {
                                                                String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + txnId + "'";
                                                                string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                                int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                                if (resvaltrn2 > 0)
                                                                {
                                                                }
                                                            }
                                                            return OrderId;
                                                        }
                                                        else
                                                        {
                                                            // ISSUE IN TRANSACTION UPDATE

                                                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Paytm : ISSUE IN TRANSACTION UPDATE : ", "Result: " + resvaltrn.ToString());
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {

                                                        // ERROR IN TRANSACTION UPDATE
                                                    }

                                                    #endregion

                                                    // send app notification to the customer

                                                    // send email to the customer

                                                    //Redirect the User

                                                    return OrderId;
                                                }
                                                else
                                                {
                                                    // ORDER ID 0
                                                    return 0;
                                                }
                                            }
                                            catch (Exception err)
                                            {
                                                // ERROR IN GENERATE ORDER
                                                return 0;
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            // ISSUE IN TRANSACTION UPDATE
                                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Mobikwik : ISSUE IN TRANSACTION UPDATE : ", "Query:" + str_new);
                                            return 0;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        // ERROR IN TRANSACTION UPDATE
                                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Mobikwik : ERROR IN TRANSACTION UPDATE: ", "MSG" + err.Message.ToString());
                                        return 0;
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception err)
                            {
                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 3, false, "3654431.MOBIKWIK:ERROR  PaymentResponseMobikwik", "Error: " + err.Message.ToArray());

                            }
                        }
                        else
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1564.Mobikwik : ISSUE IN TRANSACTION UPDATE : ", "STATUS CODE IS NOT SUCCESS =" + statuscode);
                        }
                    }
                    catch (Exception err)
                    {
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 3, false, "3.114MOBIKWIK:ERROR  PaymentResponseMobikwik", "Error: " + err.Message.ToArray());

                    }
                }
                else
                {
                    // Checksum not match
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Mobikwik : ISSUE IN TRANSACTION UPDATE : ", "CHECK SUM NOT MATCHED");
                }

            }
            catch (Exception err)
            {

                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 3, false, "3.MOBIKWIK:ERROR  PaymentResponseMobikwik", "Error: " + err.Message.ToArray());
            }



            return OrderId;

        }

               
        public int PaymentResponseFreecharge(Dictionary<string, string> parameterschk, string WholeResponse, string txnId, string amountrcvd, string pg_txnId)
        {
            int OrderId = 0;
            #region SUCCESS TRANSACTION

            try
            {
                String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_PaymentRecivedByCitrus] = @1 ,[Cit_pgTxnId] = @2 ,[IsPaymentSuccess] = @3,[PaymentConfirmationReceived]=@4  WHERE [TxnId] =  '" + txnId + "'";

                string[] parm_new = { amountrcvd.ToString(), pg_txnId, "1", "1" };

                int resvalnew = dbCon.ExecuteQueryWithParams(str_new, parm_new);


                if (resvalnew > 0)
                {
                    #region Genererate the Order

                    try
                    {
                        // Generate the order


                        GenerateOrder create_order = new GenerateOrder();
                        OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amountrcvd.ToString());


                        if (OrderId > 0)
                        {
                            // if success, Update the transaction

                            #region Update Transaction

                            try
                            {
                                String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction] = DATEADD(MINUTE, 330, GETUTCDATE()),[Statuse] = @2 WHERE [TxnId] =  '" + txnId + "'";

                                string[] parms_new = { OrderId.ToString(), "3" };

                                int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);


                                if (resvaltrn > 0)
                                {
                                    String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + txnId + "'";
                                    string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                    int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                    if (resvaltrn1 > 0)
                                    {
                                        String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + txnId + "'";
                                        string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                        int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                        if (resvaltrn2 > 0)
                                        {
                                        }
                                    }
                                    return OrderId;
                                }
                                else
                                {
                                    // ISSUE IN TRANSACTION UPDATE

                                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Paytm : ISSUE IN TRANSACTION UPDATE : ", "Result: " + resvaltrn.ToString());
                                }
                            }
                            catch (Exception err)
                            {

                                // ERROR IN TRANSACTION UPDATE
                            }

                            #endregion

                            // send app notification to the customer

                            // send email to the customer

                            #region SEND ORDER PLACED EMAIL TO CUSTOMER

                            #endregion

                            //Redirect the User

                            return OrderId;
                        }
                        else
                        {
                            // ORDER ID 0
                            return 0;
                        }
                    }
                    catch (Exception err)
                    {
                        // ERROR IN GENERATE ORDER
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    // ISSUE IN TRANSACTION UPDATE
                    return 0;
                }
            }
            catch (Exception err)
            {
                // ERROR IN TRANSACTION UPDATE
                return 0;
            }
            #endregion

            return OrderId;
        }

        public int PaymentResponseAmazonPay(Dictionary<string, string> parameterschk, string WholeResponse, string txnId, string amountrcvd, string pg_txnId)
        {
            int OrderId = 0;
            #region SUCCESS TRANSACTION

            try
            {
                String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_PaymentRecivedByCitrus] = @1 ,[Cit_pgTxnId] = @2 ,[IsPaymentSuccess] = @3,[PaymentConfirmationReceived]=@4  WHERE [TxnId] =  '" + txnId + "'";

                string[] parm_new = { amountrcvd.ToString(), pg_txnId, "1", "1" };

                int resvalnew = dbCon.ExecuteQueryWithParams(str_new, parm_new);


                if (resvalnew > 0)
                {
                    #region Genererate the Order

                    try
                    {
                        // Generate the order


                        GenerateOrder create_order = new GenerateOrder();
                        OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amountrcvd.ToString());


                        if (OrderId > 0)
                        {
                            // if success, Update the transaction

                            #region Update Transaction

                            try
                            {
                                String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction] = DATEADD(MINUTE, 330, GETUTCDATE()),[Statuse] = @2 WHERE [TxnId] =  '" + txnId + "'";

                                string[] parms_new = { OrderId.ToString(), "3" };

                                int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);


                                if (resvaltrn > 0)
                                {
                                    String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + txnId + "'";
                                    string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                    int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                    if (resvaltrn1 > 0)
                                    {
                                        String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + txnId + "'";
                                        string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                        int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                        if (resvaltrn2 > 0)
                                        {
                                        }
                                    }
                                    return OrderId;
                                }
                                else
                                {
                                    // ISSUE IN TRANSACTION UPDATE

                                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Paytm : ISSUE IN TRANSACTION UPDATE : ", "Result: " + resvaltrn.ToString());
                                }
                            }
                            catch (Exception err)
                            {

                                // ERROR IN TRANSACTION UPDATE
                            }

                            #endregion

                            // send app notification to the customer

                            // send email to the customer

                            #region SEND ORDER PLACED EMAIL TO CUSTOMER

                            #endregion

                            //Redirect the User

                            return OrderId;
                        }
                        else
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, txnId, 9, false, "5.PaymentResponseAmazonPay:  SOrderIdNotGenerated  ");

                            // ORDER ID 0
                            return 0;
                        }
                    }
                    catch (Exception err)
                    {

                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 9, false, "5486854.AMAZONPAY:ERROR  SalebhaiPaymentResponseAMAZONPAY IN ORDER GENERATION", "Error: " + err.Message.ToString());


                        // ERROR IN GENERATE ORDER
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    // ISSUE IN TRANSACTION UPDATE
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, txnId, 9, false, "5.PaymentResponseAmazonPay: ISSUE IN TRANSACTION UPDATE SOrderIdNotGenerated  ");
                    return 0;
                }
            }
            catch (Exception err)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 9, false, "24345.AMAZONPAY:ERROR  SalebhaiPaymentResponseAMAZONPAY IN ORDER GENERATION", "Error: " + err.Message.ToString());
                // ERROR IN TRANSACTION UPDATE
                return 0;
            }
            #endregion

            return OrderId;
        }
        public int PaymentResponseBillDesk(string[] spltrstr, string WholeResponse, string txnId)
        {
            int OrderId = 0;

            string checksum_recieved = spltrstr[spltrstr.Length - 1];
            string pg_txnId = "";


            string hash = string.Empty;

            #region Generate Checksum

            try
            {
                string[] spltrstr_to_generatechksm = spltrstr.Take(spltrstr.Count() - 1).ToArray();

                var result = string.Join("|", spltrstr_to_generatechksm);

                string commonkey = "htNDOPkBZr58";

                hash = GetHMACSHA256(result, commonkey);
                hash = hash.ToUpper();

            }
            catch (Exception err)
            {

                // throw;
            }

            #endregion

            //Need to change Prakashsir
            string responsecode = spltrstr[14];
            string amount_rcvd = spltrstr[4];
            decimal amountrcvd = 0;
            try
            {
                amountrcvd = Convert.ToDecimal(amount_rcvd);
                pg_txnId = spltrstr[2].ToString();
            }
            catch (Exception err)
            {

            }

            string Error_Code = spltrstr[24];
            string Error_Description = spltrstr[25];
            bool IsEqual = hash.Equals(checksum_recieved);

            if (IsEqual)
            {
                if (responsecode.Equals("0300"))
                {
                    #region SUCCESS TRANSACTION
                    try
                    {
                        String str_new = "UPDATE [dbo].[CitrusPayment] SET [Cit_PaymentRecivedByCitrus] = @1 ,[Cit_pgTxnId] = @2 ,[IsPaymentSuccess] = @3,[PaymentConfirmationReceived]=@4  WHERE [TxnId] =  '" + txnId + "'";

                        string[] parm_new = { amountrcvd.ToString(), pg_txnId, "1", "1" };
                        int resvalnew = dbCon.ExecuteQueryWithParams(str_new, parm_new);

                        if (resvalnew > 0)
                        {
                            #region Genererate the Order

                            try
                            {
                                // Generate the order
                                GenerateOrder create_order = new GenerateOrder();
                                OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amountrcvd.ToString());

                                if (OrderId > 0)
                                {
                                    // if success, Update the transaction

                                    #region Update Transaction

                                    try
                                    {
                                        String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction] = DATEADD(MINUTE, 330, GETUTCDATE()),[Statuse] = @2 WHERE [TxnId] =  '" + txnId + "'";

                                        string[] parms_new = { OrderId.ToString(), "3" };

                                        int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);


                                        if (resvaltrn > 0)
                                        {
                                            String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + txnId + "'";
                                            string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                            int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                            if (resvaltrn1 > 0)
                                            {
                                                String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + txnId + "'";
                                                string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                if (resvaltrn2 > 0)
                                                {
                                                }
                                            }
                                            return OrderId;
                                        }
                                        else
                                        {
                                            // ISSUE IN TRANSACTION UPDATE

                                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 2, false, "1.Paytm : ISSUE IN TRANSACTION UPDATE : ", "Result: " + resvaltrn.ToString());
                                        }
                                    }
                                    catch (Exception err)
                                    {

                                        // ERROR IN TRANSACTION UPDATE
                                    }

                                    #endregion

                                    // send app notification to the customer

                                    // send email to the customer

                                    #region SEND ORDER PLACED EMAIL TO CUSTOMER

                                    #endregion

                                    //Redirect the User

                                    return OrderId;
                                }
                                else
                                {
                                    // ORDER ID 0
                                    return 0;
                                }
                            }
                            catch (Exception err)
                            {
                                // ERROR IN GENERATE ORDER
                                return 0;
                            }
                            #endregion
                        }
                        else
                        {
                            // ISSUE IN TRANSACTION UPDATE
                            return 0;
                        }
                    }
                    catch (Exception err)
                    {
                        // ERROR IN TRANSACTION UPDATE
                        return 0;
                    }
                    #endregion
                }
                else
                {
                    // OTHER RESPONSE NOT SUCCESS
                }
            }
            else
            {
                // CHECKSUM NOT EQUAL
            }

            return OrderId;
        }
        public static string calculateChecksum(string secretkey, string allparamvalues)
        {

            byte[] dataToEncryptByte = Encoding.UTF8.GetBytes(allparamvalues);
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretkey);
            HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);
            byte[] checksumByte = hmacsha256.ComputeHash(dataToEncryptByte);
            String checksum = toHex(checksumByte);

            return checksum;
        }


        public Dictionary<String, String> verifyTransaction(String MerchantId, String OrderId, String Amount, String WorkingKey, String url)
        {



            Dictionary<String, String> returnDict = new Dictionary<String, String>();

            String checksumString = "'" + MerchantId + "''" + OrderId + "'";



            String checksum = calculateChecksum(WorkingKey, checksumString);




            // creates the post data for the POST request
            string postData = ("mid=" + MerchantId + "&orderid=" + OrderId + "&checksum=" + checksum + "&ver=2");

            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, OrderId, 2, false, "66.Mobikwik : verifyTransaction : ", "postData:" + postData);

            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // create the POST request
            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postData.Length;

            // POST the data


            try
            {
                try
                {
                    using (System.IO.StreamWriter requestWriter2 = new System.IO.StreamWriter(webRequest.GetRequestStream()))
                    {
                        requestWriter2.Write(postData);
                    }


                }
                catch (Exception err)
                {

                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, OrderId, 3, false, "777777.MOBIKWIK:ERROR  PaymentResponseMobikwik", "Error: " + err.Message.ToArray() + "::ST::" + err.StackTrace.ToString());
                }

                //  This actually does the request and gets the response back
                System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)webRequest.GetResponse();

                string responseData = string.Empty;

                using (System.IO.StreamReader responseReader = new System.IO.StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    // dumps the HTML from the response into a string variable
                    responseData = responseReader.ReadToEnd();

                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, OrderId, 2, false, "14168484.Mobikwik : ISSUE IN verifyTransaction : ", "responseData:" + responseData);
                }



                System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                xmlDocument.LoadXml(responseData);

                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, OrderId, 2, false, "14994.Mobikwik : ISSUE IN verifyTransaction : ", "responseData:" + responseData);

                String receivedstatuscode = xmlDocument.SelectSingleNode("wallet/statuscode").InnerText.Trim();
                String receivedorderid = xmlDocument.SelectSingleNode("wallet/orderid").InnerText.Trim();
                String receivedrefid = xmlDocument.SelectSingleNode("wallet/refid").InnerText.Trim();
                String receivedamount = xmlDocument.SelectSingleNode("wallet/amount").InnerText.Trim();
                String receivedstatusmessage = xmlDocument.SelectSingleNode("wallet/statusmessage").InnerText.Trim();
                String receivedordertype = xmlDocument.SelectSingleNode("wallet/ordertype").InnerText.Trim();
                String receivedchecksum = xmlDocument.SelectSingleNode("wallet/checksum").InnerText.Trim();
                String checksumString2 = "'" + receivedstatuscode + "''" + receivedorderid + "''" + receivedrefid + "''" + receivedamount + "''" + receivedstatusmessage + "''" + receivedordertype + "'";
                String checksum2 = calculateChecksum(WorkingKey, checksumString2);
                if ((checksum2 == receivedchecksum) && (OrderId == receivedorderid) && (Convert.ToDouble(Amount) == Convert.ToDouble(receivedamount)))
                {
                    returnDict.Add("statuscode", receivedstatuscode);
                    returnDict.Add("orderid", receivedorderid);
                    returnDict.Add("refid", receivedrefid);
                    returnDict.Add("amount", receivedamount);
                    returnDict.Add("statusmessage", receivedstatusmessage);
                    returnDict.Add("ordertype", receivedordertype);
                    returnDict.Add("checksum", receivedchecksum);
                    returnDict.Add("flag", "true");
                }
                else
                {
                    returnDict.Add("flag", "false");
                }

                return returnDict;
            }
            catch (Exception err)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, OrderId, 3, false, "777777.MOBIKWIK:ERROR  PaymentResponseMobikwik", "Error: " + err.Message.ToArray());

                return returnDict;
            }
        }



        public string GetHMACSHA256(string text, string key)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(key);
            byte[] message = encoder.GetBytes(text);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
        public static Boolean verifyChecksum(String secretKey, String allParamVauleExceptChecksum, String checksumReceived)
        {

            byte[] dataToEncryptByte = Encoding.UTF8.GetBytes(allParamVauleExceptChecksum);
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);
            byte[] checksumCalculatedByte = hmacsha256.ComputeHash(dataToEncryptByte); ;
            String checksumCalculated = toHex(checksumCalculatedByte);

            if (checksumReceived.Equals(checksumCalculated))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static string toHex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();


        }

        public string RedirectToApp(int Result_OrderId, String Trnid = "notrnid", int paymentid = 0, string agentname = "")
        {
            // (1) success - return "1"  (given  order id(no transation id))
            // (2) cancel -  return "2" (given transation id(no order id))
            // (3) Failed -  return "3"(given transation id(no order id))

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(agentname))
            {
                if (agentname.IndexOf("salebhai") > -1)
                {
                    String csname1 = "PopupScript";
                    Type cstype = this.GetType();

                    // Get a ClientScriptManager reference from the Page class.
                    ClientScriptManager cs = Page.ClientScript;

                    // Check to see if the startup script is already registered.
                    if (!cs.IsStartupScriptRegistered(cstype, csname1))
                    {
                        // StringBuilder cstext1 = new StringBuilder();

                        sb.Append("<script type = 'text/javascript'>");
                        //sb.Append("$(document).ready(function () { ");
                        sb.Append("function  openApp()  {");
                        try
                        {

                            if (Result_OrderId > 0) // OrderId generated
                            {
                                sb.Append("Android.OrederSuccessFully('" + Result_OrderId + "', '1');");
                                // cstext1.Append("<script type='text/javascript'> Android.OrederSuccessFully('" + Result_OrderId + "', '1');  </");

                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, Trnid, paymentid, false, "5.SalebhaiPaymentResponse:  SCRIPT: IN TRY GET ORDERID", "Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb.ToString());
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(Trnid))
                                {
                                    // cstext1.Append("<script type='text/javascript'> Android.OrederSuccessFully('" + Trnid + "', '3'); </");
                                    sb.Append("Android.OrederSuccessFully('" + Trnid + "', '3');");
                                }
                                else
                                {
                                    Trnid = "notrnfound";
                                    //cstext1.Append("<script type='text/javascript'> Android.OrederSuccessFully('" + Trnid + "', '3'); </");
                                    sb.Append("Android.OrederSuccessFully('" + Trnid + "', '3');");
                                }
                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, Trnid, paymentid, false, "5.SalebhaiPaymentResponse:  SCRIPT: IN TRY NOT GOT ORDERID", "Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb.ToString());
                            }



                        }
                        catch (Exception err)
                        {

                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, Trnid, paymentid, false, "5.SalebhaiPaymentResponse: ORDER GENERATED APP SCRIPT: ", "ERROR MSG:: " + err.Message.ToString() + "*****Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb.ToString());

                        }


                        sb.Append("  } openApp(); </script>");

                        // cstext1.Append("script>");
                        Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, Trnid, paymentid, false, "5.SalebhaiPaymentResponse: ORDER GENERATED APP SCRIPT: ", "Result_OrderId val =  " + Result_OrderId.ToString() + "Success : " + sb.ToString());
                        //   ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), sb.ToString(), "", true);
                        return sb.ToString();
                    }



                }
                else
                {
                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, Trnid, paymentid, false, "5.SalebhaiPaymentResponse: AGENT NOT FOUND", "Result_OrderId val =  " + Result_OrderId.ToString() + "FAIL : ");
                }
            }
            else
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, Trnid, paymentid, false, "5.SalebhaiPaymentResponse: AGENT NAME NULL: " + agentname, "Result_OrderId val =  " + Result_OrderId.ToString() + "FAIL : ");
            }

            return null;
        }

        #region Update Transaction
        public int UpdateAndSaveTransactionBeforeOrderGenerated(int paymentid = 0, string txnId = "", string WholeResponse = "", int Statuse = 2, string RespCode = "", string status_API_Response = "", string Resp_Msg = "")
        {
            DataTable TransactionDetails = dbCon.GetDataTable("SELECT TOP 1 [Id] ,[TxnId] ,[CustomerId] ,[Email] ,[Mobile] ,[OrderId] ,[Cit_PaymentRecivedByCitrus] ,[PaymentConfirmationReceived] ,[OrderAmount] ,[Cit_TimeOfTransaction] ,[Cit_respCode] ,[TimeOfTransaction] ,[Order_TimeOfTransaction] ,[Statuse] ,[StatuseString] ,[HasCame] ,[CameTime] ,[Payment_Method_Id] ,[IsFailTransactionMailSent] ,[IsPaymentSuccess] ,[TransactionSource] FROM [CitrusPayment] where TxnId='" + txnId + "' and OrderId = 0 order by TimeOfTransaction desc");


            if (TransactionDetails != null && TransactionDetails.Rows.Count > 0)
            {
                try
                {
                    #region Update TRANSACTION


                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Information, txnId, 2, false, "1.UpdateAndSaveTransactionBeforeOrderGenerated Trn: Got TRN", WholeResponse);

                    String Str1 = "UPDATE [dbo].[CitrusPayment] SET [Statuse] = @1 ,[StatuseString] = @2 ,[HasCame] = @3,[FeachTrial]= @4 ,[IsRecordsFetchedFromCitrus]= @5 ,[Cit_respCode]=@6,[Cit_respMsg] = @7, [CameTime] = DATEADD(MINUTE, 330, GETUTCDATE()) WHERE [TxnId] = '" + txnId + "'";

                    string[] parm = { Statuse.ToString(), WholeResponse, "1", "1", "1", RespCode, Resp_Msg };

                    int resval = dbCon.ExecuteQueryWithParams(Str1, parm);

                    if (resval > 0)
                    {
                        // TRANSACTION UPDATED
                        return resval;
                    }
                    else
                    {
                        // ISSUE IN TRANSACTION UPDATE
                        return resval;
                    }
                    #endregion
                }
                catch (Exception err)
                {

                    return 0;
                }

            }
            else
            {
                //TRN NOT FOUND
                return 0;
            }


        }

        #endregion
    }
}

//success,code,message,data(providerReferenceId,paymentState,amount)

//public class phonePaeResponsee
//{
//    public phonePaeResponsee()
//    {

//    }
//    public bool success { get; set; }

//    public string code { get; set; }

//    public string message { get; set; }

//    public string MyProperty { get; set; }

//    public Data data { get; set; }
//}

//public class dataList
//{
//    public string providerReferenceId { get; set; }

//    public string paymentState { get; set; }

//    public string amount { get; set; }
//}

public class Data
{
    public string transactionId { get; set; }
    public string merchantId { get; set; }
    public string providerReferenceId { get; set; }
    public decimal amount { get; set; }
    public string paymentState { get; set; }
    public string payResponseCode { get; set; }
}

public class phonePaeResponsee
{
    public bool success { get; set; }
    public string code { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}
//RESPCODE,RESPMSG,TXNDATE,TXNAMOUNT,TXNID

public class paytmResponse
{
    public string RESPCODE { get; set; }

    public string RESPMSG { get; set; }

    public string TXNDATE { get; set; }

    public string TXNAMOUNT { get; set; }

    public string TXNID { get; set; }
}