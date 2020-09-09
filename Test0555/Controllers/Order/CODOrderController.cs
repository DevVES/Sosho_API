using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.Order;
using GarageXAPINEW;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Test0555.Controllers.Order
{
    public class CODOrderController : ApiController
    {
        dbConnection dbCon = new dbConnection();

        [HttpGet]
        public CODOrderModel CODPlaceOrder(String CustomerId, decimal PaidAmount, string AddressId, string Quantity, string buywith, string discountamount = "", string Redeemeamount = "", string couponCode = "", string refrcode = "")
        {
            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start ");

            ////START 01-31-2020 CODE MODIFIED FOR GENERATING COOUPONCODE
            //decimal payableamt = 0;
            //decimal.TryParse(PaidAmount.ToString(), out payableamt);
            //if (payableamt > 0)
            //{
            //    NewCode:
            //    string coddd = dbCon.GenerateRandomNumber().ToString();
            //    int test = getcheck(coddd);

            //    if (test == 0)
            //    {
            //        goto NewCode;
            //    }
            //    couponCode = coddd;
            //}
            ////END 01-31-2020 CODE MODIFIED FOR GENERATING COOUPONCODE

            CODOrderModel objCODplaceorder = new CODOrderModel();
            try
            {
                //if (String.IsNullOrEmpty(couponCode) || couponCode == "0")
                //{
                //    couponCode = ClsCommon.GenerateRandomNumber().ToString();
                //}


                #region Avoid multiple Entry
                bool IsOrderGeneratedInLastFiveMinutes = false;
                try
                {
                    DataTable dtLastOrder = dbCon.GetDataTable("SELECT top 1 DATEDIFF(mi,[CreatedOnUtc],'" + dbCon.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss tt") + "'),Id,CustOfferCode FROM [dbo].[Order] where CustomerId=" + CustomerId + " order by id desc");
                    if (dtLastOrder != null && dtLastOrder.Rows.Count > 0)
                    {
                        //Order Place After 3 min
                        if (Convert.ToInt32(dtLastOrder.Rows[0][0].ToString()) < 5)
                        {
                            IsOrderGeneratedInLastFiveMinutes = true;
                            objCODplaceorder.resultflag = "1";
                            objCODplaceorder.OrderId = dtLastOrder.Rows[0]["Id"].ToString();
                            objCODplaceorder.Message = "Your OrderNo:" + dtLastOrder.Rows[0]["Id"].ToString() + " has been succesfully generated";
                            objCODplaceorder.Ccode = couponCode;
                        }
                    }
                }
                catch (Exception E) { }
                #endregion

                //START 24-04-2020 : ADDED CODE FOR CHECKING PIN CODE
                //Models.CheckPincode.CheckPincodeModel objchkpincode = new Models.CheckPincode.CheckPincodeModel();
                //objchkpincode = CheckPincode(Convert.ToInt32(objCODplaceorder.OrderId), Convert.ToInt32(CustomerId));
                //if (objchkpincode.resultflag == "0")
                //{
                //    objCODplaceorder.resultflag = "0";
                //    objCODplaceorder.Message = objchkpincode.Message;
                //    return objCODplaceorder;
                //}                
                //END 24-04-2020 : ADDED CODE FOR CHECKING PIN CODE

                if (!IsOrderGeneratedInLastFiveMinutes)
                {
                    string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + CustomerId;
                    if (PaidAmount > 0)
                    {
                        int result = dbCon.CreateTransaction(merchantTxnId, CustomerId, PaidAmount);
                        if (result == 1)
                        {
                            string chektrans = "select id from AlterNetOrder where trnid=@1";
                            string[] param = { merchantTxnId };
                            DataTable dtcheckTrans = dbCon.GetDataTableWithParams(chektrans, param);
                            if (dtcheckTrans != null && dtcheckTrans.Rows.Count > 0)
                            {
                                dbCon.deleteAlternateOrder(merchantTxnId);
                            }
                            else
                            {
                                int chkalternat = CreateAlternateOrder(CustomerId, merchantTxnId, AddressId, PaidAmount.ToString(), Quantity, buywith, discountamount, Redeemeamount, couponCode, refrcode);
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 14");
                                if (chkalternat > 0 && PaidAmount > 0)
                                {
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 15");
                                    int OrderId = CODPlaceOrder123456(merchantTxnId);
                                    if (OrderId > 0)
                                    {
                                        //START 20-02-2020 - Added Code To Track Source
                                        try
                                        {
                                            string values = string.Empty;
                                            Logger.InsertLogsApp("Inserting Source Of Order");
                                            if (Request.Headers.GetValues("DeviceType").First() != null)
                                            {
                                                Logger.InsertLogsApp(Request.Headers.GetValues("DeviceType").First() + " - Device Order Received From");
                                                values = Request.Headers.GetValues("DeviceType").First();
                                                if (values != null)
                                                {
                                                    string[] insert = { OrderId.ToString(), values };
                                                    string insertdeviceidentity = "INSERT INTO [dbo].[Order_Source]([OrderID],[OrderSourceName]) VALUES (@1,@2);";
                                                    int sourceid = dbCon.ExecuteScalarQueryWithParams(insertdeviceidentity, insert);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, ex.Message);
                                        }
                                        //END 20-02-2020 - Added Code To Track Source




                                        // if success, Update the transaction 
                                        #region Update Transaction
                                        try
                                        {
                                            String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction]= @2,[Statuse] = @3 ,[IsPaymentSuccess]=@4 WHERE [TxnId] =  '" + merchantTxnId + "'";
                                            string[] parms_new = { OrderId.ToString(), dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "3", "1" };
                                            int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);
                                            if (resvaltrn > 0)
                                            {
                                                String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + merchantTxnId + "'";
                                                string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                                if (resvaltrn1 > 0)
                                                {
                                                    String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + merchantTxnId + "'";
                                                    string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                    int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                    if (resvaltrn2 > 0)
                                                    {
                                                        objCODplaceorder.resultflag = "1";
                                                        objCODplaceorder.OrderId = OrderId.ToString();
                                                        objCODplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                        objCODplaceorder.Ccode = couponCode;
                                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 16");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                objCODplaceorder.resultflag = "0";
                                                objCODplaceorder.OrderId = OrderId.ToString();
                                                objCODplaceorder.Message = "Your Order was Not Generated";
                                                objCODplaceorder.Ccode = couponCode;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            objCODplaceorder.resultflag = "0";
                                            objCODplaceorder.OrderId = OrderId.ToString();
                                            objCODplaceorder.Message = "Your Order was Not Generated";
                                            objCODplaceorder.Ccode = couponCode;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        objCODplaceorder.resultflag = "0";
                                        objCODplaceorder.OrderId = OrderId.ToString();
                                        objCODplaceorder.Message = "Your Order was Not Generated";
                                        objCODplaceorder.Ccode = couponCode;
                                    }
                                }
                                else
                                {
                                    objCODplaceorder.resultflag = "0";
                                    objCODplaceorder.Message = "Your Order was Not Generated";
                                    objCODplaceorder.Ccode = couponCode;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objCODplaceorder;
        }

        [HttpGet]
        public CODOrderModel FreeProductOrder(String CustomerId, decimal PaidAmount, string AddressId, string Quantity, string buywith, string discountamount = "", string Redeemeamount = "", string couponCode = "", string refrcode = "", string ProductId = "")
        {


            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start ");
            CODOrderModel objCODplaceorder = new CODOrderModel();
            try
            {




                #region Avoid multiple Entry
                bool IsCustomerGotFreeProduct = false;
                try
                {
                    string qry = "Select ProductId,orderid from orderitem where orderid in (Select id from [Order] where customerid=" + CustomerId + ") and ProductId in (Select FreeProductId from  FreeProduct)";
                    DataTable dtLastOrder = dbCon.GetDataTable(qry);
                    if (dtLastOrder != null && dtLastOrder.Rows.Count > 0)
                    {
                        //Order Place After 3 min
                        if (Convert.ToInt32(dtLastOrder.Rows[0][0].ToString()) < 5)
                        {
                            IsCustomerGotFreeProduct = true;
                            objCODplaceorder.resultflag = "1";
                            objCODplaceorder.OrderId = dtLastOrder.Rows[0]["orderid"].ToString();
                            objCODplaceorder.Message = " <msg text> " + dtLastOrder.Rows[0]["orderid"].ToString() + " ";
                            objCODplaceorder.Ccode = couponCode;
                        }
                    }
                }
                catch (Exception E) { }
                #endregion

                if (!IsCustomerGotFreeProduct)
                {
                    string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + CustomerId;
                    if (PaidAmount > -1)
                    {
                        int result = dbCon.CreateTransaction(merchantTxnId, CustomerId, PaidAmount);
                        if (result == 1)
                        {
                            string chektrans = "select id from AlterNetOrder where trnid=@1";
                            string[] param = { merchantTxnId };
                            DataTable dtcheckTrans = dbCon.GetDataTableWithParams(chektrans, param);
                            if (dtcheckTrans != null && dtcheckTrans.Rows.Count > 0)
                            {
                                dbCon.deleteAlternateOrder(merchantTxnId);
                            }
                            else
                            {
                                int chkalternat = CreateAlternateOrder(CustomerId, merchantTxnId, AddressId, PaidAmount.ToString(), Quantity, buywith, discountamount, Redeemeamount, couponCode, refrcode, ProductId);
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 14");
                                if (chkalternat > 0 && PaidAmount > -1)
                                {
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 15");
                                    int OrderId = CODPlaceOrder123456(merchantTxnId);
                                    if (OrderId > 0)
                                    {
                                        // if success, Update the transaction 
                                        #region Update Transaction
                                        try
                                        {
                                            String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction]= @2,[Statuse] = @3 ,[IsPaymentSuccess]=@4 WHERE [TxnId] =  '" + merchantTxnId + "'";
                                            string[] parms_new = { OrderId.ToString(), dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "3", "1" };
                                            int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);
                                            if (resvaltrn > 0)
                                            {
                                                String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + merchantTxnId + "'";
                                                string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                                if (resvaltrn1 > 0)
                                                {
                                                    String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + merchantTxnId + "'";
                                                    string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                    int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                    if (resvaltrn2 > 0)
                                                    {
                                                        objCODplaceorder.resultflag = "1";
                                                        objCODplaceorder.OrderId = OrderId.ToString();
                                                        objCODplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                        objCODplaceorder.Ccode = couponCode;
                                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 16");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                objCODplaceorder.resultflag = "0";
                                                objCODplaceorder.OrderId = OrderId.ToString();
                                                objCODplaceorder.Message = "Your Order was Not Generated";
                                                objCODplaceorder.Ccode = couponCode;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            objCODplaceorder.resultflag = "0";
                                            objCODplaceorder.OrderId = OrderId.ToString();
                                            objCODplaceorder.Message = "Your Order was Not Generated";
                                            objCODplaceorder.Ccode = couponCode;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        objCODplaceorder.resultflag = "0";
                                        objCODplaceorder.OrderId = OrderId.ToString();
                                        objCODplaceorder.Message = "Your Order was Not Generated";
                                        objCODplaceorder.Ccode = couponCode;
                                    }
                                }
                                else
                                {
                                    objCODplaceorder.resultflag = "0";
                                    objCODplaceorder.Message = "Your Order was Not Generated";
                                    objCODplaceorder.Ccode = couponCode;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objCODplaceorder;
        }

        [HttpGet]
        public CODOrderModel CODPlaceOrder_V1(String CustomerId, decimal PaidAmount, string AddressId, string Quantity, string buywith, string IsFromApp, string discountamount = "", string Redeemeamount = "", string couponCode = "", string refrcode = "")
        {
            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start ");
            CODOrderModel objCODplaceorder = new CODOrderModel();
            try
            {
                //IsFromApp=1 From Web=0


                #region Avoid multiple Entry
                bool IsOrderGeneratedInLastFiveMinutes = false;
                try
                {

                    DataTable dtLastOrder = dbCon.GetDataTable("SELECT top 1 DATEDIFF(mi,[CreatedOnUtc],'" + dbCon.getindiantime().ToString("dd-MMM-yyyy hh:mm:ss tt") + "'),Id,CustOfferCode FROM [dbo].[Order] where CustomerId=" + CustomerId + " order by id desc");
                    if (dtLastOrder != null && dtLastOrder.Rows.Count > 0)
                    {
                        //Order Place After 3 min
                        if (Convert.ToInt32(dtLastOrder.Rows[0][0].ToString()) < 5)
                        {
                            IsOrderGeneratedInLastFiveMinutes = true;
                            objCODplaceorder.resultflag = "1";
                            objCODplaceorder.OrderId = dtLastOrder.Rows[0]["Id"].ToString();
                            objCODplaceorder.Message = "Your OrderNo:" + dtLastOrder.Rows[0]["Id"].ToString() + " has been succesfully generated";
                            objCODplaceorder.Ccode = couponCode;
                        }
                    }
                }
                catch (Exception E) { }
                #endregion

                if (!IsOrderGeneratedInLastFiveMinutes)
                {
                    string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + CustomerId;
                    if (PaidAmount > 0)
                    {
                        int result = dbCon.CreateTransaction(merchantTxnId, CustomerId, PaidAmount);
                        if (result == 1)
                        {
                            string chektrans = "select id from AlterNetOrder where trnid=@1";
                            string[] param = { merchantTxnId };
                            DataTable dtcheckTrans = dbCon.GetDataTableWithParams(chektrans, param);
                            if (dtcheckTrans != null && dtcheckTrans.Rows.Count > 0)
                            {
                                dbCon.deleteAlternateOrder(merchantTxnId);
                            }
                            else
                            {
                                int chkalternat = CreateAlternateOrder(CustomerId, merchantTxnId, AddressId, PaidAmount.ToString(), Quantity, buywith, discountamount, Redeemeamount, couponCode, refrcode);
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 14");
                                if (chkalternat > 0 && PaidAmount > 0)
                                {
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 15");
                                    int OrderId = CODPlaceOrder123456(merchantTxnId);
                                    if (OrderId > 0)
                                    {
                                        // if success, Update the transaction 
                                        #region Update Transaction
                                        try
                                        {
                                            String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction]= @2,[Statuse] = @3 ,[IsPaymentSuccess]=@4 WHERE [TxnId] =  '" + merchantTxnId + "'";
                                            string[] parms_new = { OrderId.ToString(), dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "3", "1" };
                                            int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);
                                            if (resvaltrn > 0)
                                            {
                                                String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + merchantTxnId + "'";
                                                string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                                if (resvaltrn1 > 0)
                                                {
                                                    String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + merchantTxnId + "'";
                                                    string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                    int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                    if (resvaltrn2 > 0)
                                                    {
                                                        objCODplaceorder.resultflag = "1";
                                                        objCODplaceorder.OrderId = OrderId.ToString();
                                                        objCODplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                        objCODplaceorder.Ccode = couponCode;
                                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 16");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                objCODplaceorder.resultflag = "0";
                                                objCODplaceorder.OrderId = OrderId.ToString();
                                                objCODplaceorder.Message = "Your Order was Not Generated";
                                                objCODplaceorder.Ccode = couponCode;
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            objCODplaceorder.resultflag = "0";
                                            objCODplaceorder.OrderId = OrderId.ToString();
                                            objCODplaceorder.Message = "Your Order was Not Generated";
                                            objCODplaceorder.Ccode = couponCode;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        objCODplaceorder.resultflag = "0";
                                        objCODplaceorder.OrderId = OrderId.ToString();
                                        objCODplaceorder.Message = "Your Order was Not Generated";
                                        objCODplaceorder.Ccode = couponCode;
                                    }
                                }
                                else
                                {
                                    objCODplaceorder.resultflag = "0";
                                    objCODplaceorder.Message = "Your Order was Not Generated";
                                    objCODplaceorder.Ccode = couponCode;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objCODplaceorder;
        }

        [HttpGet]
        public CouponValidation CouponValidation(String couponcode)
        {
            Logger.InsertLogsApp("CouponValidation start ");
            CouponValidation c = new CouponValidation();
            try
            {
                DataTable dtCouponFor = dbCon.GetDataTable("select OrderTotal, BuyWith from[order] where CustOfferCode = '" + couponcode + "'");
                if (dtCouponFor != null && dtCouponFor.Rows.Count > 0)
                {
                    //Order Place After 3 min
                    if (dtCouponFor.Rows[0]["BuyWith"] != null)
                    {
                        c.BuyWith = Convert.ToInt32(dtCouponFor.Rows[0]["BuyWith"]);
                        c.OrderTotal = Convert.ToString(dtCouponFor.Rows[0]["OrderTotal"]);

                        Logger.InsertLogsApp("CouponValidation Processed Coupon " + couponcode + " :" + c.BuyWith.ToString() + "  " + c.OrderTotal.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, "", 0, false, "", ex.StackTrace);
                c.OrderTotal = "0";
                c.BuyWith = 0;
            }
            return c;
        }

        [HttpGet]
        public NewCouponCode GenerateCouponCode()
        {
            //START 02-01-2020 CODE MODIFIED FOR GENERATING COOUPONCODE
            NewCouponCode c = new NewCouponCode();
            try
            {
                Logger.InsertLogsApp("GenerateCouponCode start ");

            NewCode:
                c.couponcode = dbCon.GenerateRandomNumber().ToString();
                int test = getcheck(c.couponcode);

                if (test == 0)
                {
                    goto NewCode;
                }
                else
                {
                    Logger.InsertLogsApp("GenerateCouponCode Coupon Generated : " + c.couponcode);
                    return c;
                }
            }
            catch (Exception ex)
            {
                Logger.InsertLogsApp("GenerateCouponCode Coupon Could Not Be Generated : " + ex.StackTrace);
                return c;
            }

            //END 02-01-2020 CODE MODIFIED FOR GENERATING COOUPONCODE
        }

        public int CreateAlternateOrder(string Customerid, string transid, string Address, string paidAmount, string Quantity, string buywith, string discountamount = "", string Redemeamount = "", string coupanCode = "", string refcode = "", string ProductId = "")
        {
            try
            {
                Logger.InsertLogsApp("Pramaeters : Customerid=" + Customerid + " transid=" + transid + " Address=" + Address + " paidAmount=" + paidAmount + " Quantity=" + Quantity + " buywith=" + buywith + " discountamount=" + discountamount + " Redemeamount=" + Redemeamount + " coupanCode=" + coupanCode + " refcode=" + refcode);
                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 1");
                if (discountamount == "")
                { discountamount = "0"; }
                if (Redemeamount == "")
                { Redemeamount = "0"; }

                if (coupanCode == "")
                { coupanCode = "0"; }
                if (coupanCode == null)
                {
                    coupanCode = "0";
                }

                if (refcode == "")
                { refcode = "0"; }
                if (refcode == null)
                {
                    refcode = "0";
                }

                decimal totalamount = 0;
                decimal totalgram = 0;
                decimal totaloffer = 0;
                decimal totalsaving = 0;
                decimal totalquantity = 0;
                decimal shiprate = 0;
                decimal newprice = 0, NewTotalAmount = 0;

                string startdate = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");
                DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
                string qrypncd = "select Pincode from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;
                DataTable dtpncd = dbCon.GetDataTable(qrypncd);
                int pincode = int.Parse(dtpncd.Rows[0]["Pincode"].ToString());
                foreach (DataRow dr in ShipperList.Rows)
                {
                    int ShipperId = int.Parse(dr["Id"].ToString());
                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability : step 2");
                    var value = dbCon.CheckAvailability(pincode, ShipperId);

                    if (value > -1)
                    {
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability start : step 3");
                        //string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "' and id=1045";
                        string querystr = "";
                        if (ProductId != null && ProductId != "")
                        {
                            querystr = "select * from Product Where Id=" + ProductId;
                        }
                        else
                        {

                            querystr = "select * from Product Where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                        }

                        DataTable dtmain = dbCon.GetDataTable(querystr);

                        if (dtmain != null && dtmain.Rows.Count > 0)
                        {
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 4");
                            int productid = Convert.ToInt32(dtmain.Rows[0]["Id"]); //Null Not check
                            decimal offer = 0, price = 0, buywithprice = 0;
                            offer = Convert.ToDecimal(dtmain.Rows[0]["Offer"]); //Null Not check
                            string productname = dtmain.Rows[0]["Name"].ToString(); //Null Not check
                            int gram = 0;
                            int quantity = Convert.ToInt32(Quantity);
                            price = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 4.5");
                            decimal finalamount = 0, newfinalamount = 0;
                            finalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;

                            if (buywith == "1")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;
                                refcode = "";
                                coupanCode = "";

                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 5");
                            }
                            else if (buywith == "2")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]) * quantity;
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 6");
                            }
                            else if (buywith == "6")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["BuyWith5FriendExtraDiscount"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["BuyWith5FriendExtraDiscount"]) * quantity;
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 7");
                            }

                            decimal PricePerUnit = Math.Round(newfinalamount / quantity);
                            decimal FixedPrice = Convert.ToDecimal(dtmain.Rows[0]["FixedShipRate"]);

                            totaloffer += offer * quantity;
                            if (totaloffer > 0)
                            {
                                newprice = newfinalamount - offer;
                            }
                            else
                            {
                                newprice = newfinalamount;
                            }
                            gram = Convert.ToInt32(dtmain.Rows[0]["Unit"].ToString());
                            totalgram += (gram * quantity);
                            shiprate = Convert.ToDecimal(dtmain.Rows[0]["Offer"].ToString());
                            if (dtmain.Rows[0]["FixedShipRate"].ToString() != null && shiprate > 0)
                                NewTotalAmount = newprice + shiprate;
                            else
                                NewTotalAmount = newprice;
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 8");
                            //vaishnavi Changes
                            totalamount = Math.Round(NewTotalAmount);

                            string[] param = { Address };
                            DataTable dtAddress = dbCon.GetDataTableWithParams("select [CustomerId],[FirstName],[LastName],[CountryId],[StateId],[CityId],[Address],[MobileNo],[PinCode] from [CustomerAddress] where [IsDeleted]=0 and [IsActive]=1 and Id=@1", param);
                            if (dtAddress != null && dtAddress.Rows.Count > 0)
                            {
                                if (paidAmount == "")
                                {
                                    paidAmount = "0";
                                }
                                if (coupanCode == null)
                                {
                                    coupanCode = "";
                                }
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 9");
                                string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                                string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, finalamount.ToString(), newfinalamount.ToString(), "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", quantity.ToString(), paidAmount, totalgram.ToString(), totalsaving.ToString(), Redemeamount, transid, "0", "3", coupanCode, refcode, "7", buywith };
                                int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 10 - " + Orderrslt.ToString());

                                if (Orderrslt > 0)
                                {
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 11");
                                    string gst = "select [TaxValue] from [GstTaxCategory] where Id=" + dtmain.Rows[0]["GSTTaxId"].ToString();
                                    DataTable dtgstv = dbCon.GetDataTable(gst);
                                    string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE());Select SCOPE_IDENTITY()";
                                    string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), paidAmount, productname, buywith.ToString(), buywithprice.ToString() };
                                    int result11 = dbCon.ExecuteScalarQueryWithParams(insertquery1, param11);
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 12 " + result11.ToString());
                                    try
                                    {
                                        string nameqry = "select  CustomerAddress.FirstName AS CUSNAMNE  from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;

                                        // string nameqry = "SELECT OrderItem.ProductId, CONCAT(Customer.FirstName,' ', Customer.LastName)AS CUSNAMNE FROM  Customer INNER JOIN [Order] ON Customer.Id = [Order].CustomerId INNER JOIN OrderItem ON [Order].Id = OrderItem.OrderId INNER JOIN Product ON OrderItem.ProductId = Product.Id WHERE [Order].Id = " + Orderrslt;
                                        DataTable dtnms = dbCon.GetDataTable(nameqry);
                                        string name = "";
                                        if (dtnms != null && dtnms.Rows.Count > 0)
                                        {
                                            name = dtnms.Rows[0]["CUSNAMNE"].ToString() + " just bought! ";
                                        }

                                        string final1 = "";

                                        string qry = "select  Product.Id ,Product.sold from Product where Product.Id=" + productid;
                                        DataTable dt = dbCon.GetDataTable(qry);
                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            string[] sen = dt.Rows[0]["sold"].ToString().Split(' ');
                                            int last = int.Parse(sen[1].ToString());
                                            int final = last + 1;
                                            final1 = sen[0] + " " + final.ToString() + " " + sen[2];


                                        }
                                        string updt = "UPDATE [dbo].[Product] SET [sold] ='" + final1 + "',JustBougth='" + name + "' WHERE Product.Id=" + productid;
                                        int ordersoldupdt = dbCon.ExecuteQuery(updt);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 13");
                                    return result11;

                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        return 0;

                    }

                }


            }
            catch
            {
                return 0;
            }
            return 0;
        }

        private int CODPlaceOrder123456(String txnId)
        {
            DataTable Get_Transactiontbl = new DataTable();
            string amount = "0";
            int OrderId = 0;
            GenerateOrder create_order = new GenerateOrder();
            OrderId = create_order.CreateOrderFromAlternetOrder(txnId, amount);
            if (OrderId > 0)
            {
                return OrderId;
            }
            else
            {
                return 0;
            }
        }

        private int CODPlaceOrderForMultiple(String txnId)
        {
            DataTable Get_Transactiontbl = new DataTable();
            string amount = "0";
            int OrderId = 0;
            GenerateOrder create_order = new GenerateOrder();
            OrderId = create_order.CreateOrderFromAlternetOrderForMultiple(txnId, amount);
            if (OrderId > 0)
            {
                return OrderId;
            }
            else
            {
                return 0;
            }
        }

        public int getcheck(string code)
        {
            try
            {
                dbConnection dbc = new dbConnection();
                string querydata = "select top 1 Id from [order] where CustOfferCode='" + code + "'";
                DataTable dtcheck = dbc.GetDataTable(querydata);
                if (dtcheck.Rows.Count > 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            }
            catch (Exception ee)
            {
                return 0;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        //START 24-04-2020 : ADDED CODE FOR CHECKING PIN CODE
        private Models.CheckPincode.CheckPincodeModel CheckPincode(Int32 orderid, Int32 customerid)
        {
            Models.CheckPincode.CheckPincodeModel objchkpincode = new Models.CheckPincode.CheckPincodeModel();
            DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
            //int pincode = 0;
            try
            {
                string query = "select top 1 PinCode from CustomerAddress a inner join[order] o on o.CustomerId = a.CustomerId where o.CustomerId = " + customerid + " and o.Id = " + orderid;
                var data = dbCon.ExecuteSQLScaler(query);
                if (data != null && data.ToString() != "" && data.ToString().Length == 6)
                {
                    foreach (DataRow dr in ShipperList.Rows)
                    {
                        int ShipperId = int.Parse(dr["Id"].ToString());
                        int pincode = int.Parse(data.ToString());

                        var value = dbCon.CheckAvailability(pincode, ShipperId);

                        if (value == 0)
                        {
                            objchkpincode.resultflag = "0";
                            objchkpincode.Message = "Delivery is not available in this area.";
                        }
                        else
                        {
                            objchkpincode.resultflag = "1";
                            objchkpincode.Message = "Service available at " + pincode;
                        }
                    }
                }
                else
                {
                    objchkpincode.resultflag = "0";
                    objchkpincode.Message = "Enter Valid Pincode";

                }

            }
            catch (Exception ex)
            {
                objchkpincode.resultflag = "0";
                objchkpincode.Message = "Enter Valid Pincode";
            }
            return objchkpincode;
        }
        //END 24-04-2020 : ADDED CODE FOR CHECKING PIN CODE



        [HttpPost]
        public CODOrderModel CODPlaceMultipleOrder(PlaceMultipleOrderModel model)
        {
            //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            //List<PlaceMultipleOrderModel> model =
            //       (List<PlaceMultipleOrderModel>)json_serializer.DeserializeObject(jsonstring);

            //List<PlaceMultipleOrderModel> model = JsonConvert.DeserializeObject<List<PlaceMultipleOrderModel>> (jsonstring);

            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start ");

            CODOrderModel objCODplaceorder = new CODOrderModel();
            try
            {

                string ccode = "";
                //foreach (var item in model)
                //{
                string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + model.CustomerId;
                if (!string.IsNullOrEmpty(model.totalAmount) && model.totalAmount != "0")
                {
                    var amount = Convert.ToDecimal(model.totalAmount);
                    int result = dbCon.CreateTransaction(merchantTxnId, model.CustomerId, amount);
                    if (result == 1)
                    {
                        string chektrans = "select id from AlterNetOrder where trnid=@1";
                        string[] param = { merchantTxnId };
                        DataTable dtcheckTrans = dbCon.GetDataTableWithParams(chektrans, param);
                        if (dtcheckTrans != null && dtcheckTrans.Rows.Count > 0)
                        {
                            dbCon.deleteAlternateOrder(merchantTxnId);
                        }
                        else
                        {
                            int chkalternat = CreateMultipleAlternateOrder(model.CustomerId, merchantTxnId, model.AddressId, model.products, model.orderMRP, model.totalAmount, model.totalQty, model.totalWeight,out ccode, model.discountamount, model.Redeemeamount);
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 14");
                            if (chkalternat > 0 && !string.IsNullOrEmpty(model.totalAmount) && model.totalAmount != "0")
                            {
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 15");
                                int OrderId = CODPlaceOrderForMultiple(merchantTxnId);
                                if (OrderId > 0)
                                {
                                    //START 20-02-2020 - Added Code To Track Source
                                    try
                                    {
                                        string values = string.Empty;
                                        Logger.InsertLogsApp("Inserting Source Of Order");
                                        if (Request.Headers.GetValues("DeviceType").First() != null)
                                        {
                                            Logger.InsertLogsApp(Request.Headers.GetValues("DeviceType").First() + " - Device Order Received From");
                                            values = Request.Headers.GetValues("DeviceType").First();
                                            if (values != null)
                                            {
                                                string[] insert = { OrderId.ToString(), values };
                                                string insertdeviceidentity = "INSERT INTO [dbo].[Order_Source]([OrderID],[OrderSourceName]) VALUES (@1,@2);";
                                                int sourceid = dbCon.ExecuteScalarQueryWithParams(insertdeviceidentity, insert);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, ex.Message);
                                    }
                                    //END 20-02-2020 - Added Code To Track Source




                                    // if success, Update the transaction 
                                    #region Update Transaction
                                    try
                                    {
                                        String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction]= @2,[Statuse] = @3 ,[IsPaymentSuccess]=@4 WHERE [TxnId] =  '" + merchantTxnId + "'";
                                        string[] parms_new = { OrderId.ToString(), dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "3", "1" };
                                        int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);
                                        if (resvaltrn > 0)
                                        {
                                            String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + merchantTxnId + "'";
                                            string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                            int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                            if (resvaltrn1 > 0)
                                            {
                                                String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + merchantTxnId + "'";
                                                string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                if (resvaltrn2 > 0)
                                                {
                                                    objCODplaceorder.resultflag = "1";
                                                    objCODplaceorder.OrderId = OrderId.ToString();
                                                    objCODplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                    objCODplaceorder.Ccode = ccode;
                                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 16");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            objCODplaceorder.resultflag = "0";
                                            objCODplaceorder.OrderId = OrderId.ToString();
                                            objCODplaceorder.Message = "Your Order was Not Generated";
                                            objCODplaceorder.Ccode = ccode;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        objCODplaceorder.resultflag = "0";
                                        objCODplaceorder.OrderId = OrderId.ToString();
                                        objCODplaceorder.Message = "Your Order was Not Generated";
                                        objCODplaceorder.Ccode = ccode;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    objCODplaceorder.resultflag = "0";
                                    objCODplaceorder.OrderId = OrderId.ToString();
                                    objCODplaceorder.Message = "Your Order was Not Generated";
                                    objCODplaceorder.Ccode = ccode;
                                }
                            }
                            else
                            {
                                objCODplaceorder.resultflag = "0";
                                objCODplaceorder.Message = "Your Order was Not Generated";
                                objCODplaceorder.Ccode = ccode;
                            }
                        }
                    }
                }

                //}
            }
            catch (Exception ex)
            {

            }
            return objCODplaceorder;
        }

        public int CreateMultipleAlternateOrder(string Customerid, string transid, string Address, List<ProductList> products, string orderMRP, string totalAmount, string totalQty, string totalWeight, out string ccode , string discountamount = "", string Redemeamount = "")
        {
            var jsonstring = JsonConvert.SerializeObject(products);
            ccode = products.Where(m => m.couponCode != string.Empty && m.couponCode != null && m.couponCode != "0").FirstOrDefault().couponCode;
            try
            {
                Logger.InsertLogsApp("Pramaeters : Customerid=" + Customerid + " transid=" + transid + " Address=" + Address + " products=" + jsonstring + " orderMRP=" + orderMRP + " totalAmount=" + totalAmount + " totalQty=" + totalQty + " totalWeight=" + totalWeight + " discountamount=" + discountamount + " Redemeamount=" + Redemeamount);
                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 1");
                if (discountamount == "")
                { discountamount = "0"; }
                if (Redemeamount == "")
                { Redemeamount = "0"; }

                //if (coupanCode == "")
                //{ coupanCode = "0"; }
                //if (coupanCode == null)
                //{
                //    coupanCode = "0";
                //}

                //if (refcode == "")
                //{ refcode = "0"; }
                //if (refcode == null)
                //{
                //    refcode = "0";
                //}

                decimal totalamount = 0;
                decimal totalgram = 0;
                decimal totaloffer = 0;
                decimal totalsaving = 0;
                decimal totalquantity = 0;
                decimal shiprate = 0;
                decimal newprice = 0, NewTotalAmount = 0;
                int result11 = 0;

                string startdate = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");
                DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
                string qrypncd = "select Pincode from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;
                DataTable dtpncd = dbCon.GetDataTable(qrypncd);
                int pincode = int.Parse(dtpncd.Rows[0]["Pincode"].ToString());
                foreach (DataRow dr in ShipperList.Rows)
                {
                    int ShipperId = int.Parse(dr["Id"].ToString());
                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability : step 2");
                    var value = dbCon.CheckAvailability(pincode, ShipperId);

                    if (value > -1)
                    {
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability start : step 3");
                        //string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "' and id=1045";
                        string querystr = "";

                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder  : step 4");
                        string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                        string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, orderMRP, totalAmount, "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", totalQty, "0", totalWeight, totalsaving.ToString(), Redemeamount, transid, "0", "3", "0", "0", "7", "0" };
                        int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder  : step 5 - " + Orderrslt.ToString());

                        foreach (var item in products)
                        {

                            if (item.productid != null && item.productid != "")
                            {
                                querystr = "select * from Product Where Id=" + item.productid;
                            }
                            else
                            {

                                querystr = "select * from Product Where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                            }

                            DataTable dtmain = dbCon.GetDataTable(querystr);

                            if (dtmain != null && dtmain.Rows.Count > 0)
                            {
                                for (int i = 0; i < dtmain.Rows.Count; i++)
                                {


                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 6");
                                    int productid = Convert.ToInt32(dtmain.Rows[i]["Id"]); //Null Not check
                                    decimal offer = 0, price = 0, buywithprice = 0;
                                    offer = Convert.ToDecimal(dtmain.Rows[i]["Offer"]); //Null Not check
                                    string productname = dtmain.Rows[i]["Name"].ToString(); //Null Not check
                                    int gram = 0;
                                    int quantity = Convert.ToInt32(item.Quantity);
                                    price = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]);
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 6.5");
                                    decimal finalamount = 0, newfinalamount = 0;
                                    finalamount = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]) * quantity;

                                    if (item.buywith == "1")
                                    {
                                        buywithprice = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]);
                                        newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]) * quantity;
                                        item.refrcode = "";
                                        item.couponCode = "";

                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 7");
                                    }
                                    else if (item.buywith == "2")
                                    {
                                        buywithprice = Convert.ToDecimal(dtmain.Rows[i]["BuyWith1FriendExtraDiscount"]);
                                        newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["BuyWith1FriendExtraDiscount"]) * quantity;
                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 8");
                                    }
                                    else if (item.buywith == "6")
                                    {
                                        buywithprice = Convert.ToDecimal(dtmain.Rows[i]["BuyWith5FriendExtraDiscount"]);
                                        newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["BuyWith5FriendExtraDiscount"]) * quantity;
                                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 9");
                                    }

                                    decimal PricePerUnit = Math.Round(newfinalamount / quantity);
                                    decimal FixedPrice = Convert.ToDecimal(dtmain.Rows[i]["FixedShipRate"]);

                                    totaloffer += offer * quantity;
                                    if (totaloffer > 0)
                                    {
                                        newprice = newfinalamount - offer;
                                    }
                                    else
                                    {
                                        newprice = newfinalamount;
                                    }
                                    gram = Convert.ToInt32(dtmain.Rows[i]["Unit"].ToString());
                                    totalgram += (gram * quantity);
                                    shiprate = Convert.ToDecimal(dtmain.Rows[i]["Offer"].ToString());
                                    if (dtmain.Rows[i]["FixedShipRate"].ToString() != null && shiprate > 0)
                                        NewTotalAmount = newprice + shiprate;
                                    else
                                        NewTotalAmount = newprice;
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 10");
                                    //vaishnavi Changes
                                    totalamount = Math.Round(NewTotalAmount);

                                    string[] param = { Address };
                                    DataTable dtAddress = dbCon.GetDataTableWithParams("select [CustomerId],[FirstName],[LastName],[CountryId],[StateId],[CityId],[Address],[MobileNo],[PinCode] from [CustomerAddress] where [IsDeleted]=0 and [IsActive]=1 and Id=@1", param);
                                    if (dtAddress != null && dtAddress.Rows.Count > 0)
                                    {
                                        //if (item.pa == "")
                                        //{
                                        //    paidAmount = "0";
                                        //}
                                        if (item.couponCode == null)
                                        {
                                            item.couponCode = "";
                                        }
                                        //Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 9");
                                        //string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                                        //string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, finalamount.ToString(), newfinalamount.ToString(), "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", quantity.ToString(), paidAmount, totalgram.ToString(), totalsaving.ToString(), Redemeamount, transid, "0", "3", coupanCode, refcode, "7", buywith };
                                        //int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                                        //Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 10 - " + Orderrslt.ToString());

                                        if (Orderrslt > 0)
                                        {
                                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 11");
                                            string gst = "select [TaxValue] from [GstTaxCategory] where Id=" + dtmain.Rows[i]["GSTTaxId"].ToString();
                                            DataTable dtgstv = dbCon.GetDataTable(gst);
                                            string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc],[CustOfferCode],[RefferedOfferCode]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE(),@18,@19);Select SCOPE_IDENTITY()";
                                            string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), item.PaidAmount.ToString(), productname, item.buywith, buywithprice.ToString(), item.couponCode, item.refrcode };
                                            result11 = dbCon.ExecuteScalarQueryWithParams(insertquery1, param11);
                                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 12 " + result11.ToString());
                                            try
                                            {
                                                string nameqry = "select  CustomerAddress.FirstName AS CUSNAMNE  from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;

                                                // string nameqry = "SELECT OrderItem.ProductId, CONCAT(Customer.FirstName,' ', Customer.LastName)AS CUSNAMNE FROM  Customer INNER JOIN [Order] ON Customer.Id = [Order].CustomerId INNER JOIN OrderItem ON [Order].Id = OrderItem.OrderId INNER JOIN Product ON OrderItem.ProductId = Product.Id WHERE [Order].Id = " + Orderrslt;
                                                DataTable dtnms = dbCon.GetDataTable(nameqry);
                                                string name = "";
                                                if (dtnms != null && dtnms.Rows.Count > 0)
                                                {
                                                    name = dtnms.Rows[0]["CUSNAMNE"].ToString() + " just bought! ";
                                                }

                                                string final1 = "";

                                                string qry = "select  Product.Id ,Product.sold from Product where Product.Id=" + productid;
                                                DataTable dt = dbCon.GetDataTable(qry);
                                                if (dt != null && dt.Rows.Count > 0)
                                                {
                                                    string[] sen = dt.Rows[0]["sold"].ToString().Split(' ');
                                                    int last = int.Parse(sen[1].ToString());
                                                    int final = last + 1;
                                                    final1 = sen[0] + " " + final.ToString() + " " + sen[2];


                                                }
                                                string updt = "UPDATE [dbo].[Product] SET [sold] ='" + final1 + "',JustBougth='" + name + "' WHERE Product.Id=" + productid;
                                                int ordersoldupdt = dbCon.ExecuteQuery(updt);
                                            }
                                            catch (Exception)
                                            {
                                            }



                                        }
                                        else
                                        {
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 13");
                        return result11;
                    }
                    else
                    {
                        return 0;

                    }

                }


            }
            catch
            {
                return 0;
            }
            return 0;
        }

        [HttpGet]
        public List<CouponValidation> CouponValidationForMultipleProduct(String couponcode)
        {
            Logger.InsertLogsApp("CouponValidation start ");
            List<CouponValidation> c = new List<CouponValidation>();
            try
            {
                DataTable dtCouponFor = dbCon.GetDataTable("select [Order].OrderTotal, [OrderItem].BuyWith,[OrderItem].BuyWithPerUnit,[OrderItem].ProductId FROM [Order] Inner join OrderItem on OrderItem.OrderId=[Order].Id where [OrderItem].CustOfferCode = '" + couponcode + "'");
                if (dtCouponFor != null && dtCouponFor.Rows.Count > 0)
                {
                    ////Order Place After 3 min
                    //if (dtCouponFor.Rows[0]["BuyWith"] != null)
                    //{
                    //    c.BuyWith = Convert.ToInt32(dtCouponFor.Rows[0]["BuyWith"]);
                    //    c.OrderTotal = Convert.ToString(dtCouponFor.Rows[0]["OrderTotal"]);

                    //    Logger.InsertLogsApp("CouponValidation Processed Coupon " + couponcode + " :" + c.BuyWith.ToString() + "  " + c.OrderTotal.ToString());

                    //}
                    for (int i = 0; i < dtCouponFor.Rows.Count; i++)
                    {

                        c.Add(new CouponValidation
                        {
                            BuyWith = Convert.ToInt32(dtCouponFor.Rows[i]["BuyWith"]),
                            OrderTotal = Convert.ToString(dtCouponFor.Rows[i]["BuyWithPerUnit"]),
                            ProductId = Convert.ToInt32(dtCouponFor.Rows[i]["ProductId"])
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, "", 0, false, "", ex.StackTrace);
                c.Add(new CouponValidation
                {
                    BuyWith = 0,
                    OrderTotal = string.Empty,
                    ProductId = 0
                });
            }
            return c;
        }

        [HttpPost]
        public CODOrderModel CODPlaceMultipleOrderNew(PlaceMultipleOrderNewModel model)
        {
            
            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start ");

            CODOrderModel objCODplaceorder = new CODOrderModel();
            try
            {

                string ccode = "";
                //foreach (var item in model)
                //{
                string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + model.CustomerId;
                if (!string.IsNullOrEmpty(model.totalAmount) && model.totalAmount != "0")
                {
                    var amount = Convert.ToDecimal(model.totalAmount);
                    int result = dbCon.CreateTransaction(merchantTxnId, model.CustomerId, amount);
                    if (result == 1)
                    {
                        string chektrans = "select id from AlterNetOrder where trnid=@1";
                        string[] param = { merchantTxnId };
                        DataTable dtcheckTrans = dbCon.GetDataTableWithParams(chektrans, param);
                        if (dtcheckTrans != null && dtcheckTrans.Rows.Count > 0)
                        {
                            dbCon.deleteAlternateOrder(merchantTxnId);
                        }
                        else
                        {
                            int chkalternat = CreateMultipleAlternateOrderNew(model.CustomerId, merchantTxnId, model.AddressId, model.products, model.orderMRP, model.totalAmount, model.totalQty, model.totalWeight, out ccode, model.discountamount, model.Redeemeamount);
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 14");
                            if (chkalternat > 0 && !string.IsNullOrEmpty(model.totalAmount) && model.totalAmount != "0")
                            {
                                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 15");
                                int OrderId = CODPlaceOrderForMultiple(merchantTxnId);
                                if (OrderId > 0)
                                {
                                    //START 20-02-2020 - Added Code To Track Source
                                    try
                                    {
                                        string values = string.Empty;
                                        Logger.InsertLogsApp("Inserting Source Of Order");
                                        if (Request.Headers.GetValues("DeviceType").First() != null)
                                        {
                                            Logger.InsertLogsApp(Request.Headers.GetValues("DeviceType").First() + " - Device Order Received From");
                                            values = Request.Headers.GetValues("DeviceType").First();
                                            if (values != null)
                                            {
                                                string[] insert = { OrderId.ToString(), values };
                                                string insertdeviceidentity = "INSERT INTO [dbo].[Order_Source]([OrderID],[OrderSourceName]) VALUES (@1,@2);";
                                                int sourceid = dbCon.ExecuteScalarQueryWithParams(insertdeviceidentity, insert);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, ex.Message);
                                    }
                                    //END 20-02-2020 - Added Code To Track Source




                                    // if success, Update the transaction 
                                    #region Update Transaction
                                    try
                                    {
                                        String querynew = "UPDATE [dbo].[CitrusPayment] SET [OrderId] = @1 ,[Order_TimeOfTransaction]= @2,[Statuse] = @3 ,[IsPaymentSuccess]=@4 WHERE [TxnId] =  '" + merchantTxnId + "'";
                                        string[] parms_new = { OrderId.ToString(), dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "3", "1" };
                                        int resvaltrn = dbCon.ExecuteQueryWithParams(querynew, parms_new);
                                        if (resvaltrn > 0)
                                        {
                                            String querynew1 = "UPDATE [dbo].[AlternetOrder] SET [UpdatedOnUtc]=@1,[IsPaymentDone]=@2 WHERE [TrnId] =  '" + merchantTxnId + "'";
                                            string[] parms_new1 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                            int resvaltrn1 = dbCon.ExecuteQueryWithParams(querynew1, parms_new1);
                                            if (resvaltrn1 > 0)
                                            {
                                                String querynew2 = "UPDATE [dbo].[Order] SET [UpdatedOnUtc] =@1,[IsPaymentDone]=@2 WHERE [TRNID] ='" + merchantTxnId + "'";
                                                string[] parms_new2 = { dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss"), "1" };

                                                int resvaltrn2 = dbCon.ExecuteQueryWithParams(querynew2, parms_new2);
                                                if (resvaltrn2 > 0)
                                                {
                                                    objCODplaceorder.resultflag = "1";
                                                    objCODplaceorder.OrderId = OrderId.ToString();
                                                    objCODplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                    objCODplaceorder.Ccode = ccode;
                                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 16");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            objCODplaceorder.resultflag = "0";
                                            objCODplaceorder.OrderId = OrderId.ToString();
                                            objCODplaceorder.Message = "Your Order was Not Generated";
                                            objCODplaceorder.Ccode = ccode;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        objCODplaceorder.resultflag = "0";
                                        objCODplaceorder.OrderId = OrderId.ToString();
                                        objCODplaceorder.Message = "Your Order was Not Generated";
                                        objCODplaceorder.Ccode = ccode;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    objCODplaceorder.resultflag = "0";
                                    objCODplaceorder.OrderId = OrderId.ToString();
                                    objCODplaceorder.Message = "Your Order was Not Generated";
                                    objCODplaceorder.Ccode = ccode;
                                }
                            }
                            else
                            {
                                objCODplaceorder.resultflag = "0";
                                objCODplaceorder.Message = "Your Order was Not Generated";
                                objCODplaceorder.Ccode = ccode;
                            }
                        }
                    }
                }

                //}
            }
            catch (Exception ex)
            {

            }
            return objCODplaceorder;
        }
        public int CreateMultipleAlternateOrderNew(string Customerid, string transid, string Address, List<ProductListNew> products, string orderMRP, string totalAmount, string totalQty, string totalWeight, out string ccode, string discountamount = "", string Redemeamount = "")
        {
            var jsonstring = JsonConvert.SerializeObject(products);
            //ccode = products.Where(m => m.couponCode != string.Empty && m.couponCode != null && m.couponCode != "0").FirstOrDefault().couponCode;
            ccode = "0";
            try
            {
                Logger.InsertLogsApp("Pramaeters : Customerid=" + Customerid + " transid=" + transid + " Address=" + Address + " products=" + jsonstring + " orderMRP=" + orderMRP + " totalAmount=" + totalAmount + " totalQty=" + totalQty + " totalWeight=" + totalWeight + " discountamount=" + discountamount + " Redemeamount=" + Redemeamount);
                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 1");
                if (discountamount == "")
                { discountamount = "0"; }
                if (Redemeamount == "")
                { Redemeamount = "0"; }

              
                decimal totalamount = 0;
                decimal totalgram = 0;
                decimal totaloffer = 0;
                decimal totalsaving = 0;
                decimal totalquantity = 0;
                decimal shiprate = 0;
                decimal newprice = 0, NewTotalAmount = 0;
                int result11 = 0;

                string startdate = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");
                DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
                string qrypncd = "select Pincode from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;
                DataTable dtpncd = dbCon.GetDataTable(qrypncd);
                int pincode = int.Parse(dtpncd.Rows[0]["Pincode"].ToString());
                foreach (DataRow dr in ShipperList.Rows)
                {
                    int ShipperId = int.Parse(dr["Id"].ToString());
                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability : step 2");
                    var value = dbCon.CheckAvailability(pincode, ShipperId);

                    if (value > -1)
                    {
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability start : step 3");
                        //string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "' and id=1045";
                        string querystr = "";

                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder  : step 4");
                        string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                        string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, orderMRP, totalAmount, "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", totalQty, "0", totalWeight, totalsaving.ToString(), Redemeamount, transid, "0", "3", "0", "0", "7", "0" };
                        int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder  : step 5 - " + Orderrslt.ToString());

                        foreach (var item in products)
                        {

                            if (item.productid != null && item.productid != "")
                            {
                                querystr = "select * from Product Where Id=" + item.productid;
                            }
                            else
                            {

                                querystr = "select * from Product Where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                            }

                            DataTable dtmain = dbCon.GetDataTable(querystr);

                            if (dtmain != null && dtmain.Rows.Count > 0)
                            {
                                for (int i = 0; i < dtmain.Rows.Count; i++)
                                {


                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 6");
                                    int productid = Convert.ToInt32(dtmain.Rows[i]["Id"]); //Null Not check
                                    decimal offer = 0, price = 0, buywithprice = 0;
                                    offer = Convert.ToDecimal(dtmain.Rows[i]["Offer"]); //Null Not check
                                    string productname = dtmain.Rows[i]["Name"].ToString(); //Null Not check
                                    int gram = 0;
                                    int quantity = Convert.ToInt32(item.Quantity);

                                    string unitqry = "Select * From [UnitMaster] where UnitName ='" + item.UnitId + "'";
                                    DataTable dtunit = dbCon.GetDataTable(unitqry);
                                    int unitid = 0;
                                    if (dtunit != null && dtunit.Rows.Count > 0)
                                    {
                                        unitid = Convert.ToInt32(dtunit.Rows[0]["Id"]);
                                    }
                                    //int unitid = Convert.ToInt32(item.UnitId);

                                    string sUnit = item.Unit;
                                    price = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]);
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 6.5");
                                    decimal finalamount = 0, newfinalamount = 0;
                                    finalamount = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]) * quantity;
                                    newfinalamount = finalamount;
                                    //if (item.buywith == "1")
                                    //{
                                    //    buywithprice = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]);
                                    //    newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["Mrp"]) * quantity;
                                    //    item.refrcode = "";
                                    //    item.couponCode = "";

                                    //    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 7");
                                    //}
                                    //else if (item.buywith == "2")
                                    //{
                                    //    buywithprice = Convert.ToDecimal(dtmain.Rows[i]["BuyWith1FriendExtraDiscount"]);
                                    //    newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["BuyWith1FriendExtraDiscount"]) * quantity;
                                    //    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 8");
                                    //}
                                    //else if (item.buywith == "6")
                                    //{
                                    //    buywithprice = Convert.ToDecimal(dtmain.Rows[i]["BuyWith5FriendExtraDiscount"]);
                                    //    newfinalamount = Convert.ToDecimal(dtmain.Rows[i]["BuyWith5FriendExtraDiscount"]) * quantity;
                                    //    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 9");
                                    //}

                                    decimal PricePerUnit = Math.Round(newfinalamount / quantity);
                                    decimal FixedPrice = Convert.ToDecimal(dtmain.Rows[i]["FixedShipRate"]);

                                    totaloffer += offer * quantity;
                                    if (totaloffer > 0)
                                    {
                                        newprice = newfinalamount - offer;
                                    }
                                    else
                                    {
                                        newprice = newfinalamount;
                                    }
                                    //gram = Convert.ToInt32(dtmain.Rows[i]["Unit"].ToString());
                                    //totalgram += (gram * quantity);
                                    totalgram += (Convert.ToInt32(item.Unit) * quantity);
                                    shiprate = Convert.ToDecimal(dtmain.Rows[i]["Offer"].ToString());
                                    if (dtmain.Rows[i]["FixedShipRate"].ToString() != null && shiprate > 0)
                                        NewTotalAmount = newprice + shiprate;
                                    else
                                        NewTotalAmount = newprice;
                                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 10");
                                    //vaishnavi Changes
                                    totalamount = Math.Round(NewTotalAmount);

                                    string[] param = { Address };
                                    DataTable dtAddress = dbCon.GetDataTableWithParams("select [CustomerId],[FirstName],[LastName],[CountryId],[StateId],[CityId],[Address],[MobileNo],[PinCode] from [CustomerAddress] where [IsDeleted]=0 and [IsActive]=1 and Id=@1", param);
                                    if (dtAddress != null && dtAddress.Rows.Count > 0)
                                    {
                                        
                                        if (item.couponCode == null)
                                        {
                                            item.couponCode = "";
                                        }
                                        
                                        if (Orderrslt > 0)
                                        {
                                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 11");
                                            string gst = "select [TaxValue] from [GstTaxCategory] where Id=" + dtmain.Rows[i]["GSTTaxId"].ToString();
                                            DataTable dtgstv = dbCon.GetDataTable(gst);
                                            string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc],[CustOfferCode],[RefferedOfferCode],[UnitId],[Unit]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE(),@18,@19,@20,@21);Select SCOPE_IDENTITY()";
                                            //string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), item.PaidAmount.ToString(), productname, item.buywith, buywithprice.ToString(), item.couponCode, item.refrcode };
                                            string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), item.PaidAmount.ToString(), productname, "0", buywithprice.ToString(), item.couponCode, item.refrcode ,unitid.ToString(),sUnit.ToString()};
                                            result11 = dbCon.ExecuteScalarQueryWithParams(insertquery1, param11);
                                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 12 " + result11.ToString());
                                            try
                                            {
                                                string nameqry = "select  CustomerAddress.FirstName AS CUSNAMNE  from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;

                                                // string nameqry = "SELECT OrderItem.ProductId, CONCAT(Customer.FirstName,' ', Customer.LastName)AS CUSNAMNE FROM  Customer INNER JOIN [Order] ON Customer.Id = [Order].CustomerId INNER JOIN OrderItem ON [Order].Id = OrderItem.OrderId INNER JOIN Product ON OrderItem.ProductId = Product.Id WHERE [Order].Id = " + Orderrslt;
                                                DataTable dtnms = dbCon.GetDataTable(nameqry);
                                                string name = "";
                                                if (dtnms != null && dtnms.Rows.Count > 0)
                                                {
                                                    name = dtnms.Rows[0]["CUSNAMNE"].ToString() + " just bought! ";
                                                }

                                                string final1 = "";

                                                string qry = "select  Product.Id ,Product.sold from Product where Product.Id=" + productid;
                                                DataTable dt = dbCon.GetDataTable(qry);
                                                if (dt != null && dt.Rows.Count > 0)
                                                {
                                                    string[] sen = dt.Rows[0]["sold"].ToString().Split(' ');
                                                    int last = int.Parse(sen[1].ToString());
                                                    int final = last + 1;
                                                    final1 = sen[0] + " " + final.ToString() + " " + sen[2];


                                                }
                                                string updt = "UPDATE [dbo].[Product] SET [sold] ='" + final1 + "',JustBougth='" + name + "' WHERE Product.Id=" + productid;
                                                int ordersoldupdt = dbCon.ExecuteQuery(updt);
                                            }
                                            catch (Exception)
                                            {
                                            }



                                        }
                                        else
                                        {
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 13");
                        return result11;
                    }
                    else
                    {
                        return 0;

                    }

                }


            }
            catch
            {
                return 0;
            }
            return 0;
        }



    }
}
