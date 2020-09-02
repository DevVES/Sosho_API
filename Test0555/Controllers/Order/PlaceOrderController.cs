using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.Order;

namespace Test0555.Controllers.Order
{
    public class PlaceOrderController : ApiController
    {
         dbConnection dbCon = new dbConnection();
        
        [HttpGet]
         public PlaceOrderModel PlaceOrder(String CustomerId, decimal PaidAmount, string AddressId, string Quantity, string buywith, string discountamount = "", string Redeemeamount = "", string couponCode = "")
         {
               PlaceOrderModel objplaceorder = new PlaceOrderModel();
               try
               {
                   string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + CustomerId;
                   if(PaidAmount==0)
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
                               int chkalternat = CreateAlternateOrder(CustomerId, merchantTxnId, AddressId, PaidAmount.ToString(), Quantity, buywith, discountamount, Redeemeamount, couponCode);
                               if (chkalternat > 0 && PaidAmount == 0)
                               {
                                   int OrderId = WalletPlaceOrder(merchantTxnId);
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
                                                       objplaceorder.resultflag = "1";
                                                       objplaceorder.OrderId = OrderId.ToString();
                                                       objplaceorder.Message = "Your OrderNo:" + OrderId + " has been succesfully generated";
                                                   }
                                               }
                                           }
                                           else
                                           {
                                               objplaceorder.resultflag = "1";
                                               objplaceorder.OrderId = OrderId.ToString();
                                               objplaceorder.Message = "Your Order was Not Generated";
                                           }
                                       }
                                       catch (Exception err)
                                       {
                                           objplaceorder.resultflag = "1";
                                           objplaceorder.OrderId = OrderId.ToString();
                                           objplaceorder.Message = "Your Order was Not Generated";
                                       }
                                       #endregion
                                   }
                                   else
                                   {
                                       objplaceorder.resultflag = "1";
                                       objplaceorder.OrderId = OrderId.ToString();
                                       objplaceorder.Message = "Your Order was Not Generated";
                                   }

                               }
                               else
                               {
                                   objplaceorder.resultflag = "0";
                                   objplaceorder.Message = "Your Order was Not Generated";
                               }

                           }
                       }
                   }

               }
               catch(Exception ex)
               {

               }
               return objplaceorder;
         }
        public int CreateAlternateOrder(string Customerid, string transid, string Address, string paidAmount, string Quantity, string buywith, string discountamount = "", string Redemeamount = "", string coupanCode = "")
        {
            try
            {
                Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder start : step 1");
                if (discountamount == "")
                { discountamount = "0"; }
                if (Redemeamount == "")
                { Redemeamount = "0"; }
                if (coupanCode == "")
                { coupanCode = "0"; }

                decimal totalamount = 0;
                decimal totalgram = 0;
                decimal totaloffer = 0;
                decimal totalsaving = 0;
                decimal totalquantity = 0;
                decimal shiprate = 0;
                decimal newprice = 0, NewTotalAmount = 0;

                string startdate = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss");
                DataTable ShipperList = dbCon.GetAllActiveShipperDetails();
                string qrypncd = "select Pincode from [CustomerAddress] where Id="+Address +"and CustomerId="+Customerid;
                DataTable dtpncd = dbCon.GetDataTable(qrypncd);
                int pincode = int.Parse(dtpncd.Rows[0]["Pincode"].ToString());
                foreach (DataRow dr in ShipperList.Rows)
                {
                    int ShipperId = int.Parse(dr["Id"].ToString());
                    Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability : step 2");
                    var value = dbCon.CheckAvailability(pincode, ShipperId);

                    if (value > 0)
                    {
                        Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder CheckAvailability start : step 3");
                        //string querystr = "select * from Product where IsActive=1 and IsDeleted=0 and [StartDate]<='" + startdate + "' and [EndDate]>='" + startdate + "' and id=1045";
                        string querystr = "select * from Product Where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";

                        DataTable dtmain = dbCon.GetDataTable(querystr);

                        if (dtmain != null && dtmain.Rows.Count > 0)
                        {
                            Logger.InsertLogsApp("PlaceOrder CreateAlternateOrder dtmain start : step 4");
                            int productid = Convert.ToInt32(dtmain.Rows[0]["Id"]);
                            decimal offer = 0, price = 0,buywithprice=0;
                            offer = Convert.ToDecimal(dtmain.Rows[0]["Offer"]);
                            //for check pincode servicebility
                            //DataTable dtZippcode = dbCon.GetDataTable("select ZipPostalCode from [Address] where Id=" + Address);
                            // objorder.IsServicable = ChecoutCheckPinCode(dtZippcode.Rows[0]["ZipPostalCode"].ToString(), productid.ToString()).ToString();
                            string productname = dtmain.Rows[0]["Name"].ToString();
                            int gram = 0;
                            int quantity = Convert.ToInt32(Quantity);
                            price = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                             
                            decimal finalamount= 0,newfinalamount=0;
                            finalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;

                            if(buywith =="1")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;
                            }
                            else if(buywith =="2")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]) * quantity;
                            }
                            else if(buywith =="6")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["BuyWith5FriendExtraDiscount"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["BuyWith5FriendExtraDiscount"]) * quantity;
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
                                string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                                string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, finalamount.ToString(), newfinalamount.ToString(), "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", quantity.ToString(), paidAmount, totalgram.ToString(), totalsaving.ToString(), Redemeamount, transid, "0", "3", coupanCode, "", "1" ,buywith};
                                int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                                if (Orderrslt > 0)
                                {
                                    string gst = "select [TaxValue] from [GstTaxCategory] where Id=" + dtmain.Rows[0]["GSTTaxId"].ToString();
                                    DataTable dtgstv = dbCon.GetDataTable(gst);
                                    string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE());Select SCOPE_IDENTITY()";
                                    string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), paidAmount, productname, buywith.ToString(), buywithprice.ToString() };
                                    int result11 = dbCon.ExecuteScalarQueryWithParams(insertquery1, param11);
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
        
       
        private int WalletPlaceOrder(String txnId)
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
    }
}
