using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Test0555
{
    public class GenerateOrder
    {
        dbConnection dbCon = new dbConnection();
        public int CreateOrderFromAlternetOrder(string txnId, string amount)
        {
            try
            {
                int AlternetOrderId = 0;
                int OrderId = 0;
                int addressid = 0;
                int cust_id = 0;
                decimal redeemedamount = 0;

                #region Insert Order
                DataTable Get_ALternetOrder = dbCon.GetDataTable("select * from AlterNetOrder where TrnId='" + txnId + "' order by CreatedOnUtc desc;");

                if (Get_ALternetOrder != null && Get_ALternetOrder.Rows.Count > 0)
                {
                    for (int order = 0; order < Get_ALternetOrder.Rows.Count; order++)
                    {
                        try
                        {
                            decimal.TryParse(Get_ALternetOrder.Rows[0]["Customer_Redeem_Amount"].ToString(), out redeemedamount);
                            int.TryParse(Get_ALternetOrder.Rows[0]["Id"].ToString(), out AlternetOrderId);
                            int.TryParse(Get_ALternetOrder.Rows[0]["AddressId"].ToString(), out addressid);
                            int.TryParse(Get_ALternetOrder.Rows[0]["CustomerId"].ToString(), out cust_id);

                            if (addressid == 0)
                            {
                                DataTable add_id = dbCon.GetDataTable("select Top 1 Address_Id from CustomerAddress where CustomerId = " + cust_id + " order by Id desc;");

                                if (add_id != null && add_id.Rows.Count > 0)
                                {
                                    if (addressid == 0)
                                    {
                                        int.TryParse(add_id.Rows[0]["Id"].ToString(), out addressid);
                                    }
                                }
                            }
                            #region ORDER INSERT PROCESS

                            string[] insert = {
Get_ALternetOrder.Rows[0]["OrderGuid"].ToString(),
Get_ALternetOrder.Rows[0]["CustomerId"].ToString(),
addressid.ToString(),
Get_ALternetOrder.Rows[0]["OrderStatusId"].ToString(),
Get_ALternetOrder.Rows[0]["OrderDiscount"].ToString(),
Get_ALternetOrder.Rows[0]["OrderMRP"].ToString(),
Get_ALternetOrder.Rows[0]["OrderTotal"].ToString(),
Get_ALternetOrder.Rows[0]["ShippingMethod"].ToString(),
Get_ALternetOrder.Rows[0]["TotalQty"].ToString(),
amount,
Get_ALternetOrder.Rows[0]["TotalGram"].ToString(),
Get_ALternetOrder.Rows[0]["Customer_Redeem_Amount"].ToString(),
txnId,
Get_ALternetOrder.Rows[0]["IsPaymentDone"].ToString(),
Get_ALternetOrder.Rows[0]["CustOfferCode"].ToString(),
Get_ALternetOrder.Rows[0]["RefferedOfferCode"].ToString(),
Get_ALternetOrder.Rows[0]["PaymentGatewayId"].ToString(),
Get_ALternetOrder.Rows[0]["BuyWith"].ToString(),
};
                            string cmd = "INSERT INTO [dbo].[Order]([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[ShippingId],[TotalQTY],[PaidAmount],[TotalGram],[CustReedeemAmount],[TRNID],[IsPaymentDone],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[CreatedOnUtc],[UpdatedOnUtc]) VALUES(@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,DATEADD(MINUTE, 330, GETUTCDATE()),DATEADD(MINUTE, 330, GETUTCDATE())); SELECT SCOPE_IDENTITY();";

                            OrderId = dbCon.ExecuteScalarQueryWithParams(cmd, insert);

                            if (OrderId == 0)
                            {
                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.Order Id not generated", " ");
                                return 0;
                                //PRINT ERROR : ORDER ID 0
                            }
                            else
                            {

                            }
                            #endregion
                            if (AlternetOrderId > 0 && OrderId > 0)
                            {
                                #region Insert OrderItem

                                DataTable Get_AlternetOrderItem = dbCon.GetDataTable("SELECT *  FROM [AlterNetOrderItem] where OrderId = '" + Get_ALternetOrder.Rows[order]["Id"].ToString() + "' order by CreatedOnUtc desc;");

                                if (Get_AlternetOrderItem != null && Get_AlternetOrderItem.Rows.Count > 0)
                                {
                                    for (int orderitem = 0; orderitem < Get_AlternetOrderItem.Rows.Count; orderitem++)
                                    {
                                        try
                                        {
                                            string[] insertitem = {
OrderId.ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ProductId"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["Quantity"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["MrpPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BuyWithPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["DiscountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ExtraDiscountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["SGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["SGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["CGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["CGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["IGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["IGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["TaxablePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["TotalAmount"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ProductName"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BuyWith"].ToString(),
};
                                            string cmditem = "INSERT INTO [dbo].[OrderItem] ([OrderId] ,[ProductId] ,[Quantity] ,[MrpPerUnit],[BuyWithPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[CreatedOnUtc]) VALUES  (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,DATEADD(MINUTE, 330, GETUTCDATE())); SELECT SCOPE_IDENTITY();";

                                            int OrderItemId = dbCon.ExecuteScalarQueryWithParams(cmditem, insertitem);
                                            try
                                            {
                                                if (OrderItemId > 0 && redeemedamount > 0)
                                                {
                                                    string Drwallet = "insert into [Customer_Wallet_History] ([CustomerId],[Dr_Amount],[Cr_Amount],[DOC],[Wallet_Type],[Message],[OrderId],[Amount]) values (@1,@2,@3,GETDATE(),@4,@5,@6,@7);Select SCOPE_IDENTITY()";
                                                    string[] paramDRWallet = { cust_id.ToString(), redeemedamount.ToString(), "0", "System", "OrderId " + OrderId.ToString() + "", OrderId.ToString(), "0" };
                                                    int DR_rslt = dbCon.ExecuteScalarQueryWithParams(Drwallet, paramDRWallet);
                                                }

                                            }
                                            catch (Exception)
                                            {

                                            }
                                            if (OrderItemId == 0)
                                            {
                                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.Order Item not generated", " ");
                                                return 0;
                                                //PRINT ERROR : ORDER ITEM ID 0
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "OrderItem Id not generated" + txnId, ex.Message.ToString());
                                            return 0;
                                            //PRINT ERROR
                                        }
                                        finally
                                        {
                                            dbCon.closeConnection();
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.EMPTY DATA TABLE FOUND OF ALTERNET-ORDERITEM", " ");
                                    // EMPTY DATA TABLE FOUND OF ALTERNET-ORDERITEM!
                                    return 0;
                                }
                                #endregion
                            }
                            else
                            {
                                return 0;
                                //PRINT ERROR
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "Order Id not generated", ex.Message.ToString());
                            return 0;
                        }
                        finally
                        {
                            dbCon.closeConnection();
                        }
                    }

                    #region SendFIRSTOrderSMS
                    if (OrderId != 0)
                    {
                        try
                        {
                            DataTable dtcheck = dbCon.GetDataTable("select distinct Customer.Id,Customer.Mobile from Customer inner  join [Order] on [Order].CustomerId=[Customer].Id where  Customer.Id=" + cust_id + " group by Customer.Id,Customer.Mobile having count([Order].Id)>=1");

                            string data = "SELECT [Order].OrderMRP, [Order].AddressId, [Order].OrderTotal,[Order].CustOfferCode, CONVERT(varchar, Product.EndDate,106)+' '+ CONVERT(varchar, Product.EndDate,108) as lastdate FROM  [Order] INNER JOIN OrderItem ON [Order].Id = OrderItem.OrderId INNER JOIN Product ON OrderItem.ProductId = Product.Id where [Order].Id=" + OrderId;
                            DataTable dtdata = dbCon.GetDataTable(data);


                            string price = "Rs." + dtdata.Rows[0]["OrderTotal"].ToString();
                            string date = dtdata.Rows[0]["lastdate"].ToString();
                            string orderidid = OrderId.ToString();
                            string offcode = dbCon.Base64Encode(orderidid);

                            DataTable dtcustomerwp = dbCon.GetDataTable("select ISNULL(IsInWhatsappGroup,0) as IsInWhatsappGroup FROM Customer where  Customer.Id=" + cust_id);
                            var IsInWhatsappGroup = Convert.ToBoolean(dtcustomerwp.Rows[0]["IsInWhatsappGroup"]);
                            var AddId = dtdata.Rows[0]["AddressId"].ToString();
                            DataTable dtcustomerzip = dbCon.GetDataTable("select PinCode FROM CustomerAddress  where Id=" + AddId);

                            var zipcode = dtcustomerzip.Rows[0]["PinCode"].ToString();
                            DataTable dtwpurl = dbCon.GetDataTable("select Url FROM WhatsappUrls  where zipcode=" + zipcode);

                            var wpurl = dtwpurl.Rows[0]["Url"].ToString();


                            if (dtcheck.Rows.Count > 0)
                            {
                                string SMS_Text11 = "Your SoSho order " + OrderId + " for COD  " + price + " placed. Delivery in 1-2 days." +
                                    Environment.NewLine + "Join us to receive exciting offer from sosho click here " + wpurl;

                                //string SMS_Text11 = "Your OrderNo:"+ OrderId +" has been Successfully Placed.";
                                dbCon.SendSMS(dtcheck.Rows[0]["Mobile"].ToString(), SMS_Text11);

                                //if (!IsInWhatsappGroup)
                                //{
                                //    string smstxt = "Join us to receive exciting offer from sosho click here " + wpurl;
                                //    dbCon.SendSMS(dtcheck.Rows[0]["Mobile"].ToString(), smstxt);

                                //}
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }
                    #endregion





                    //#region SendFIRSTOrderSMS
                    //if (OrderId != 0)
                    //{                          
                    //    try
                    //    {
                    //        DataTable dtcheck = dbCon.GetDataTable("select distinct Customer.Id,Customer.Mobile from Customer inner  join [Order] on [Order].CustomerId=[Customer].Id where  [Customer].Email is not null and Customer.Id=" + cust_id + " group by Customer.Id,Customer.Mobile having count([Order].Id)>=1");

                    //        if (dtcheck.Rows.Count > 0)
                    //        {
                    //            string SMS_Text11 = "Your OrderNo:"+ OrderId +" has been Successfully Placed.";
                    //            int res1 = dbCon.Customer_Wallet_SMS(dtcheck.Rows[0]["Mobile"].ToString(), SMS_Text11);
                    //        }

                    //    }
                    //    catch (Exception e)
                    //    {

                    //    }

                    //}
                    //#endregion

                    //#region sendMail
                    //string sendMailTemplete = @"select * from MessageTemplate where Id=1";
                    //DataTable dtCustomersendMail = dbCon.GetDataTable(sendMailTemplete);
                    //if (dtCustomersendMail != null && dtCustomersendMail.Rows.Count > 0)
                    //{
                    //    string Body = dtCustomersendMail.Rows[0]["Body"].ToString();
                    //    StringBuilder sb = new StringBuilder();
                    //    sb.Append(Body);
                    //    string ToEmail = dbCon.GetEmailFromCustomerId(cust_id.ToString());
                    //    sb.Replace("%OrderId%", OrderId.ToString());
                    //    decimal PaidAmount = Convert.ToDecimal(amount);
                    //    sb.Replace("%PaidAmount%", PaidAmount.ToString("0.##"));

                    //    double s= SendNotificationEmail("Thank you for your order on salebhai.com", sb.ToString(), "1", "cs@salebhai.com", "Salebhai", ToEmail, ToEmail, "1");
                    //}
                    //#endregion
                }

                #endregion

                return OrderId;
            }

            catch (Exception ex)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.EMPTY DATA TABLE FOUND", " ");
                return 0;
            }
            finally
            {
                dbCon.closeConnection();
            }
        }
        public double SendNotificationEmail(string subject, string body, string Priority, string From, string FromName, string To, string ToName, string EmailAccountId)
        {
            dbConnection dbc = new dbConnection();
            string InsertEmail = "";
            int rest = 0;
            try
            {
                InsertEmail = "INSERT INTO dbo.QueuedEmail([Priority],[From],FromName,[To],ToName,[Subject],Body,CreatedOnUtc,EmailAccountId,AttachedDownloadId,SentTries,isSent,inProcess,AttachmentFileName,AttachmentFilePath) VALUES(@1,@2,@3,@4,@5,@6,@7,GETUTCDATE(),@8,0,0,0,0,@9,@10)";
                string[] parm = { Priority, From, FromName, To, ToName, subject, body, EmailAccountId, "", "" };
                rest = dbc.ExecuteQueryWithParams(InsertEmail, parm);
            }
            catch (Exception err)
            {
                //Logger.InsertLogs(InvoiceLOGS.InvoiceLogLevel.Error, InsertEmail, 0, false, "1.Qued Email Customer", " ");
            }
            return rest;
        }






        public int CreateOrderFromAlternetOrderForMultiple(string txnId, string amount)
        {
            try
            {
                int AlternetOrderId = 0;
                int OrderId = 0;
                int addressid = 0;
                int cust_id = 0;
                decimal redeemedamount = 0;

                #region Insert Order
                DataTable Get_ALternetOrder = dbCon.GetDataTable("select * from AlterNetOrder where TrnId='" + txnId + "' order by CreatedOnUtc desc;");

                if (Get_ALternetOrder != null && Get_ALternetOrder.Rows.Count > 0)
                {
                    for (int order = 0; order < Get_ALternetOrder.Rows.Count; order++)
                    {
                        try
                        {
                            decimal.TryParse(Get_ALternetOrder.Rows[0]["Customer_Redeem_Amount"].ToString(), out redeemedamount);
                            int.TryParse(Get_ALternetOrder.Rows[0]["Id"].ToString(), out AlternetOrderId);
                            int.TryParse(Get_ALternetOrder.Rows[0]["AddressId"].ToString(), out addressid);
                            int.TryParse(Get_ALternetOrder.Rows[0]["CustomerId"].ToString(), out cust_id);

                            if (addressid == 0)
                            {
                                DataTable add_id = dbCon.GetDataTable("select Top 1 Address_Id from CustomerAddress where CustomerId = " + cust_id + " order by Id desc;");

                                if (add_id != null && add_id.Rows.Count > 0)
                                {
                                    if (addressid == 0)
                                    {
                                        int.TryParse(add_id.Rows[0]["Id"].ToString(), out addressid);
                                    }
                                }
                            }
                            #region ORDER INSERT PROCESS

                            string[] insert = {
Get_ALternetOrder.Rows[0]["OrderGuid"].ToString(),
Get_ALternetOrder.Rows[0]["CustomerId"].ToString(),
addressid.ToString(),
Get_ALternetOrder.Rows[0]["OrderStatusId"].ToString(),
Get_ALternetOrder.Rows[0]["OrderDiscount"].ToString(),
Get_ALternetOrder.Rows[0]["OrderMRP"].ToString(),
Get_ALternetOrder.Rows[0]["OrderTotal"].ToString(),
Get_ALternetOrder.Rows[0]["ShippingMethod"].ToString(),
Get_ALternetOrder.Rows[0]["TotalQty"].ToString(),
Get_ALternetOrder.Rows[0]["PaidAmount"].ToString(),//amount,
Get_ALternetOrder.Rows[0]["TotalGram"].ToString(),
Get_ALternetOrder.Rows[0]["Customer_Redeem_Amount"].ToString(),
txnId,
Get_ALternetOrder.Rows[0]["IsPaymentDone"].ToString(),
Get_ALternetOrder.Rows[0]["CustOfferCode"].ToString(),
Get_ALternetOrder.Rows[0]["RefferedOfferCode"].ToString(),
Get_ALternetOrder.Rows[0]["PaymentGatewayId"].ToString(),
Get_ALternetOrder.Rows[0]["BuyWith"].ToString(),
Get_ALternetOrder.Rows[0]["JurisdictionID"].ToString(),
Get_ALternetOrder.Rows[0]["CashbackAmount"].ToString(),
Get_ALternetOrder.Rows[0]["ReOrderId"].ToString()
};
                            string cmd = "INSERT INTO [dbo].[Order]([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[ShippingId],[TotalQTY],[PaidAmount],[TotalGram],[CustReedeemAmount],[TRNID],[IsPaymentDone],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[CreatedOnUtc],[UpdatedOnUtc],[JurisdictionID],[CashbackAmount],[ReOrderId]) VALUES(@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,DATEADD(MINUTE, 330, GETUTCDATE()),DATEADD(MINUTE, 330, GETUTCDATE()),@19,@20,@21); SELECT SCOPE_IDENTITY();";
                            try
                            {
                                OrderId = dbCon.ExecuteScalarQueryWithParams(cmd, insert);
                            }
                            catch (Exception ex)
                            {

                            }
                           

                            if (OrderId == 0)
                            {
                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.Order Id not generated", " ");
                                return 0;
                                //PRINT ERROR : ORDER ID 0
                            }
                            else
                            {

                            }
                            #endregion
                            if (AlternetOrderId > 0 && OrderId > 0)
                            {
                                #region Insert OrderItem

                                DataTable Get_AlternetOrderItem = dbCon.GetDataTable("SELECT *  FROM [AlterNetOrderItem] where OrderId = '" + Get_ALternetOrder.Rows[order]["Id"].ToString() + "' order by CreatedOnUtc desc;");

                                if (Get_AlternetOrderItem != null && Get_AlternetOrderItem.Rows.Count > 0)
                                {
                                    for (int orderitem = 0; orderitem < Get_AlternetOrderItem.Rows.Count; orderitem++)
                                    {
                                        try
                                        {
                                            string[] insertitem = {
OrderId.ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ProductId"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["Quantity"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["MrpPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BuyWithPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["DiscountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ExtraDiscountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["SGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["SGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["CGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["CGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["IGSTValuePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["IGSTAmountPerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["TaxablePerUnit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["TotalAmount"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["ProductName"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BuyWith"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["CustOfferCode"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["RefferedOfferCode"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["UnitId"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["Unit"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["AttributeId"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BannerProductType"].ToString(),
Get_AlternetOrderItem.Rows[orderitem]["BannerId"].ToString()
};
                                            string cmditem = "INSERT INTO [dbo].[OrderItem] ([OrderId] ,[ProductId] ,[Quantity] ,[MrpPerUnit],[BuyWithPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[CreatedOnUtc],[CustOfferCode],[RefferedOfferCode],[UnitId],[Unit],[AttributeId],[BannerProductType],[BannerId]) VALUES  (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,DATEADD(MINUTE, 330, GETUTCDATE()),@18,@19,@20,@21,@22,@23,@24); SELECT SCOPE_IDENTITY();";

                                            int OrderItemId = dbCon.ExecuteScalarQueryWithParams(cmditem, insertitem);
                                            try
                                            {
                                                if (OrderItemId > 0 && redeemedamount > 0)
                                                {
                                                    string Drwallet = "insert into [Customer_Wallet_History] ([CustomerId],[Dr_Amount],[Cr_Amount],[DOC],[Wallet_Type],[Message],[OrderId],[Amount]) values (@1,@2,@3,GETDATE(),@4,@5,@6,@7);Select SCOPE_IDENTITY()";
                                                    string[] paramDRWallet = { cust_id.ToString(), redeemedamount.ToString(), "0", "System", "OrderId " + OrderId.ToString() + "", OrderId.ToString(), "0" };
                                                    int DR_rslt = dbCon.ExecuteScalarQueryWithParams(Drwallet, paramDRWallet);
                                                }

                                            }
                                            catch (Exception)
                                            {

                                            }
                                            if (OrderItemId == 0)
                                            {
                                                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.Order Item not generated", " ");
                                                return 0;
                                                //PRINT ERROR : ORDER ITEM ID 0
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "OrderItem Id not generated" + txnId, ex.Message.ToString());
                                            return 0;
                                            //PRINT ERROR
                                        }
                                        finally
                                        {
                                            dbCon.closeConnection();
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.EMPTY DATA TABLE FOUND OF ALTERNET-ORDERITEM", " ");
                                    // EMPTY DATA TABLE FOUND OF ALTERNET-ORDERITEM!
                                    return 0;
                                }
                                #endregion
                            }
                            else
                            {
                                return 0;
                                //PRINT ERROR
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "Order Id not generated", ex.Message.ToString());
                            return 0;
                        }
                        finally
                        {
                            dbCon.closeConnection();
                        }
                    }

                    #region SendFIRSTOrderSMS
                    if (OrderId != 0)
                    {
                        try
                        {
                            DataTable dtcheck = dbCon.GetDataTable("select distinct Customer.Id,Customer.Mobile from Customer inner  join [Order] on [Order].CustomerId=[Customer].Id where  Customer.Id=" + cust_id + " group by Customer.Id,Customer.Mobile having count([Order].Id)>=1");

                            string data = "SELECT [Order].OrderMRP,[Order].AddressId, [Order].OrderTotal,[Order].CustOfferCode, CONVERT(varchar, Product.EndDate,106)+' '+ CONVERT(varchar, Product.EndDate,108) as lastdate FROM  [Order] INNER JOIN OrderItem ON [Order].Id = OrderItem.OrderId INNER JOIN Product ON OrderItem.ProductId = Product.Id where [Order].Id=" + OrderId;
                            DataTable dtdata = dbCon.GetDataTable(data);


                            string price = "Rs." + dtdata.Rows[0]["OrderTotal"].ToString();
                            string date = dtdata.Rows[0]["lastdate"].ToString();
                            string orderidid = OrderId.ToString();
                            string offcode = dbCon.Base64Encode(orderidid);

                            DataTable dtcustomerwp = dbCon.GetDataTable("select ISNULL(IsInWhatsappGroup,0) as IsInWhatsappGroup FROM Customer where  Customer.Id=" + cust_id);
                            var IsInWhatsappGroup = Convert.ToBoolean(dtcustomerwp.Rows[0]["IsInWhatsappGroup"]);
                            var AddId = dtdata.Rows[0]["AddressId"].ToString();
                            DataTable dtcustomerzip = dbCon.GetDataTable("select PinCode FROM CustomerAddress  where Id=" + AddId);

                            var zipcode = dtcustomerzip.Rows[0]["PinCode"].ToString();
                            DataTable dtwpurl = dbCon.GetDataTable("select Url FROM WhatsappUrls  where zipcode=" + zipcode);

                            var wpurl = dtwpurl.Rows[0]["Url"].ToString();

                            if (dtcheck.Rows.Count > 0)
                            {
                                string SMS_Text11 = "Your SoSho order " + OrderId + " for COD  " + price + " placed. Delivery in 1-2 days." +
                                     Environment.NewLine + "Join us to receive exciting offer from sosho click here " + wpurl;

                                //string SMS_Text11 = "Your OrderNo:"+ OrderId +" has been Successfully Placed.";
                                dbCon.SendSMS(dtcheck.Rows[0]["Mobile"].ToString(), SMS_Text11);
                                //if (!IsInWhatsappGroup)
                                //{
                                //    string smstxt = "Join us to receive exciting offer from sosho click here " + wpurl;
                                //    dbCon.SendSMS(dtcheck.Rows[0]["Mobile"].ToString(), smstxt);

                                //}
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }
                    #endregion





                    //#region SendFIRSTOrderSMS
                    //if (OrderId != 0)
                    //{                          
                    //    try
                    //    {
                    //        DataTable dtcheck = dbCon.GetDataTable("select distinct Customer.Id,Customer.Mobile from Customer inner  join [Order] on [Order].CustomerId=[Customer].Id where  [Customer].Email is not null and Customer.Id=" + cust_id + " group by Customer.Id,Customer.Mobile having count([Order].Id)>=1");

                    //        if (dtcheck.Rows.Count > 0)
                    //        {
                    //            string SMS_Text11 = "Your OrderNo:"+ OrderId +" has been Successfully Placed.";
                    //            int res1 = dbCon.Customer_Wallet_SMS(dtcheck.Rows[0]["Mobile"].ToString(), SMS_Text11);
                    //        }

                    //    }
                    //    catch (Exception e)
                    //    {

                    //    }

                    //}
                    //#endregion

                    //#region sendMail
                    //string sendMailTemplete = @"select * from MessageTemplate where Id=1";
                    //DataTable dtCustomersendMail = dbCon.GetDataTable(sendMailTemplete);
                    //if (dtCustomersendMail != null && dtCustomersendMail.Rows.Count > 0)
                    //{
                    //    string Body = dtCustomersendMail.Rows[0]["Body"].ToString();
                    //    StringBuilder sb = new StringBuilder();
                    //    sb.Append(Body);
                    //    string ToEmail = dbCon.GetEmailFromCustomerId(cust_id.ToString());
                    //    sb.Replace("%OrderId%", OrderId.ToString());
                    //    decimal PaidAmount = Convert.ToDecimal(amount);
                    //    sb.Replace("%PaidAmount%", PaidAmount.ToString("0.##"));

                    //    double s= SendNotificationEmail("Thank you for your order on salebhai.com", sb.ToString(), "1", "cs@salebhai.com", "Salebhai", ToEmail, ToEmail, "1");
                    //}
                    //#endregion
                }

                #endregion

                return OrderId;
            }

            catch (Exception ex)
            {
                Logger.InsertLogs(Test0555.Logger.InvoiceLOGS.InvoiceLogLevel.Error, txnId, 0, false, "1.EMPTY DATA TABLE FOUND", " ");
                return 0;
            }
            finally
            {
                dbCon.closeConnection();
            }
        }



    }
}