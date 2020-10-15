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
                            int chkalternat = CreateMultipleAlternateOrderNew(model.CustomerId, merchantTxnId, model.AddressId, model.products, model.orderMRP, model.totalAmount, model.totalQty, model.totalWeight, out ccode, model.discountamount, model.Redeemeamount,model.JurisdictionID, model.PaidAmount, model.PromoCode,model.Cashbackamount, model.ReOrderId);
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
                                        decimal redeemeAmt = 0;
                                        decimal balanceAmt = 0;
                                        if (model.Redeemeamount.ToString() != "0" && model.Walletbalance.ToString() != "0" && model.WalletId != "0")
                                        {
                                            if(model.WalletType == "%")
                                            {
                                                redeemeAmt = Convert.ToDecimal(Convert.ToDecimal(model.totalAmount) * Convert.ToDecimal(model.WalletCrAmount) / 100);
                                            }
                                            else
                                            {
                                                redeemeAmt = Convert.ToDecimal(model.Redeemeamount);
                                            }
                                            balanceAmt = Convert.ToDecimal(model.Walletbalance) - redeemeAmt;
                                            string[] parm1 = { model.WalletId, model.CustomerId, OrderId.ToString(),model.WalletLinkId, model.WalletCrDate,model.WalletCrDescription,model.WalletCrAmount,
                                                                dbCon.getindiantime().ToString("dd-MMM-yyyy HH:mm:ss"), model.WalletCrDescription,
                                                redeemeAmt.ToString(),balanceAmt.ToString(),"1",
                                            dbCon.getindiantime().ToString("dd-MMM-yyyy HH:mm:ss"), model.CustomerId};

                                            string insertredeemewallet = "INSERT INTO [dbo].[tblWalletCustomerHistory]([wallet_id],[customer_id],[order_id]," +
                                                                          " [wallet_link_id],[Cr_date],[Cr_description],[Cr_amount],[Dr_date],[Dr_description], " +
                                                                          " [Dr_amount],[balance],[is_active],[created_date],[created_by]) VALUES (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14);";
                                            dbCon.ExecuteScalarQueryWithParams(insertredeemewallet, parm1);
                                        }
                                        if(model.Cashbackamount.ToString() != "0" && model.PromoCodeId != "0")
                                        {
                                            //decimal promocodeAmt = 0;
                                            //if (model.PromoCodetype == "%")
                                            //{
                                            //    promocodeAmt = Convert.ToDecimal(Convert.ToDecimal(model.totalAmount) * (Convert.ToDecimal(model.PromoCodeCrAmount) / 100));
                                            //    balanceAmt = 0;
                                            //}
                                            //else
                                            //{
                                                //promocodeAmt = Convert.ToDecimal(model.PromoCodebalance);
                                            balanceAmt = Convert.ToDecimal(Convert.ToInt32(model.PromoCodebalance) + Convert.ToInt32(Convert.ToDecimal(model.Cashbackamount)));
                                            //}
                                            string[] parm2 = { model.PromoCodeId, model.CustomerId, OrderId.ToString(),model.PromoCodeLinkId, dbCon.getindiantime().ToString("dd-MMM-yyyy HH:mm:ss"),model.PromoCodeCrDescription,model.Cashbackamount.ToString(),
                                                               "0",balanceAmt.ToString(),"1",
                                                               dbCon.getindiantime().ToString("dd-MMM-yyyy HH:mm:ss"), model.CustomerId};
                                                string insertCouponCodeAmt = "INSERT INTO [dbo].[tblWalletCustomerHistory]([wallet_id],[customer_id],[order_id]," +
                                                                          " [wallet_link_id],[Cr_date],[Cr_description],[Cr_amount],[Dr_date],[Dr_description], " +
                                                                          " [Dr_amount],[balance],[is_active],[created_date],[created_by]) VALUES (@1,@2,@3,@4,@5,@6,@7,null,null,@8,@9,@10,@11,@12);";
                                                int historyid = dbCon.ExecuteScalarQueryWithParams(insertCouponCodeAmt, parm2);

                                            string updatePromoCodeMark = " UPDATE tblWalletCustomerLink SET is_used = 1 WHERE customer_id = " + Convert.ToInt32(model.CustomerId) +
                                                                         " AND wallet_id = " + Convert.ToInt32(model.PromoCodeId);
                                            dbCon.ExecuteQuery(updatePromoCodeMark);
                                        }

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
        public int CreateMultipleAlternateOrderNew(string Customerid, string transid, string Address, List<ProductListNew> products, string orderMRP, string totalAmount, string totalQty, string totalWeight, out string ccode, string discountamount = "", string Redemeamount = "",string JurisdictionID = "", decimal PaidAmount = 0, string PromoCode = "", decimal CashbackAmount = 0, string ReOrderId = "")
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
                        string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc],[JurisdictionID],[CashbackAmount],[ReOrderId]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE(),@24,@25,@26);Select SCOPE_IDENTITY()";
                        string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, orderMRP, totalAmount, "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", totalQty, PaidAmount.ToString(), totalWeight, totalsaving.ToString(), Redemeamount, transid, "0", "3", PromoCode, "0", "7", "0" , JurisdictionID,CashbackAmount.ToString(), ReOrderId };
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
                                            string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc],[CustOfferCode],[RefferedOfferCode],[UnitId],[Unit],[AttributeId],[BannerProductType],[BannerId]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE(),@18,@19,@20,@21,@22,@23,@24);Select SCOPE_IDENTITY()";
                                            //string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), item.PaidAmount.ToString(), productname, item.buywith, buywithprice.ToString(), item.couponCode, item.refrcode };
                                            string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), item.PaidAmount.ToString(), productname, "0", buywithprice.ToString(), PromoCode, item.refrcode ,unitid.ToString(),sUnit.ToString(), item.AttributeId.ToString(),item.BannerProductType.ToString(),item.BannerId.ToString()};
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
        public ReOrderProductList GetProductListFromReOrder(String OrderId="", String JurisdictionId="", String CustomerId = "")
        {
            Logger.InsertLogsApp("ReOrder start ");
            ReOrderProductList objeprodt = new ReOrderProductList();
            objeprodt.ProductList = new List<NewProductDataList>();
            try
            {
                objeprodt.response = "1";
                objeprodt.message = "Successfully";
                string querystr = " SELECT O.Id AS OrderId, O.AddressId FROM[Order] O WHERE O.Id = " + OrderId +
                                  " AND O.JurisdictionID = "+ JurisdictionId + 
                                  " AND O.CustomerId = " + CustomerId;
                DataTable dtOrder = dbCon.GetDataTable(querystr);
                if (dtOrder != null && dtOrder.Rows.Count > 0)
                {
                    string addressId = dtOrder.Rows[0]["AddressId"].ToString();
                    objeprodt.AddressId = addressId;
                    string Insertdata = "select isnull((select Tagname from TagMaster where TagMaster.Id=CustomerAddress.TagId),'') as Tagname, " +
                                    " isnull((Select StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId),'') as statename, " +
                                    " isnull((Select Area from ZipCode where ZipCode.Id=CustomerAddress.AreaId),'') as Area, " +
                                    " isnull((Select Building from tblBuilding where tblBuilding.Id=CustomerAddress.BuildingId),'') as Building, " +
                                    " isnull((Select CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId),'') as CountryName, " +
                                    " isnull((Select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId),'') as CityName,*" +
                                    " from CustomerAddress where IsActive=1 and IsDeleted=0 and Id = " + addressId;
                    DataTable dtdata = dbCon.GetDataTable(Insertdata);
                    objeprodt.CustAddressList = new List<CustAddressDataList>();
                    if (dtdata != null && dtdata.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtdata.Rows.Count; i++)
                        {
                            string custaddid = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["Id"].ToString() : "");
                            string custid1 = (dtdata.Rows[i]["CustomerId"] != null ? dtdata.Rows[i]["CustomerId"].ToString() : "");
                            string fnaem1 = (dtdata.Rows[i]["FirstName"] != null ? dtdata.Rows[i]["FirstName"].ToString() : "");
                            string lname1 = "";// dtdata.Rows[i]["LastName"].ToString();
                            string tagname1 = (dtdata.Rows[i]["Tagname"] != null ? dtdata.Rows[i]["Tagname"].ToString() : "");
                            string country1 = (dtdata.Rows[i]["CountryName"] != null ? dtdata.Rows[i]["CountryName"].ToString() : "");
                            string state1 = (dtdata.Rows[i]["statename"] != null ? dtdata.Rows[i]["statename"].ToString() : "");
                            string city1 = (dtdata.Rows[i]["CityName"] != null ? dtdata.Rows[i]["CityName"].ToString() : "");
                            string addr1 = (dtdata.Rows[i]["Address"] != null ? dtdata.Rows[i]["Address"].ToString() : "");
                            string mob1 = (dtdata.Rows[i]["MobileNo"] != null ? dtdata.Rows[i]["MobileNo"].ToString() : "");
                            string email = (dtdata.Rows[i]["Email"] != null ? dtdata.Rows[i]["Email"].ToString() : "");
                            string pin1 = (dtdata.Rows[i]["PinCode"] != null ? dtdata.Rows[i]["PinCode"].ToString() : "");
                            string buildingId = (dtdata.Rows[i]["BuildingId"] != null ? dtdata.Rows[i]["BuildingId"].ToString() : "");
                            string building = (dtdata.Rows[i]["Building"] != null ? dtdata.Rows[i]["Building"].ToString() : "");
                            string AreaId = (dtdata.Rows[i]["AreaId"] != null ? dtdata.Rows[i]["AreaId"].ToString() : "");
                            string Area = (dtdata.Rows[i]["Area"] != null ? dtdata.Rows[i]["Area"].ToString() : "");
                            string buildingNo = (dtdata.Rows[i]["BuildingNo"] != null ? dtdata.Rows[i]["BuildingNo"].ToString() : "");
                            string landmark = (dtdata.Rows[i]["LandMark"] != null ? dtdata.Rows[i]["LandMark"].ToString() : "");
                            string otherdetail = (dtdata.Rows[i]["OtherDetail"] != null ? dtdata.Rows[i]["OtherDetail"].ToString() : "");
                            string stateId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["StateId"].ToString() : "");
                            string cityId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["CityId"].ToString() : "");
                            string countryId = (dtdata.Rows[i]["Id"] != null ? dtdata.Rows[i]["CountryId"].ToString() : "");
                            ;
                            objeprodt.CustAddressList.Add(new CustAddressDataList
                            {
                                CustomerAddressId = custaddid,
                                Custid = custid1,
                                fname = fnaem1,
                                lname = lname1,
                                tagname = tagname1,
                                countryId = countryId,
                                countryName = country1,
                                stateId = stateId,
                                statename = state1,
                                cityId = cityId,
                                cityname = city1,
                                addr = addr1,
                                email = email,
                                mob = mob1,
                                pcode = pin1,
                                AreaId = AreaId,
                                Area = Area,
                                BuildingId = buildingId,
                                Building = building,
                                BuildingNo = buildingNo,
                                LandMark = landmark,
                                OtherDetail = otherdetail
                            });
                        }
                        
                    }

                    string sAttributeId="",sCategoryId = "", sCategoryName = "", sProductId = "", sProductName="", sItemType="";
                    string sITitle = "", sHTitle = "", sBannerId="", sEdate = "", Attribuepathimg="";
                    bool sIsExpired = false;
                    string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                    DataTable dtpath = dbCon.GetDataTable(querydata);
                    string urlpath = string.Empty;
                    string ImageName1 = string.Empty;
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                    }
                    string Attributedata = "select KeyValue from StringResources where KeyName='ProductAttributeImageUrl'";
                    DataTable dtAttrpathimg = dbCon.GetDataTable(Attributedata);
                    if (dtAttrpathimg != null && dtAttrpathimg.Rows.Count > 0)
                    {
                        //Image Path
                        Attribuepathimg = dtAttrpathimg.Rows[0]["KeyValue"].ToString();
                    }
                    string qry = "SELECT OI.ProductId, OI.AttributeId, O.Id AS OrderId, P.Name AS ProductName, " +
                                 " PL.CategoryID, isnull(cat.CategoryName, '') as CategoryName, " +
                                  " CASE WHEN GETDATE() BETWEEN P.StartDate AND P.EndDate THEN 0 ELSE 1 END AS 'ISOfferExpired', OI.BannerProductType, " +
                                 " ISNULL(OI.BannerId, 0) AS BannerId, IM.Title AS ITitle, HM.Title AS HTitle, ISNULL(Im.ImageName,'') AS IImageName, " +
                                 " ISNULL(HM.ImageName,'') AS HImageName, (CONVERT(varchar,P.EndDate,103)+' '+ CONVERT(varchar,P.EndDate,108)) as edate " +
                                 " FROM[Order] O " +
                                 " INNER JOIN OrderItem OI ON OI.OrderId = O.Id " +
                                 " LEFT join Product P on P.Id = OI.ProductId " +
                                 " LEFT join tblCategoryProductLink PL on PL.ProductId = OI.ProductId " +
                                 " inner join Category cat on cat.CategoryID = PL.CategoryID " +
                                 " LEFT JOIN IntermediateBanners IM ON IM.Id = OI.BannerId " +
                                 " LEFT JOIN HomepageBanner HM ON HM.Id = OI.BannerId " +
                                 " WHERE OI.OrderId = " + OrderId +
                                 " AND O.JurisdictionID = " + JurisdictionId +
                                 " AND O.CustomerId = " + CustomerId;
                    DataTable dtProductList = dbCon.GetDataTable(qry);
                    if (dtProductList != null && dtProductList.Rows.Count > 0)
                    {
                        ProductAttributelist attributelist = new ProductAttributelist();
                        for (int j = 0; j < dtProductList.Rows.Count; j++)
                        {
                            if (!string.IsNullOrEmpty(dtProductList.Rows[j]["CategoryID"].ToString()))
                            {
                                sCategoryId = dtProductList.Rows[j]["CategoryID"].ToString();
                                sCategoryName = dtProductList.Rows[j]["CategoryName"].ToString();
                            }
                            else
                            {
                                sCategoryId = "0";
                                sCategoryName = "";
                            }
                            if (Convert.ToInt32(dtProductList.Rows[j]["AttributeId"]) > 0)
                            {
                                sAttributeId = Convert.ToInt32(dtProductList.Rows[j]["AttributeId"]).ToString();
                            }
                            if (Convert.ToInt32(dtProductList.Rows[j]["ProductId"]) > 0)
                            {
                                sProductId = Convert.ToInt32(dtProductList.Rows[j]["ProductId"]).ToString();
                            }
                            if (Convert.ToInt32(dtProductList.Rows[j]["BannerId"]) > 0)
                            {
                                sBannerId = Convert.ToInt32(dtProductList.Rows[j]["BannerId"]).ToString();
                            }
                            sEdate = dtProductList.Rows[j]["edate"].ToString();
                            sProductName = dtProductList.Rows[j]["ProductName"].ToString();
                            sItemType = dtProductList.Rows[j]["BannerProductType"].ToString();
                            sITitle = dtProductList.Rows[j]["ITitle"].ToString();
                            sHTitle = dtProductList.Rows[j]["HTitle"].ToString();
                            sIsExpired = Convert.ToBoolean( dtProductList.Rows[j]["ISOfferExpired"]);
                            
                            NewProductDataList objProdList = new NewProductDataList();
                            objProdList.CategoryId = sCategoryId;
                            objProdList.CategoryName = sCategoryName;
                            objProdList.ProductId = sProductId;
                            objProdList.ProductName = sProductName;
                            objProdList.ItemType = sItemType;
                            objProdList.isOfferExpired = sIsExpired;
                            objProdList.OfferEndDate = sEdate;
                            objProdList.bannerId = sBannerId;
                            
                            if (sItemType == "2")
                            {
                                objProdList.Title = sITitle;
                                objProdList.bannerURL = urlpath + dtProductList.Rows[j]["IImageName"].ToString();
                            }
                            else if (sItemType == "3")
                            {
                                objProdList.Title = sHTitle;
                                objProdList.bannerURL = urlpath + dtProductList.Rows[j]["HImageName"].ToString();
                            }
                            else 
                            {
                                objProdList.Title = "";
                                objProdList.bannerURL = "";
                            }
                            objeprodt.ProductList.Add(objProdList);

                            string AttImageDetails = " SELECT pam.unit+' - '+um.UnitName as DUnit,case when isnull(isSelected,'') = '' then 'false' else 'true' end as isSelectedDetails, " +
                                                     " Isnull(cast(cast(pam.discount as decimal(10,2)) AS FLOAT),'') AS Discount, " +
                                                     " pam.Id,pam.ProductId,pam.Unit,pam.UnitId,pam.Mrp,pam.DiscountType,pam.SoshoPrice, " +
                                                     " pam.PackingType,pam.ProductImage, pam.IsActive,pam.IsDeleted,pam.CreatedOn,pam.CreatedBy," +
                                                     " pam.isOutOfStock,case when isnull(IsBestBuy,'') = '' then 'false' else 'true' end as IsBestBuy, " +
                                                     " pam.MaxQty, pam.MinQty,case when isnull(IsQtyFreeze,'') = '' then 'false' else 'true' end as IsQtyFreeze " +
                                                     " FROM Product_ProductAttribute_Mapping pam " +
                                                     " inner join Unitmaster um on um.id=pam.UnitId " +
                                                     " where pam.id=" + sAttributeId + " and pam.IsActive=1 and pam.IsDeleted = 0";
                            DataTable dtAttdetails = dbCon.GetDataTable(AttImageDetails);
                            List<ProductAttributelist> objAttrList = new List<ProductAttributelist>();
                            if (dtAttdetails != null && dtAttdetails.Rows.Count > 0)
                            {
                                string sAMrp = "", sADiscount = "", sAPackingType = "", sAsoshoPrice = "", sAweight = "", sApackSizeId = "", sAImage = "";
                                string sAPDiscount = "", sisSelected = "", sisQtyFreeze = "";
                                string sMaxQty = "", sMinQty = "";
                                Boolean bAisOutOfStock = false;
                                for (int n = 0; n < dtAttdetails.Rows.Count; n++)
                                {
                                    attributelist = new ProductAttributelist();
                                    sApackSizeId = dtAttdetails.Rows[n]["Id"].ToString();
                                    sAMrp = dtAttdetails.Rows[n]["Mrp"].ToString();
                                    sMinQty = dtAttdetails.Rows[n]["MinQty"].ToString();
                                    sMaxQty = dtAttdetails.Rows[n]["MaxQty"].ToString();


                                    sADiscount = dtAttdetails.Rows[n]["Discount"].ToString();
                                    if (sADiscount.ToString() != "0")
                                    {
                                        if (dtAttdetails.Rows[n]["DiscountType"].ToString() == "%")
                                            sAPDiscount = sADiscount.ToString() + "% Off";
                                        else if (dtAttdetails.Rows[n]["DiscountType"].ToString() == "Fixed")
                                            sAPDiscount = CommonString.rusymbol + " " + sADiscount.ToString() + " Off";
                                        else
                                            sAPDiscount = "";
                                    }
                                    else
                                        sAPDiscount = "";

                                    sAPackingType = dtAttdetails.Rows[n]["PackingType"].ToString();
                                    sAsoshoPrice = dtAttdetails.Rows[n]["SoshoPrice"].ToString();
                                    sAweight = dtAttdetails.Rows[n]["DUnit"].ToString();
                                    sAImage = dtAttdetails.Rows[n]["ProductImage"].ToString();
                                    if (dtAttdetails.Rows[n]["isOutOfStock"].ToString() == "1")
                                        bAisOutOfStock = true;
                                    else
                                        bAisOutOfStock = false;

                                    sisSelected = dtAttdetails.Rows[n]["isSelectedDetails"].ToString();
                                    sisQtyFreeze = dtAttdetails.Rows[n]["IsQtyFreeze"].ToString();

                                    attributelist.Mrp = Convert.ToDouble(sAMrp);
                                    attributelist.Discount = sAPDiscount;
                                    attributelist.PackingType = sAPackingType;
                                    attributelist.soshoPrice = Convert.ToDouble(sAsoshoPrice);
                                    attributelist.weight = sAweight;
                                    attributelist.AImageName = Attribuepathimg + sAImage;
                                    attributelist.isOutOfStock = Convert.ToBoolean(bAisOutOfStock);
                                    attributelist.isSelected = Convert.ToBoolean(sisSelected);
                                    attributelist.isQtyFreeze = Convert.ToBoolean(sisQtyFreeze);
                                    attributelist.MinQty = Convert.ToInt32(sMinQty);
                                    attributelist.MaxQty = Convert.ToInt32(sMaxQty);
                                    attributelist.AttributeId = sApackSizeId;
                                    objAttrList.Add(attributelist);
                                }
                                objProdList.ProductAttributesList = objAttrList;

                            }
                        }
                    }
                    objeprodt.response = "1";
                    objeprodt.message = "Successfully";

                }
                else
                {
                    objeprodt.response = "0";
                    objeprodt.message = "Details Not Found";
                }


                //DataTable dtProductList = dbCon.GetDataTable("SELECT O.Id AS OrderId,O.AddressId, OI.ProductId, OI.AttributeId, P.Name AS ProductName, PA.Unit, PA.UnitId, U.UnitName, PA.isOutOfStock, PA.Mrp, P.SoshoPrice, OI.Quantity, CASE WHEN GETDATE() BETWEEN P.StartDate AND P.EndDate THEN 0 ELSE 1 END AS 'ISOfferExpired' FROM[Order] O INNER JOIN OrderItem OI ON OI.OrderId = O.Id INNER JOIN Product P ON P.Id = OI.ProductId INNER JOIN Product_ProductAttribute_Mapping PA ON PA.Id = OI.AttributeId INNER JOIn UnitMaster U ON U.Id= PA.UnitId WHERE OI.OrderId = " + OrderId+ " AND O.JurisdictionID = "+JurisdictionId+" AND O.CustomerId = "+CustomerId +"");
                //if (dtProductList != null && dtProductList.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtProductList.Rows.Count; i++)
                //    {
                //        orderList.Add(new ReOrderProductList
                //        {
                //            OrderId = dtProductList.Rows[i]["OrderId"].ToString(),
                //            AddressId = dtProductList.Rows[i]["AddressId"].ToString(),
                //            ProductId = dtProductList.Rows[i]["ProductId"].ToString(),
                //            AttributeId = dtProductList.Rows[i]["AttributeId"].ToString(),
                //            ProductName = dtProductList.Rows[i]["ProductName"].ToString(),
                //            UnitId = dtProductList.Rows[i]["UnitId"].ToString(),
                //            UnitName = dtProductList.Rows[i]["UnitName"].ToString(),
                //            Unit = dtProductList.Rows[i]["Unit"].ToString(),
                //            isOutOfStock = Convert.ToBoolean(dtProductList.Rows[i]["isOutOfStock"]),
                //            isOfferExpired = Convert.ToBoolean(dtProductList.Rows[i]["ISOfferExpired"]),
                //            Mrp =Convert.ToDecimal(dtProductList.Rows[i]["Mrp"]),
                //            SoshoPrice = Convert.ToDecimal(dtProductList.Rows[i]["SoshoPrice"]),
                //            Quantity = Convert.ToDecimal(dtProductList.Rows[i]["Quantity"])
                //        });
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, "", 0, false, "", ex.StackTrace);
            }
            return objeprodt;
        }

    }
}
