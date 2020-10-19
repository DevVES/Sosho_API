using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.Order;

namespace Test0555.Controllers.Order
{
    public class OrderController : ApiController
    {
        dbConnection dbCon = new dbConnection();
        [HttpGet]
        public OrderModels.PlaceOrder GenerateOrder(string CustomerId, decimal PaidAmount, string AddressId, string Quantity, string buywith, string discountamount = "", string Redeemeamount = "", string couponCode = "")
        {
            OrderModels.PlaceOrder objorder = new OrderModels.PlaceOrder();
            try
            {
                string merchantTxnId = dbCon.getindiantime().ToString("yyyyMMddHHmmssffff") + CustomerId;
                //decimal paidAmount = 0;
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
                        int chkalternat = CreateAlternateOrder(CustomerId, merchantTxnId, AddressId, PaidAmount, Quantity, buywith, discountamount, Redeemeamount, couponCode);

                        if (chkalternat > 0 && PaidAmount > 0)
                        {
                            objorder.resultflag = "1";
                            objorder.Message = CommonString.successmessage;
                            objorder.txnId = merchantTxnId;
                        }
                        else
                        {
                            objorder.resultflag = "0";
                            objorder.Message = "Error Creating AlternateOrder";
                        }
                    }
                }
            }

            catch (Exception ex)
            {

            }

            return objorder;
        }
        public int CreateAlternateOrder(string Customerid, string transid, string Address, decimal paidAmount, string Quantity, string buywith, string discountamount = "", string Redemeamount = "", string coupanCode = "")
        {
            try
            {
                Logger.InsertLogsApp("Order CreateAlternateOrder start : step 1");
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
                string qrypncd = "select Pincode from [CustomerAddress] where Id=" + Address + "and CustomerId=" + Customerid;
                DataTable dtpncd = dbCon.GetDataTable(qrypncd);
                int pincode = int.Parse(dtpncd.Rows[0]["Pincode"].ToString());

                foreach (DataRow dr in ShipperList.Rows)
                {
                    int ShipperId = int.Parse(dr["Id"].ToString());
                    Logger.InsertLogsApp("Order CreateAlternateOrder CheckAvailability : step 2");
                    var value = dbCon.CheckAvailability(pincode, ShipperId);

                    if (value > 0)
                    {
                        Logger.InsertLogsApp("Order CreateAlternateOrder CheckAvailability start : step 3");
                        string querystr = "select * from Product Where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "' and EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss") + "'";
                        DataTable dtmain = dbCon.GetDataTable(querystr);

                        if (dtmain != null && dtmain.Rows.Count > 0)
                        {
                            Logger.InsertLogsApp("Order CreateAlternateOrder dtmain start : step 4");
                            int productid = Convert.ToInt32(dtmain.Rows[0]["Id"]);
                            decimal offer = 0, price = 0, buywithprice = 0;
                            offer = Convert.ToDecimal(dtmain.Rows[0]["Offer"]);
                            //for check pincode servicebility
                            //DataTable dtZippcode = dbCon.GetDataTable("select ZipPostalCode from [Address] where Id=" + Address);
                            // objorder.IsServicable = ChecoutCheckPinCode(dtZippcode.Rows[0]["ZipPostalCode"].ToString(), productid.ToString()).ToString();
                            string productname = dtmain.Rows[0]["Name"].ToString();
                            int gram = 0;
                            int quantity = Convert.ToInt32(Quantity);
                            price = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                            decimal finalamount = 0, newfinalamount = 0;
                            finalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;

                            if (buywith == "1")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["Mrp"]) * quantity;
                            }
                            else if (buywith == "2")
                            {
                                buywithprice = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]);
                                newfinalamount = Convert.ToDecimal(dtmain.Rows[0]["BuyWith1FriendExtraDiscount"]) * quantity;
                            }
                            else if (buywith == "6")
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
                                string insertquery = "insert into AlterNetOrder([OrderGuid],[CustomerId],[AddressId],[OrderStatusId],[OrderDiscount],[OrderMRP],[OrderTotal],[RefundedAmount],[CustomerIp],[ShippingMethod],[Deleted],[CreatedOnUtc],[TotalQty],[PaidAmount],[TotalGram],[TotalSaving],[Customer_Redeem_Amount],[TrnId],[IsPaymentDone],[OrderSourceId],[CustOfferCode],[RefferedOfferCode],[PaymentGatewayId],[BuyWith],[UpdatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,GETDATE(),@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,GETDATE());Select SCOPE_IDENTITY()";
                                string[] param1 = { Guid.NewGuid().ToString(), Customerid, Address, "10", discountamount, finalamount.ToString(), newfinalamount.ToString(), "0", dbCon.GetIP4Address().ToString(), ShipperId.ToString(), "0", quantity.ToString(), paidAmount.ToString(), totalgram.ToString(), totalsaving.ToString(), Redemeamount, transid, "0", "3", coupanCode, "", "1", buywith };
                                int Orderrslt = dbCon.ExecuteScalarQueryWithParams(insertquery, param1);
                                if (Orderrslt > 0)
                                {
                                    string gst = "select [TaxValue] from [GstTaxCategory] where Id=" + dtmain.Rows[0]["GSTTaxId"].ToString();
                                    DataTable dtgstv = dbCon.GetDataTable(gst);
                                    string insertquery1 = "insert into [AlternetOrderItem](OrderId,[ProductId],[Quantity],[MrpPerUnit],[DiscountPerUnit],[ExtraDiscountPerUnit],[SGSTValuePerUnit],[SGSTAmountPerUnit],[CGSTValuePerUnit],[CGSTAmountPerUnit],[IGSTValuePerUnit],[IGSTAmountPerUnit],[TaxablePerUnit],[TotalAmount],[ProductName],[BuyWith],[BuyWithPerUnit],[CreatedOnUtc]) values (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,GETDATE());Select SCOPE_IDENTITY()";
                                    string[] param11 = { Orderrslt.ToString(), productid.ToString(), quantity.ToString(), price.ToString(), discountamount, offer.ToString(), "0", "0", "0", "0", "0", "0", dtgstv.Rows[0]["TaxValue"].ToString(), paidAmount.ToString(), productname, buywith.ToString(), buywithprice.ToString() };
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
        [HttpGet]
        public OrderModels.orderlist CustOrderList(String custid)
        {
            OrderModels.orderlist objorder = new OrderModels.orderlist();
            try
            {
                string CustomerId = custid;
                if (!string.IsNullOrWhiteSpace(CustomerId))
                {
                    string query = "SELECT [KeyValue] FROM [StringResources] where KeyName='ProductImageUrl'";
                    DataTable dtfolder = dbCon.GetDataTable(query);
                    string folder = "";
                    if (dtfolder.Rows.Count > 0)
                    {
                        folder = dtfolder.Rows[0]["KeyValue"].ToString();
                    }
                    string Querydata = "select [Order].CustOfferCode as cccode,(convert(varchar,[Order].CreatedOnUtc,106)+ ' '+ (CONVERT(varchar,[Order].CreatedOnUtc,108))) as OrderDate,[Order].RefferedOfferCode as Refercode,(select top 1 Product.EndDate from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as enddatetime,(select top 1 Product.Id from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as productid,[Order].id as OrderId,(select top 1 Name from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as ProductName,(select  (DATENAME(dw,CAST(DATEPART(m, EndDate) AS VARCHAR)+ '/'+ CAST(DATEPART(d, EndDate) AS VARCHAR)  + '/' + CAST(DATEPART(yy, EndDate) AS VARCHAR))) +' '+convert(varchar(12),EndDate,106)+', '+convert(varchar(12),EndDate,108) as EndDate from  Product where Product.Id=OrderItem.ProductId and Product.EndDate is not null) as EndDate,[Order].OrderTotal from [Order] inner join OrderItem on OrderItem.OrderId=[Order].Id where [Order].CustomerId=" + CustomerId + "  order by [Order].id desc";

                    DataTable dtproduct = dbCon.GetDataTable(Querydata);

                    if (dtproduct != null && dtproduct.Rows.Count > 0)
                    {


                        string dtdata = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt");
                        DataTable dtlive = new DataTable();
                        try
                        {
                            dtlive = dtproduct.Select("enddatetime<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "'").CopyToDataTable();
                        }
                        catch (Exception)
                        {
                        }
                        OrderModels.custorderdetails datacust = custdetails(CustomerId);
                        objorder.ListOrder = new List<OrderModels.ListOrder>();
                        for (int i = 0; i < dtproduct.Rows.Count; i++)
                        {

                            string productidd = dtproduct.Rows[i]["productid"].ToString();
                            string wflag;
                            string where = "";
                            string wmessage = "";
                            string isCancel = "0";
                            string ccode = string.Empty;

                            ccode = (dtproduct.Rows[i][0] != null || dtproduct.Rows[i][0].ToString() != "") ? dtproduct.Rows[i][0].ToString() : string.Empty;

                            if (productidd != "")
                            {
                                where = " and  Product.id=" + productidd;
                            }

                            string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' " + where;

                            DataTable dtproducts = dbCon.GetDataTable(expprostr);

                            if (dtproducts.Rows.Count > 0)
                            {
                                string ocode = "";
                                if (ccode != "0" && ccode != "")
                                {
                                    ocode = "/?offercode=" + ccode;
                                }

                                wflag = "1";
                                MasterDataController msc = new MasterDataController();
                                wmessage = msc.ReturnMessage("OrderSummaryPageMsg").Message + ocode;
                            }
                            else
                            {
                                wflag = "0";
                            }

                            string orderidd = dtproduct.Rows[i]["OrderId"].ToString();
                            string orddate = dtproduct.Rows[i]["OrderDate"].ToString();
                            string productname = dtproduct.Rows[i]["ProductName"].ToString();
                            string imagename = "select ImageFileName from ProductImages where Productid=" + productidd + "";
                            DataTable dtimage = dbCon.GetDataTable(imagename);
                            string imgname = "";
                            if (dtimage != null && dtimage.Rows.Count > 0)
                            {
                                imagename = folder + dtimage.Rows[0]["ImageFileName"].ToString();
                            }


                            string flag = "0";
                            if (dtlive != null && dtlive.Rows.Count > 0)
                            {
                                DataRow[] drr = dtlive.Select("OrderId=" + orderidd);
                                if (drr.Length > 0)
                                {
                                    flag = "1";

                                }
                                else
                                {
                                    flag = "0";
                                }

                            }
                            else
                            {
                                flag = "0";
                            }
                            string qry = "select Product.Id as ProductId,  [Order].[OrderMRP], Product.BuyWith1FriendExtraDiscount, Product.BuyWith5FriendExtraDiscount, (CONVERT(varchar ,Product.EndDate,106)) +' ' + (CONVERT(varchar ,Product.EndDate,108)) as pedate, [Order].Id as orderid, OrderItem.Id as orderitemid  from Product Inner join OrderItem on OrderItem.ProductId=Product.Id Inner Join [Order] On [Order].Id = OrderItem.OrderId where [Order].Id=" + orderidd;
                            DataTable dt = dbCon.GetDataTable(qry);
                            string mrp = "";
                            string forcust = "";
                            string date = "";
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                string flg = datacust.Buywith;
                                mrp = dt.Rows[0]["OrderMRP"].ToString();
                                if (flg == "1")
                                {
                                    forcust = dt.Rows[0]["OrderMRP"].ToString();
                                }
                                else if (flg == "2")
                                {
                                    forcust = dt.Rows[0]["BuyWith1FriendExtraDiscount"].ToString();
                                }
                                else if (flg == "5")
                                {
                                    forcust = dt.Rows[0]["BuyWith5FriendExtraDiscount"].ToString();
                                }
                                date = dt.Rows[0]["pedate"].ToString();
                            }
                            //OrderModels.ListOrder objorder1 = new OrderModels.ListOrder();
                            objorder.ListOrder.Add(new OrderModels.ListOrder
                            {
                                expdate = date,
                                totalamt = mrp,
                                orderid = orderidd,
                                productimg = imagename,
                                productname = productname,
                                orderdate = orddate,
                                whatsappflag = wflag,
                                whatsappMessage = wmessage,
                                IsCancel = isCancel,
                                OrderStatusText = "",
                                OrderStatus = "0"
                            });
                        }
                        objorder.Responce = "1";
                        objorder.Message = "Success";
                    }
                }
                else
                {
                    objorder.Responce = "0";
                    objorder.Message = "Fail";
                }
            }
            catch (Exception ex)
            {
                objorder.Responce = "0";
                objorder.Message = "Fail";
            }
            return objorder;
        }
        public static OrderModels.custorderdetails custdetails(string Custid)
        {
            dbConnection dbc = new dbConnection();
            OrderModels.custorderdetails objcustdataa = new OrderModels.custorderdetails();
            try
            {
                string currentproduct = "select top 1 Product.id from Product where StartDate<=GETDATE()AND EndDate>=GETDATE() order by Product.id desc";

                DataTable dt = dbc.GetDataTable(currentproduct);

                if (dt != null && dt.Rows.Count > 0)
                {

                    string productidd = dt.Rows[0]["Id"].ToString();
                    string querydata = "select (select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productstarttime,(select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productendtime,OrderItem.Id as Orderitemid,OrderItem.ProductId as Productid,convert(varchar,[Order].CreatedOnUtc,108) as Ordertime ,convert(varchar,[Order].CreatedOnUtc,103) as Orderdate ,[Order].id as OrderId,[Order].BuyWith,ISNULL([Order].CustOfferCode,0) as CustOfferCode ,ISNULL([Order].RefferedOfferCode,0) as RefferedOfferCode  from [Order] inner join OrderItem on [Order].Id=OrderItem.OrderId where CustomerId=" + Custid + " and ProductId='" + productidd + "' and [Order].IsPaymentDone=1";

                    DataTable dtorderdata = dbc.GetDataTable(querydata);

                    if (dtorderdata != null && dtorderdata.Rows.Count > 0)
                    {

                        {
                            objcustdataa.Response = "1";
                            objcustdataa.Message = "Successfully Done";
                            objcustdataa.Orderid = dtorderdata.Rows[0]["OrderId"].ToString();
                            objcustdataa.OrderItemId = dtorderdata.Rows[0]["Orderitemid"].ToString();
                            objcustdataa.ProductId = dtorderdata.Rows[0]["Productid"].ToString();
                            objcustdataa.Buywith = dtorderdata.Rows[0]["BuyWith"].ToString();
                            objcustdataa.OrderDate = dtorderdata.Rows[0]["Orderdate"].ToString();
                            objcustdataa.OrderTime = dtorderdata.Rows[0]["Ordertime"].ToString();
                            objcustdataa.Custoffercode = dtorderdata.Rows[0]["CustOfferCode"].ToString();
                            objcustdataa.ReferCode = dtorderdata.Rows[0]["RefferedOfferCode"].ToString();
                            objcustdataa.ProductEnddate = dtorderdata.Rows[0]["Productendtime"].ToString();
                            objcustdataa.ProductStartDate = dtorderdata.Rows[0]["Productstarttime"].ToString();
                            return objcustdataa;
                        }
                    }
                    else
                    {

                        objcustdataa.Response = "0";
                        objcustdataa.Message = "No Data Found";
                    }
                }
            }
            catch (Exception ee)
            {
                objcustdataa.Response = "0";
                objcustdataa.Message = "Fail";
            }
            return objcustdataa;
        }
        [HttpGet]
        public OrderModels.orderdetail CustOrderDetail(String orderid)
        {
            OrderModels.orderdetail objorderdtil = new OrderModels.orderdetail();
            try
            {
                string OrderId = orderid;
                if (orderid != "")
                {
                    string addressstr = "select FirstName as CustName,Address,(select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId)as CityName,CustomerAddress.pincode,(select StateMaster.StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId) as StateName,(select CountryMaster.CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId)as CountryName,CustomerAddress.MobileNo from CustomerAddress where Id=(select AddressId from [Order] where id=" + OrderId + ") ;";

                    DataTable dtdataa = dbCon.GetDataTable(addressstr);

                    if (dtdataa != null && dtdataa.Rows.Count > 0)
                    {
                        string custname = dtdataa.Rows[0]["CustName"].ToString();
                        objorderdtil.CustName = custname;
                        string custadd = dtdataa.Rows[0]["Address"].ToString() + ", " + dtdataa.Rows[0]["CityName"] + "-" + dtdataa.Rows[0]["pincode"].ToString() + " " + dtdataa.Rows[0]["StateName"] + ", " + dtdataa.Rows[0]["CountryName"];
                        objorderdtil.CustAddress = custadd;
                        string mobno = dtdataa.Rows[0]["MobileNo"].ToString();
                        objorderdtil.CustMob = mobno;
                    }

                    string OrderDetails = "select [Order].BuyWith,[Order].CustOfferCode,[Order].Id as oid, (convert(varchar,[Order].CreatedOnUtc,106)+ ' '+ (CONVERT(varchar,[Order].CreatedOnUtc,108))) as OrderDate, (DATENAME(dw,CAST(DATEPART(m, CreatedOnUtc) AS VARCHAR)+ '/'+ CAST(DATEPART(d, CreatedOnUtc) AS VARCHAR)  + '/' + CAST(DATEPART(yy, CreatedOnUtc) AS VARCHAR))) +' '+convert(varchar(12),CreatedOnUtc,106)+', '+convert(varchar(12),CreatedOnUtc,108) as EndDate,ISNULL([Order].OrderMRP,0) as MRP,ISNULL([Order].OrderTotal,0)as OrderTotal,[Order].TotalQTY,isnull((select top 1 Name from Payment_Methods where [Order].PaymentGatewayId=Payment_Methods.Id),'Default') as GatwayType  from [Order] where [Order].Id=" + OrderId + "";

                    DataTable dtorderdetails = dbCon.GetDataTable(OrderDetails);

                    string imaagedetails = "select Product.ProductMrp,Product.Mrp,Product.BuyWith1FriendExtraDiscount,Product.BuyWith5FriendExtraDiscount,Product.id as pid,product.Name,isnull(Unit,'0') as unitweg,isnull((select UnitName from UnitMaster where UnitMaster.Id=Product.UnitId),'Gram')as Unit from Product where Product.Id=(select ProductId from orderitem where orderid=" + OrderId + ")";
                    DataTable dtimgstr = dbCon.GetDataTable(imaagedetails);


                    string productid = "";
                    if (dtimgstr != null && dtimgstr.Rows.Count > 0)
                    {
                        productid = dtimgstr.Rows[0]["pid"].ToString();


                    }
                    bool status = ProductStatus(productid);
                    string msg = "";
                    if (status == true)
                    {
                        msg = "";
                        objorderdtil.WhatsappbtnShowStatus = "1";
                    }
                    else
                    {
                        objorderdtil.WhatsappbtnShowStatus = "0";
                    }
                    if (dtorderdetails != null && dtorderdetails.Rows.Count > 0)
                    {
                        string orderid1 = dtorderdetails.Rows[0]["oid"].ToString();
                        objorderdtil.OrderId = orderid1;
                        string expon = dtorderdetails.Rows[0]["EndDate"].ToString();
                        objorderdtil.ProductEnddate = expon;
                        string mrp = dtimgstr.Rows[0]["ProductMrp"].ToString();
                        objorderdtil.MRP = mrp;
                        string orddate = dtorderdetails.Rows[0]["OrderDate"].ToString();
                        objorderdtil.OrderDate = orddate;
                        string totalamt = dtorderdetails.Rows[0]["OrderTotal"].ToString();
                        objorderdtil.Amount = totalamt;
                        string qty = dtorderdetails.Rows[0]["TotalQTY"].ToString();
                        objorderdtil.Qty = qty;
                        string weight = dtorderdetails.Rows[0]["GatwayType"].ToString();
                        objorderdtil.Weight = weight;
                        string paymentmode = "COD";

                        objorderdtil.PaymentMode = paymentmode;
                        string productidd = dtimgstr.Rows[0]["pid"].ToString();
                        string flag = dtorderdetails.Rows[0]["BuyWith"].ToString();
                        string custorder = dtorderdetails.Rows[0]["CustOfferCode"].ToString();


                        objorderdtil.OrderStatusText = "";
                        objorderdtil.OrderStatus = "0";

                        //string mess = "";
                        string forcust = "";

                        mrp = dtimgstr.Rows[0]["Mrp"].ToString();

                        string ocode = "";
                        if (custorder != null && custorder != "")
                        {
                            ocode = "/?offercode=" + custorder;
                        }
                        if (flag == "1")
                        {

                            msg = "Share this offer on WhatsApp so that your friends can also make the most if it!";
                        }
                        else if (flag == "2")
                        {

                            forcust = dtimgstr.Rows[0]["BuyWith1FriendExtraDiscount"].ToString();

                            //string title = "Final Step";
                            string[] daaata = forcust.Split('.');
                            forcust = daaata[0].ToString();
                            string[] mrpdata = mrp.Split('.');
                            mrp = mrpdata[0].ToString();
                            objorderdtil.MRP = mrp;
                            //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                            msg = "Since you have chosen to buy with 1 friend, share offer to ensure your friend buys it by " + expon + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.Link sosho.in";


                        }
                        else if (flag == "6")
                        {

                            forcust = dtimgstr.Rows[0]["BuyWith5FriendExtraDiscount"].ToString();
                            string[] daaata = forcust.Split('.');
                            forcust = daaata[0].ToString();


                            string[] mrpdata = mrp.Split('.');
                            mrp = mrpdata[0].ToString();
                            //string title = "Final Step";

                            //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                            msg = "Since you have chosen to buy with 5 friends, share offer to ensure your friends buy it by  " + expon + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                        }

                        MasterDataController msc = new MasterDataController();
                        msg = msc.ReturnMessage("OrderDetail").Message + ocode;

                        // msg = "Hi! I just bought this awesome product at just ₹" + forcust + ". If you buy it before " + expon + ", you can also get the same discount. Just follow this link: http://www.sosho.in" + ocode + "";

                        objorderdtil.WhatsappMsg = msg;

                    }

                    if (dtimgstr.Rows.Count > 0)
                    {
                        string prdtname = dtimgstr.Rows[0]["name"].ToString();
                        objorderdtil.ProductName = prdtname;
                        string unt = dtimgstr.Rows[0]["unitweg"].ToString() + " " + dtimgstr.Rows[0]["unit"];
                        objorderdtil.Weight = unt;

                        string productidd = dtimgstr.Rows[0]["pid"].ToString();
                        string imagename = "select imagefilename from productimages where productid=" + productidd + "";
                        DataTable dtimage = dbCon.GetDataTable(imagename);
                        string query = "select [keyvalue] from [stringresources] where keyname='productimageurl'";
                        DataTable dtfolder = dbCon.GetDataTable(query);
                        string folder = "";
                        if (dtfolder.Rows.Count > 0)
                        {
                            folder = dtfolder.Rows[0]["keyvalue"].ToString();
                        }
                        string imgname = folder + dtimage.Rows[0]["imagefilename"].ToString();
                        objorderdtil.IsCancel = "0";

                        objorderdtil.ProductImg = imgname;
                        objorderdtil.Response = "1";
                        objorderdtil.Message = "Success";
                    }
                    else
                    {
                        objorderdtil.Response = "0";
                        objorderdtil.Message = "Fail";
                    }
                }



            }
            catch (Exception ex)
            {
                objorderdtil.Response = "0";
                objorderdtil.Message = "Fail";
            }

            return objorderdtil;
        }
        public static bool ProductStatus(string productid = "")
        {
            dbConnection dbc = new dbConnection();
            string where = "";
            if (productid != "")
            {
                where = " and  Product.id=" + productid;
            }
            string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' " + where;
            DataTable dtproduct = dbc.GetDataTable(expprostr);
            if (dtproduct.Rows.Count != 0 && dtproduct != null && dtproduct.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
        [HttpGet]
        public OrderModels.finallist FinalScreen(string orderid, string buyflagen, string custid, string ccode)
        {
            OrderModels.finallist objfs = new OrderModels.finallist();
            try
            {
                string OrderId = orderid;
                if (OrderId != null)
                {
                    string Querypaymenttype = "select top 1 Id,Name from payment_methods where id=(select PaymentGatewayId from [Order] where [Order].Id=" + OrderId + ")";
                    DataTable dtpayment = dbCon.GetDataTable(Querypaymenttype);
                    if (dtpayment.Rows.Count > 0)
                    {
                        string paymentmode = dtpayment.Rows[0]["Name"].ToString();
                        objfs.paymentmode = paymentmode;
                    }

                    string Custid = custid;
                    if (Custid != null && Custid != "")
                    {
                        OrderModels.custorderdetails1 objcustdetails = custdetails1(Custid);
                        string productidd = objcustdetails.ProductId;
                        DataTable dtproduct = dbCon.GetDataTable("select * from Product where id=" + productidd + "");
                        if (dtproduct != null && dtproduct.Rows.Count > 0)
                        {
                            string proname = dtproduct.Rows[0]["Name"].ToString();
                            objfs.ProductName = proname;
                            DataTable custaddr = dbCon.GetDataTable("select FirstName,Address,PinCode,(select Cityname from CityMaster where CityMaster.Id=CustomerAddress.CityId) as Cityname from CustomerAddress where id=(select AddressId from [Order] where [Order].Id=" + OrderId + ")");
                            if (custaddr != null && custaddr.Rows.Count > 0)
                            {
                                string custdetail = custaddr.Rows[0]["FirstName"] + " " + custaddr.Rows[0]["Address"] + "-" + custaddr.Rows[0]["PinCode"];
                                objfs.CustomerDetail = custdetail;
                            }
                            DataTable custorderqty = dbCon.GetDataTable("select TotalQTY from [Order] where [Order].Id=" + OrderId + "");
                            if (custorderqty != null && custorderqty.Rows.Count > 0)
                            {
                                string qtyy = custorderqty.Rows[0]["TotalQTY"].ToString();
                                string[] data123 = qtyy.Split('.');
                                string qty = data123[0];
                                objfs.Qty = qty;
                            }
                        }

                        string qry1 = "select Product.Id as ProductId,  Product.Mrp, Product.BuyWith1FriendExtraDiscount, Product.BuyWith5FriendExtraDiscount, (CONVERT(varchar ,Product.EndDate,106)) as pedate , (CONVERT(varchar ,Product.EndDate,100)) as pedate1, [Order].Id as orderid, OrderItem.Id as orderitemid,Product.Name,Convert(int,Product.ProductMrp) as ProductMrp from Product Inner join OrderItem on OrderItem.ProductId=Product.Id Inner Join [Order] On [Order].Id = OrderItem.OrderId  where [Order].Id=" + OrderId;

                        //
                        DataTable dt = dbCon.GetDataTable(qry1);

                        string mrp = "";
                        string forcust = "";
                        string date = "";
                        string flg = buyflagen;
                        if (dt != null && dt.Rows.Count > 0)
                        {

                            mrp = dt.Rows[0]["Mrp"].ToString();
                            date = dt.Rows[0]["pedate"].ToString();
                            string date1 = dt.Rows[0]["pedate1"].ToString();
                            string[] dataaa = date1.Split(' ');
                            int lendata = dataaa.Length;

                            string time = dataaa[lendata - 1];

                            date = date + ' ' + time;

                            string mess = "";

                            if (flg == "1")
                            {
                                forcust = dt.Rows[0]["Mrp"].ToString();
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                mess = "Share this offer with your friends now!";
                            }
                            else if (flg == "2")
                            {
                                forcust = dt.Rows[0]["BuyWith1FriendExtraDiscount"].ToString();
                                //string title = "Final Step";
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                mess = "Since you have chosen to buy with 1 friend, share offer to ensure your friend buys it by " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                            }
                            else if (flg == "6")
                            {
                                forcust = dt.Rows[0]["BuyWith5FriendExtraDiscount"].ToString();
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                //string title = "Final Step";

                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                mess = "Since you have chosen to buy with 5 friends, share offer to ensure your friends buy it by  " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                            }
                            objfs.msg = mess;
                            objfs.deliverymsg = "Delivery in 1-2 working days";

                        }

                        if (ccode != "" && buyflagen != "")
                        {
                            string ocode = "";
                            if (ccode != "0" && ccode != "")
                            {
                                ocode = "/?offercode=" + ccode;
                            }



                            string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' ";

                            DataTable dtproducts = dbCon.GetDataTable(expprostr);

                            if (dtproducts.Rows.Count > 0)
                            {
                                if (flg == "1")
                                {
                                    MasterDataController msc = new MasterDataController();
                                    objfs.whatsappmsg = msc.ReturnMessage("BuyAlone").Message + ocode;
                                }
                                else if (flg == "2")
                                {
                                    MasterDataController msc = new MasterDataController();
                                    objfs.whatsappmsg = msc.ReturnMessage("BuyWithOneFrd").Message + ocode;
                                }
                                else if (flg == "6")
                                {
                                    MasterDataController msc = new MasterDataController();
                                    objfs.whatsappmsg = msc.ReturnMessage("ButWith4Frd").Message + ocode;
                                }


                            }

                        }
                        else
                        {
                            objfs.whatsappmsg = "";
                        }
                        objfs.Response = "1";
                        objfs.Message = "Success";

                    }
                    else
                    {
                        objfs.Response = "0";
                        objfs.Message = "Fail";
                    }

                }
                else
                {
                    objfs.Response = "0";
                    objfs.Message = "Fail";
                }
            }
            catch (Exception ex)
            {
                objfs.Response = "0";
                objfs.Message = "Fail";
            }
            return objfs;
        }



        [HttpGet]
        public OrderModels.finallist FreeFinalScreen(string orderid, string ProductId, string buyflagen, string custid, string ccode)
        {
            OrderModels.finallist objfs = new OrderModels.finallist();
            try
            {
                string OrderId = orderid;
                if (OrderId != null)
                {
                    string Querypaymenttype = "select top 1 Id,Name from payment_methods where id=(select PaymentGatewayId from [Order] where [Order].Id=" + OrderId + ")";
                    DataTable dtpayment = dbCon.GetDataTable(Querypaymenttype);
                    if (dtpayment.Rows.Count > 0)
                    {
                        string paymentmode = dtpayment.Rows[0]["Name"].ToString();
                        objfs.paymentmode = paymentmode;
                    }
                    string Custid = custid;
                    if (Custid != null && Custid != "")
                    {
                        OrderModels.custorderdetails1 objcustdetails = FreeProcustdetails1(Custid, ProductId);
                        string productidd = objcustdetails.ProductId;
                        DataTable dtproduct = dbCon.GetDataTable("select * from Product where id=" + ProductId + "");
                        if (dtproduct != null && dtproduct.Rows.Count > 0)
                        {
                            string proname = dtproduct.Rows[0]["Name"].ToString();
                            objfs.ProductName = proname;
                            DataTable custaddr = dbCon.GetDataTable("select FirstName,Address,PinCode,(select Cityname from CityMaster where CityMaster.Id=CustomerAddress.CityId) as Cityname from CustomerAddress where id=(select AddressId from [Order] where [Order].Id=" + OrderId + ")");
                            if (custaddr != null && custaddr.Rows.Count > 0)
                            {
                                string custdetail = custaddr.Rows[0]["FirstName"] + " " + custaddr.Rows[0]["Address"] + "-" + custaddr.Rows[0]["PinCode"];
                                objfs.CustomerDetail = custdetail;
                            }
                            DataTable custorderqty = dbCon.GetDataTable("select TotalQTY from [Order] where [Order].Id=" + OrderId + "");
                            if (custorderqty != null && custorderqty.Rows.Count > 0)
                            {
                                string qtyy = custorderqty.Rows[0]["TotalQTY"].ToString();
                                string[] data123 = qtyy.Split('.');
                                string qty = data123[0];
                                objfs.Qty = qty;
                            }
                        }

                        string qry1 = "select Product.Id as ProductId,  Product.Mrp, Product.BuyWith1FriendExtraDiscount, Product.BuyWith5FriendExtraDiscount, (CONVERT(varchar ,Product.EndDate,106)) as pedate , (CONVERT(varchar ,Product.EndDate,100)) as pedate1, [Order].Id as orderid, OrderItem.Id as orderitemid,Product.Name,Convert(int,Product.ProductMrp) as ProductMrp from Product Inner join OrderItem on OrderItem.ProductId=Product.Id Inner Join [Order] On [Order].Id = OrderItem.OrderId  where [Order].Id=" + OrderId;

                        //
                        DataTable dt = dbCon.GetDataTable(qry1);

                        string mrp = "";
                        string forcust = "";
                        string date = "";
                        string flg = buyflagen;
                        if (dt != null && dt.Rows.Count > 0)
                        {

                            mrp = dt.Rows[0]["Mrp"].ToString();
                            date = dt.Rows[0]["pedate"].ToString();
                            string date1 = dt.Rows[0]["pedate1"].ToString();
                            string[] dataaa = date1.Split(' ');
                            int lendata = dataaa.Length;

                            string time = dataaa[lendata - 1];

                            date = date + ' ' + time;

                            string mess = "";

                            if (flg == "1")
                            {
                                forcust = dt.Rows[0]["Mrp"].ToString();
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                mess = "Share this offer with your friends now!";
                            }
                            else if (flg == "2")
                            {
                                forcust = dt.Rows[0]["BuyWith1FriendExtraDiscount"].ToString();
                                //string title = "Final Step";
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                mess = "Since you have chosen to buy with 1 friend, share offer to ensure your friend buys it by " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                            }
                            else if (flg == "6")
                            {
                                forcust = dt.Rows[0]["BuyWith5FriendExtraDiscount"].ToString();
                                string[] daaata = forcust.Split('.');
                                forcust = daaata[0].ToString();
                                //string title = "Final Step";

                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                mess = "Since you have chosen to buy with 5 friends, share offer to ensure your friends buy it by  " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                            }
                            objfs.msg = mess;
                            objfs.deliverymsg = "Delivery in 1-2 working days";



                            if (ccode != "" && buyflagen != "")
                            {
                                string ocode = "";
                                if (ccode != "0" && ccode != "")
                                {
                                    ocode = "/?offercode=" + ccode;
                                }

                                string productname = dt.Rows[0]["Name"].ToString();





                                string Message = "Hi! I just downloaded Sosho app and got " + productname + " at just Rs. " + mrp + ". If you also download this app, you can get this offer. Just follow this link to download shorturl.at/EMX01";

                                objfs.whatsappmsg = Message;

                                //  string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' ";

                                // DataTable dtproducts = dbCon.GetDataTable(expprostr);

                                //if (productidd != "" && productidd!= null)
                                //{
                                //    if (flg == "1")
                                //    {
                                //        MasterDataController msc = new MasterDataController();
                                //        objfs.whatsappmsg = msc.ReturnMessage("BuyAlone").Message + ocode;
                                //    }
                                //    else if (flg == "2")
                                //    {
                                //        MasterDataController msc = new MasterDataController();
                                //        objfs.whatsappmsg = msc.ReturnMessage("BuyWithOneFrd").Message + ocode;
                                //    }
                                //    else if (flg == "6")
                                //    {
                                //        MasterDataController msc = new MasterDataController();
                                //        objfs.whatsappmsg = msc.ReturnMessage("ButWith4Frd").Message + ocode;
                                //    }


                                //}
                            }
                        }
                        else
                        {
                            objfs.whatsappmsg = "";
                            objfs.Response = "0";
                            objfs.Message = "Fail";
                        }
                        objfs.Response = "1";
                        objfs.Message = "Success";

                    }
                    else
                    {
                        objfs.Response = "0";
                        objfs.Message = "Fail";
                    }

                }
                else
                {
                    objfs.Response = "0";
                    objfs.Message = "Fail";
                }
            }
            catch (Exception ex)
            {
                objfs.Response = "0";
                objfs.Message = "Fail";
            }
            return objfs;
        }
        public static OrderModels.custorderdetails1 custdetails1(string Custid)
        {
            dbConnection dbc = new dbConnection();
            OrderModels.custorderdetails1 objcustdataa = new OrderModels.custorderdetails1();
            try
            {
                string currentproduct = "select top 1 Product.id from Product where StartDate<='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbc.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' order  by Product.id desc";
                DataTable dt = dbc.GetDataTable(currentproduct);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string productidd = dt.Rows[0]["Id"].ToString();
                    string querydata = "select (select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productstarttime,(select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productendtime,OrderItem.Id as Orderitemid,OrderItem.ProductId as Productid,convert(varchar,[Order].CreatedOnUtc,108) as Ordertime ,convert(varchar,[Order].CreatedOnUtc,103) as Orderdate ,[Order].id as OrderId,[Order].BuyWith,ISNULL([Order].CustOfferCode,0) as CustOfferCode ,ISNULL([Order].RefferedOfferCode,0) as RefferedOfferCode  from [Order] inner join OrderItem on [Order].Id=OrderItem.OrderId where CustomerId=" + Custid + " and ProductId=" + productidd + "";
                    DataTable dtorderdata = dbc.GetDataTable(querydata);
                    if (dtorderdata != null && dtorderdata.Rows.Count > 0)
                    {
                        {
                            objcustdataa.Response = "1";
                            objcustdataa.Message = "Successfully Done";
                            objcustdataa.Orderid = dtorderdata.Rows[0]["OrderId"].ToString();
                            objcustdataa.OrderItemId = dtorderdata.Rows[0]["Orderitemid"].ToString();
                            objcustdataa.ProductId = dtorderdata.Rows[0]["Productid"].ToString();
                            objcustdataa.Buywith = dtorderdata.Rows[0]["BuyWith"].ToString();
                            objcustdataa.OrderDate = dtorderdata.Rows[0]["Orderdate"].ToString();
                            objcustdataa.OrderTime = dtorderdata.Rows[0]["Ordertime"].ToString();
                            objcustdataa.Custoffercode = dtorderdata.Rows[0]["CustOfferCode"].ToString();
                            objcustdataa.ReferCode = dtorderdata.Rows[0]["RefferedOfferCode"].ToString();
                            objcustdataa.ProductEnddate = dtorderdata.Rows[0]["Productendtime"].ToString();
                            objcustdataa.ProductStartDate = dtorderdata.Rows[0]["Productstarttime"].ToString();
                            return objcustdataa;
                        }
                    }
                    else
                    {
                        objcustdataa.Response = "0";
                        objcustdataa.Message = "No Data Found";
                    }
                }
            }
            catch (Exception ee)
            {
            }
            return objcustdataa;
        }


        public static OrderModels.custorderdetails1 FreeProcustdetails1(string Custid, string ProductId)
        {
            dbConnection dbc = new dbConnection();
            OrderModels.custorderdetails1 objcustdataa = new OrderModels.custorderdetails1();
            try
            {

                string productidd = ProductId;
                string querydata = "select (select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productstarttime,(select top 1 startdate from Product where Product.id=OrderItem.ProductId) as Productendtime,OrderItem.Id as OrderItemid,OrderItem.ProductId as Productid,convert(varchar,[Order].CreatedOnUtc,108) as Ordertime ,convert(varchar,[Order].CreatedOnUtc,103) as Orderdate ,[Order].id as OrderId,[Order].BuyWith,ISNULL([Order].CustOfferCode,0) as CustOfferCode ,ISNULL([Order].RefferedOfferCode,0) as RefferedOfferCode  from [Order] inner join OrderItem on [Order].Id=OrderItem.OrderId  where [Order].CustomerId=" + Custid + " AND OrderItem.ProductId='" + productidd + "'";
                DataTable dtorderdata = dbc.GetDataTable(querydata);
                if (dtorderdata != null && dtorderdata.Rows.Count > 0)
                {
                    {
                        objcustdataa.Response = "1";
                        objcustdataa.Message = "Successfully Done";
                        objcustdataa.Orderid = dtorderdata.Rows[0]["OrderId"].ToString();
                        objcustdataa.OrderItemId = dtorderdata.Rows[0]["Orderitemid"].ToString();
                        objcustdataa.ProductId = dtorderdata.Rows[0]["Productid"].ToString();
                        objcustdataa.Buywith = dtorderdata.Rows[0]["BuyWith"].ToString();
                        objcustdataa.OrderDate = dtorderdata.Rows[0]["Orderdate"].ToString();
                        objcustdataa.OrderTime = dtorderdata.Rows[0]["Ordertime"].ToString();
                        objcustdataa.Custoffercode = dtorderdata.Rows[0]["CustOfferCode"].ToString();
                        objcustdataa.ReferCode = dtorderdata.Rows[0]["RefferedOfferCode"].ToString();
                        objcustdataa.ProductEnddate = dtorderdata.Rows[0]["Productendtime"].ToString();
                        objcustdataa.ProductStartDate = dtorderdata.Rows[0]["Productstarttime"].ToString();
                        return objcustdataa;
                    }
                }
                else
                {
                    objcustdataa.Response = "0";
                    objcustdataa.Message = "No Data Found";
                }

            }
            catch (Exception ee)
            {
            }
            return objcustdataa;
        }
        [HttpGet]
        public OrderModels.orderlist CustMultipleOrderList(String custid)
        {
            OrderModels.orderlist objorder = new OrderModels.orderlist();
            try
            {
                string CustomerId = custid;
                if (!string.IsNullOrWhiteSpace(CustomerId))
                {
                    
                    string Querydata = "select ISNULL(OrderItem.CustOfferCode,[Order].CustOfferCode) as cccode,(convert(varchar,[Order].CreatedOnUtc,106)+ ' '+ (CONVERT(varchar,[Order].CreatedOnUtc,108))) as OrderDate, " +
                                       " ISNULL(OrderItem.RefferedOfferCode,[Order].RefferedOfferCode) as Refercode, " +
                                       " (select top 1 Product.EndDate from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as enddatetime, " + 
                                       " (select top 1 Product.Id from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as productid, " + 
                                       " [Order].id as OrderId,(select top 1 Name from Product where Product.Id=OrderItem.ProductId order by Product.Id desc) as ProductName, " + 
                                       " (select  (DATENAME(dw,CAST(DATEPART(m, EndDate) AS VARCHAR)+ '/'+ CAST(DATEPART(d, EndDate) AS VARCHAR)  + '/' + CAST(DATEPART(yy, EndDate) AS VARCHAR))) +' '+convert(varchar(12),EndDate,106)+', '+convert(varchar(12),EndDate,108) as EndDate " + 
                                       " from  Product where Product.Id=OrderItem.ProductId and Product.EndDate is not null) as EndDate, " + 
                                       " [Order].OrderTotal, PA.Id AS AttributeId , PA.ProductImage " + 
                                       " from [Order] " + 
                                       " inner join OrderItem on OrderItem.OrderId=[Order].Id " +
                                       " LEFT Join Product_ProductAttribute_Mapping PA ON PA.id = OrderItem.AttributeId " +
                                       " where [Order].CustomerId=" + CustomerId + "  order by [Order].id desc";

                    DataTable dtproduct = dbCon.GetDataTable(Querydata);

                    if (dtproduct != null && dtproduct.Rows.Count > 0)
                    {


                        string dtdata = dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt");
                        DataTable dtlive = new DataTable();
                        try
                        {
                            dtlive = dtproduct.Select("enddatetime<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "'").CopyToDataTable();
                        }
                        catch (Exception)
                        {
                        }
                        OrderModels.custorderdetails datacust = custdetails(CustomerId);
                        objorder.ListOrder = new List<OrderModels.ListOrder>();
                        for (int i = 0; i < dtproduct.Rows.Count; i++)
                        {

                            string productidd = dtproduct.Rows[i]["productid"].ToString();
                            string wflag;
                            string where = "";
                            string wmessage = "";
                            string isCancel = "0";
                            string ccode = string.Empty;
                            string productdetails = string.Empty;
                            string enddate = string.Empty;

                            string proddetails = "Select TOP 1 FORMAT(EndDate, 'dd MMM yyy htt') AS ProductEndDate, Product.Name + ' at only Rs ' + CONVERT(nvarchar, Product.Mrp) + ' (MRP ' + CONVERT(nvarchar, Product.ProductMrp) + ') for ' + isnull(Product.Unit, '0') + ' ' + isnull((select UnitName from UnitMaster where UnitMaster.Id = Product.UnitId),'Gram') as productdetails from Product inner join OrderItem ON OrderItem.ProductId = Product.Id Where OrderItem.OrderId =" + dtproduct.Rows[i]["OrderId"].ToString() + " Order By EndDate Desc";

                            DataTable dtproductdetail = dbCon.GetDataTable(proddetails);
                            if (dtproductdetail.Rows.Count > 0)
                            {
                                productdetails = dtproductdetail.Rows[0]["productdetails"].ToString();
                                enddate = dtproductdetail.Rows[0]["ProductEndDate"].ToString();
                            }

                            ccode = (dtproduct.Rows[i][0] != null || dtproduct.Rows[i][0].ToString() != "") ? dtproduct.Rows[i][0].ToString() : string.Empty;

                            if (productidd != "")
                            {
                                where = " and  Product.id=" + productidd;
                            }

                            string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' " + where;

                            DataTable dtproducts = dbCon.GetDataTable(expprostr);

                            if (dtproducts.Rows.Count > 0)
                            {
                                string ocode = "";
                                if (ccode != "0" && ccode != "")
                                {
                                    ocode = "/?offercode=" + ccode;
                                }

                                wflag = "1";
                                MasterDataController msc = new MasterDataController();
                                //wmessage = msc.ReturnMessage("OrderSummaryPageMsg").Message + ocode;
                                wmessage = "Hi! I bought " + productdetails + " and other items at great rates. Free shipping with Covid precautions and Cash on delivery. If you buy it before " + enddate + ", you can also get the same discount. Just follow this link: http://www.sosho.in" + ocode;
                            }
                            else
                            {
                                wflag = "0";
                            }

                            string orderidd = dtproduct.Rows[i]["OrderId"].ToString();
                            string orddate = dtproduct.Rows[i]["OrderDate"].ToString();
                            string productname = dtproduct.Rows[i]["ProductName"].ToString();
                            string prodImage = dtproduct.Rows[i]["ProductImage"].ToString();
                            string imagename = string.Empty;


                            string folder = "";
                            string keyVal = "";
                            if (!string.IsNullOrEmpty(prodImage))
                            {
                                keyVal = "ProductAttributeImageUrl";
                            }
                            else
                            {
                                keyVal = "ProductImageUrl";
                            }
                            string query = "SELECT [KeyValue] FROM [StringResources] where KeyName='"+ keyVal +"'";
                            DataTable dtfolder = dbCon.GetDataTable(query);
                            if (dtfolder.Rows.Count > 0)
                            {
                                folder = dtfolder.Rows[0]["KeyValue"].ToString();
                            }

                            if (!string.IsNullOrEmpty(prodImage))
                            {
                                imagename = folder + prodImage;
                            }
                            else
                            {
                                imagename = "select ImageFileName from ProductImages where Productid=" + productidd + "";
                                DataTable dtimage = dbCon.GetDataTable(imagename);
                                string imgname = "";
                                if (dtimage != null && dtimage.Rows.Count > 0)
                                {
                                    imagename = folder + dtimage.Rows[0]["ImageFileName"].ToString();
                                }
                            }

                            string flag = "0";
                            if (dtlive != null && dtlive.Rows.Count > 0)
                            {
                                DataRow[] drr = dtlive.Select("OrderId=" + orderidd);
                                if (drr.Length > 0)
                                {
                                    flag = "1";

                                }
                                else
                                {
                                    flag = "0";
                                }

                            }
                            else
                            {
                                flag = "0";
                            }
                            string qry = "select Product.Id as ProductId,  [Order].[OrderMRP], Product.BuyWith1FriendExtraDiscount, Product.BuyWith5FriendExtraDiscount, (CONVERT(varchar ,Product.EndDate,106)) +' ' + (CONVERT(varchar ,Product.EndDate,108)) as pedate, [Order].Id as orderid, OrderItem.Id as orderitemid  from Product Inner join OrderItem on OrderItem.ProductId=Product.Id Inner Join [Order] On [Order].Id = OrderItem.OrderId where [Order].Id=" + orderidd;
                            DataTable dt = dbCon.GetDataTable(qry);
                            string mrp = "";
                            string forcust = "";
                            string date = "";
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                string flg = datacust.Buywith;
                                mrp = dt.Rows[0]["OrderMRP"].ToString();
                                if (flg == "1")
                                {
                                    forcust = dt.Rows[0]["OrderMRP"].ToString();
                                }
                                else if (flg == "2")
                                {
                                    forcust = dt.Rows[0]["BuyWith1FriendExtraDiscount"].ToString();
                                }
                                else if (flg == "5")
                                {
                                    forcust = dt.Rows[0]["BuyWith5FriendExtraDiscount"].ToString();
                                }
                                date = dt.Rows[0]["pedate"].ToString();
                            }
                            //OrderModels.ListOrder objorder1 = new OrderModels.ListOrder();
                            objorder.ListOrder.Add(new OrderModels.ListOrder
                            {
                                expdate = date,
                                totalamt = mrp,
                                orderid = orderidd,
                                productimg = imagename,
                                productname = productname,
                                orderdate = orddate,
                                whatsappflag = wflag,
                                whatsappMessage = wmessage,
                                IsCancel = isCancel,
                                OrderStatusText = "",
                                OrderStatus = "0"
                            });
                        }
                        objorder.Responce = "1";
                        objorder.Message = "Success";
                    }
                }
                else
                {
                    objorder.Responce = "0";
                    objorder.Message = "Fail";
                }
            }
            catch (Exception ex)
            {
                objorder.Responce = "0";
                objorder.Message = "Fail";
            }
            return objorder;
        }

        [HttpGet]
        public OrderModels.orderdetailformultiple CustOrderDetailForMultipleProduct(String orderid)
        {
            OrderModels.orderdetailformultiple objorderdtil = new OrderModels.orderdetailformultiple();
            objorderdtil.products = new List<OrderModels.ProductList>();
            try
            {
                string OrderId = orderid;
                objorderdtil.CustAddress = "";
                objorderdtil.CustName = "";
                objorderdtil.CustMob = "";
                objorderdtil.OrderId = "0";
                objorderdtil.Amount = "0";
                objorderdtil.Response = "1";
                objorderdtil.Message = "Successfully";
                objorderdtil.OrderStatus = "";
                objorderdtil.OrderStatusText = "";
                objorderdtil.IsCancel = "";
                objorderdtil.PaymentMode = "";
                objorderdtil.OrderDate = "";
                if (orderid != "")
                {
                    //string addressstr = "select FirstName as CustName,Address,(select CityName from CityMaster where CityMaster.Id=CustomerAddress.CityId)as CityName,CustomerAddress.pincode,(select StateMaster.StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId) as StateName,(select CountryMaster.CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId)as CountryName,CustomerAddress.MobileNo from CustomerAddress where Id=(select AddressId from [Order] where id=" + OrderId + ") ;";
                    string addressstr = "select FirstName as CustName, Address, PinCode, BuildingId, BuildingNo, landmark, " + 
                                        " (select Cityname from CityMaster where CityMaster.Id = CustomerAddress.CityId) as Cityname, " + 
                                        " (select StateMaster.StateName from StateMaster where StateMaster.Id=CustomerAddress.StateId) as StateName ," +
                                        " (select CountryMaster.CountryName from CountryMaster where CountryMaster.Id=CustomerAddress.CountryId) as CountryName, "+
                                        " CustomerAddress.MobileNo , B.Area, B.Building " + 
                                        " from CustomerAddress " + 
                                        " LEFT JOIN tblBuilding B ON B.Id = CustomerAddress.BuildingId " +
                                        " where CustomerAddress.Id = (select AddressId from[Order] where id = " + OrderId + ") ; ";
                    DataTable dtdataa = dbCon.GetDataTable(addressstr);

                    if (dtdataa != null && dtdataa.Rows.Count > 0)
                    {
                        string custname = dtdataa.Rows[0]["CustName"].ToString();
                        objorderdtil.CustName = custname;
                        string custadd = string.Empty;
                        if (!string.IsNullOrEmpty(dtdataa.Rows[0]["BuildingNo"].ToString()) || !string.IsNullOrEmpty(dtdataa.Rows[0]["BuildingId"].ToString()))
                        {
                            custadd = dtdataa.Rows[0]["BuildingNo"] + "," + dtdataa.Rows[0]["Building"] + "," + dtdataa.Rows[0]["landmark"] + "," + dtdataa.Rows[0]["Area"] + ", " + dtdataa.Rows[0]["CityName"] + "-" + dtdataa.Rows[0]["pincode"].ToString() + " " + dtdataa.Rows[0]["StateName"] + ", " + dtdataa.Rows[0]["CountryName"];
                        }
                        else
                        {
                            custadd = dtdataa.Rows[0]["Address"].ToString() + ", " + dtdataa.Rows[0]["CityName"] + "-" + dtdataa.Rows[0]["pincode"].ToString() + " " + dtdataa.Rows[0]["StateName"] + ", " + dtdataa.Rows[0]["CountryName"];
                        }
                        objorderdtil.CustAddress = custadd;
                        string mobno = dtdataa.Rows[0]["MobileNo"].ToString();
                        objorderdtil.CustMob = mobno;
                    }

                    string OrderDetails = "select OrderItem.BuyWith,ISNULL(OrderItem.CustOfferCode,[Order].CustOfferCode) as CustOfferCode,[Order].Id as oid, (convert(varchar,[Order].CreatedOnUtc,106)+ ' '+ (CONVERT(varchar,[Order].CreatedOnUtc,108))) as OrderDate, (DATENAME(dw,CAST(DATEPART(m, [Order].CreatedOnUtc) AS VARCHAR)+ '/'+ CAST(DATEPART(d, [Order].CreatedOnUtc) AS VARCHAR)  + '/' + CAST(DATEPART(yy, [Order].CreatedOnUtc) AS VARCHAR))) +' '+convert(varchar(12),[Order].CreatedOnUtc,106)+', '+convert(varchar(12),[Order].CreatedOnUtc,108) as EndDate,ISNULL([Order].OrderMRP,0) as MRP,ISNULL([Order].OrderTotal,0)as OrderTotal,[Order].TotalQTY,isnull((select top 1 Name from Payment_Methods where [Order].PaymentGatewayId=Payment_Methods.Id),'Default') as GatwayType ,OrderStatus.[Name] as OrderStatus ,OrderStatus.Id from [Order] Inner Join OrderItem ON OrderItem.OrderId = [Order].Id LEFT Join OrderStatus on OrderStatus.Id = [Order].OrderStatusId where [Order].Id=" + OrderId + "";

                    DataTable dtorderdetails = dbCon.GetDataTable(OrderDetails);

                    //string imaagedetails = "select Product.ProductMrp,Product.Mrp,Product.BuyWith1FriendExtraDiscount,Product.BuyWith5FriendExtraDiscount,Product.id as pid,product.Name,isnull(Unit,'0') as unitweg,isnull((select UnitName from UnitMaster where UnitMaster.Id=Product.UnitId),'Gram')as Unit from Product where Product.Id IN (select ProductId from orderitem where orderid=" + OrderId + ")";
                    string imaagedetails = "select Product.ProductMrp,PA.Mrp,Product.BuyWith1FriendExtraDiscount,Product.BuyWith5FriendExtraDiscount, " + 
                                           " Product.id as pid,OrderItem.Quantity,product.Name,isnull(PA.Unit,'0') as unitweg, " + 
                                           " isnull((select UnitName from UnitMaster where UnitMaster.Id=PA.UnitId),'Gram')as Unit,case when OrderItem.BuyWith = 1 then BuyWith1FriendExtraDiscount when OrderItem.BuyWith = 2 then BuyWith5FriendExtraDiscount when OrderItem.BuyWith = 6 then offer else offer end NewProductPrice, " +
                                           " PA.Id AS AttributeId , PA.ProductImage, PA.SoshoPrice " +
                                           " from Product " + 
                                           " inner join OrderItem ON OrderItem.ProductId = Product.Id " +
                                           " LEFT Join Product_ProductAttribute_Mapping PA ON PA.id = OrderItem.AttributeId " +
                                           " Where OrderItem.OrderId=" + OrderId;
                    DataTable dtimgstr = dbCon.GetDataTable(imaagedetails);

                    if (dtorderdetails != null && dtorderdetails.Rows.Count > 0)
                    {
                        string orderid1 = dtorderdetails.Rows[0]["oid"].ToString();
                        objorderdtil.OrderId = orderid1;

                        string orddate = dtorderdetails.Rows[0]["OrderDate"].ToString();
                        objorderdtil.OrderDate = orddate;

                        string totalamt = dtorderdetails.Rows[0]["OrderTotal"].ToString();
                        objorderdtil.Amount = totalamt;

                        //string qty = dtorderdetails.Rows[0]["TotalQTY"].ToString();
                        //objorderdtil.Qty = qty;


                        string paymentmode = "COD";

                        objorderdtil.PaymentMode = paymentmode;


                        //objorderdtil.OrderStatusText = "";
                        //objorderdtil.OrderStatus = "0";


                        objorderdtil.OrderStatusText = dtorderdetails.Rows[0]["OrderStatus"].ToString();
                        objorderdtil.OrderStatus = dtorderdetails.Rows[0]["Id"].ToString();

                        string productid = "";
                        string ShowStatus = "";
                        List<OrderModels.ProductList> list = new List<OrderModels.ProductList>();
                        for (int i = 0; i < dtimgstr.Rows.Count; i++)
                        {



                            if (dtimgstr != null && dtimgstr.Rows.Count > 0)
                            {
                                productid = dtimgstr.Rows[i]["pid"].ToString();


                            }
                            bool status = ProductStatus(productid);
                            string msg = "";
                            if (status == true)
                            {
                                msg = "";
                                ShowStatus = "1";
                            }
                            else
                            {
                                ShowStatus = "0";
                            }


                            string expon = dtorderdetails.Rows[0]["EndDate"].ToString();
                            //objorderdtil.ProductEnddate = expon;
                            string mrp = dtimgstr.Rows[i]["ProductMrp"].ToString();
                            string soshoPrice = dtimgstr.Rows[i]["SoshoPrice"].ToString();
                            //objorderdtil.MRP = mrp;





                            string productidd = dtimgstr.Rows[i]["pid"].ToString();
                            string flag = dtorderdetails.Rows[i]["BuyWith"].ToString();
                            string custorder = dtorderdetails.Rows[i]["CustOfferCode"].ToString();




                            //string mess = "";
                            string forcust = "";

                            mrp = dtimgstr.Rows[i]["Mrp"].ToString();

                            string ocode = "";
                            if (custorder != null && custorder != "")
                            {
                                ocode = "/?offercode=" + custorder;
                            }
                            if (flag == "1")
                            {
                                //mrp = dtimgstr.Rows[i]["ProductMrp"].ToString();
                                mrp = dtimgstr.Rows[i]["Mrp"].ToString();
                                msg = "Share this offer on WhatsApp so that your friends can also make the most if it!";
                            }
                            else if (flag == "2")
                            {

                                forcust = dtimgstr.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();

                                //string title = "Final Step";
                                string[] daaata = forcust.Split('.');
                                if (daaata != null && daaata.Count() > 0)
                                {
                                    forcust = daaata[0].ToString();
                                }
                                string[] mrpdata = mrp.Split('.');
                                if (mrpdata != null && mrpdata.Count() > 0)
                                {
                                    mrp = mrpdata[0].ToString();
                                }
                                //objorderdtil.MRP = mrp;
                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                msg = "Since you have chosen to buy with 1 friend, share offer to ensure your friend buys it by " + expon + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.Link sosho.in";


                            }
                            else if (flag == "6")
                            {

                                forcust = dtimgstr.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                                string[] daaata = forcust.Split('.');
                                if (daaata != null && daaata.Count() > 0)
                                {
                                    forcust = daaata[0].ToString();
                                }


                                string[] mrpdata = mrp.Split('.');
                                if (mrpdata != null && mrpdata.Count() > 0)
                                {
                                    mrp = mrpdata[0].ToString();
                                }
                                //string title = "Final Step";

                                //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                msg = "Since you have chosen to buy with 5 friends, share offer to ensure your friends buy it by  " + expon + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                            }

                            MasterDataController msc = new MasterDataController();
                            msg = msc.ReturnMessage("OrderDetail").Message + ocode;

                            // msg = "Hi! I just bought this awesome product at just ₹" + forcust + ". If you buy it before " + expon + ", you can also get the same discount. Just follow this link: http://www.sosho.in" + ocode + "";

                            // objorderdtil.WhatsappMsg = msg;



                            if (dtimgstr.Rows.Count > 0)
                            {
                                string prdtname = dtimgstr.Rows[i]["name"].ToString();
                                // objorderdtil.ProductName = prdtname;
                                string unt = dtimgstr.Rows[i]["unitweg"].ToString() + " " + dtimgstr.Rows[i]["unit"];
                                //objorderdtil.Weight = unt;



                                string folder = "";
                                string keyVal = "";
                                string prodImage = dtimgstr.Rows[i]["ProductImage"].ToString();
                                string imgname = string.Empty;
                                if (!string.IsNullOrEmpty(prodImage))
                                {
                                    keyVal = "ProductAttributeImageUrl";
                                }
                                else
                                {
                                    keyVal = "ProductImageUrl";
                                }
                                string query = "SELECT [KeyValue] FROM [StringResources] where KeyName='" + keyVal + "'";
                                DataTable dtfolder = dbCon.GetDataTable(query);
                                if (dtfolder != null && dtfolder.Rows.Count > 0)
                                {
                                    folder = dtfolder.Rows[0]["KeyValue"].ToString();
                                }
                                if (!string.IsNullOrEmpty(prodImage))
                                {
                                    imgname = folder + prodImage;
                                }
                                else
                                {
                                    productidd = dtimgstr.Rows[i]["pid"].ToString();
                                    string imagename = "select imagefilename from productimages where productid=" + productidd + "";
                                    DataTable dtimage = dbCon.GetDataTable(imagename);
                                    //string query = "select [keyvalue] from [stringresources] where keyname='productimageurl'";
                                    //DataTable dtfolder = dbCon.GetDataTable(query);

                                    //if (dtfolder.Rows.Count > 0)
                                    //{
                                    //    folder = dtfolder.Rows[0]["keyvalue"].ToString();
                                    //}
                                    if (dtimage != null && dtimage.Rows.Count > 0)
                                    {
                                        imgname = folder + dtimage.Rows[0]["imagefilename"].ToString();
                                    }
                                }

                                //objorderdtil.ProductImg = imgname;

                                list.Add(new OrderModels.ProductList
                                {
                                    MRP = mrp,
                                    ProductEnddate = expon,
                                    ProductImg = imgname,
                                    ProductName = prdtname,
                                    Weight = unt,
                                    WhatsappbtnShowStatus = ShowStatus,
                                    WhatsappMsg = msg,
                                    Qty = dtimgstr.Rows[i]["Quantity"].ToString(),
                                    SoshoPrice=soshoPrice
                                    //BuyWith = flag

                                });

                                string productdetails = string.Empty;
                                string enddate = string.Empty;

                                string proddetails = "Select TOP 1 FORMAT(EndDate, 'dd MMM yyy htt') AS ProductEndDate, Product.Name + ' at only Rs ' + CONVERT(nvarchar, Product.Mrp) + ' (MRP ' + CONVERT(nvarchar, Product.ProductMrp) + ') for ' + isnull(Product.Unit, '0') + ' ' + isnull((select UnitName from UnitMaster where UnitMaster.Id = Product.UnitId),'Gram') as productdetails from Product inner join OrderItem ON OrderItem.ProductId = Product.Id Where OrderItem.OrderId =" + orderid + " Order By EndDate Desc";

                                DataTable dtproductdetail = dbCon.GetDataTable(proddetails);
                                if (dtproductdetail != null && dtproductdetail.Rows.Count > 0)
                                {
                                    productdetails = dtproductdetail.Rows[0]["productdetails"].ToString();
                                    enddate = dtproductdetail.Rows[0]["ProductEndDate"].ToString();
                                }


                                objorderdtil.WhatsappMsg = "Hi! I bought " + productdetails + " and other items at great rates. Free shipping with Covid precautions and Cash on delivery. If you buy it before " + enddate + ", you can also get the same discount. Just follow this link: http://www.sosho.in" + ocode;
                                objorderdtil.IsCancel = "0";
                                objorderdtil.Response = "1";
                                objorderdtil.Message = "Success";
                            }
                            else
                            {
                                objorderdtil.Response = "0";
                                objorderdtil.Message = "Fail";
                            }
                        }

                        objorderdtil.products = list;
                    }

                }



            }
            catch (Exception ex)
            {
                Logger.InsertLogs(Logger.InvoiceLOGS.InvoiceLogLevel.Error, "", 0, false, "", ex.StackTrace);
                objorderdtil.Response = "0";
                objorderdtil.Message = "Fail";
            }

            return objorderdtil;
        }

        [HttpGet]
        public OrderModels.finallistformulti FinalScreenForMultiple(string orderid, string custid, string ccode)
        {
            OrderModels.finallistformulti objfs = new OrderModels.finallistformulti();
            List<OrderModels.finallistprod> list = new List<OrderModels.finallistprod>();
            try
            {
                string OrderId = orderid;
                if (OrderId != null)
                {
                    string Querypaymenttype = "select top 1 Id,Name from payment_methods where id=(select PaymentGatewayId from [Order] where [Order].Id=" + OrderId + ")";
                    DataTable dtpayment = dbCon.GetDataTable(Querypaymenttype);
                    if (dtpayment.Rows.Count > 0)
                    {
                        string paymentmode = dtpayment.Rows[0]["Name"].ToString();
                        objfs.paymentmode = paymentmode;
                    }

                    objfs.deliverymsg = "Delivery in 1-2 working days";
                    objfs.whatsappmsg = "";

                    string OrderQry = " select OrderTotal,CustOfferCode,CashbackAmount,CustReedeemAmount,OrderDiscount from [order] where Id= " + OrderId;
                    DataTable dtOrderQry = dbCon.GetDataTable(OrderQry);
                    if (dtOrderQry.Rows.Count > 0)
                    {
                        objfs.OrderTotal = dtOrderQry.Rows[0]["OrderTotal"].ToString();
                        objfs.PromoCode = dtOrderQry.Rows[0]["CustOfferCode"].ToString();
                        objfs.CashbackAmount = dtOrderQry.Rows[0]["CashbackAmount"].ToString();
                        objfs.ReedeemAmount = dtOrderQry.Rows[0]["CustReedeemAmount"].ToString();
                        objfs.DiscountAmount = dtOrderQry.Rows[0]["OrderDiscount"].ToString();
                    }

                    string Custid = custid;
                    if (Custid != null && Custid != "")
                    {
                        //OrderModels.custorderdetails1 objcustdetails = custdetails1(Custid);
                        //string productidd = objcustdetails.ProductId;
                        //DataTable dtproduct = dbCon.GetDataTable("select * from Product where id=" + productidd + "");
                        //if (dtproduct != null && dtproduct.Rows.Count > 0)
                        //{
                        //    string proname = dtproduct.Rows[0]["Name"].ToString();
                        //    objfs.ProductName = proname;
                        //DataTable custaddr = dbCon.GetDataTable("select FirstName,Address,PinCode,,BuildingNo,landmark,(select Cityname from CityMaster where CityMaster.Id=CustomerAddress.CityId) as Cityname from CustomerAddress where id=(select AddressId from [Order] where [Order].Id=" + OrderId + ")");
                        DataTable custaddr = dbCon.GetDataTable("select FirstName, Address, PinCode, BuildingId, BuildingNo, landmark,(select Cityname from CityMaster where CityMaster.Id = CustomerAddress.CityId) as Cityname ,B.Area, B.Building from CustomerAddress LEFT JOIN tblBuilding B ON B.Id = CustomerAddress.BuildingId where CustomerAddress.id = (select AddressId from[Order] where[Order].Id = " + OrderId + ")");
                        string custdetail = string.Empty;
                        if (custaddr != null && custaddr.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(custaddr.Rows[0]["BuildingNo"].ToString()) || !string.IsNullOrEmpty(custaddr.Rows[0]["BuildingId"].ToString()))
                            {
                                custdetail = custaddr.Rows[0]["FirstName"] + " " + custaddr.Rows[0]["BuildingNo"] + "," + custaddr.Rows[0]["Building"] + "," + custaddr.Rows[0]["landmark"] + "," + custaddr.Rows[0]["Area"] + "-" + custaddr.Rows[0]["PinCode"];
                            }
                            else
                            {
                                custdetail = custaddr.Rows[0]["FirstName"] + " " + custaddr.Rows[0]["Address"] + "-" + custaddr.Rows[0]["PinCode"];
                            }
                            objfs.CustomerDetail = custdetail;
                        }
                        //DataTable custorderqty = dbCon.GetDataTable("select TotalQTY from [Order] where [Order].Id=" + OrderId + "");
                        //if (custorderqty != null && custorderqty.Rows.Count > 0)
                        //{
                        //    string qtyy = custorderqty.Rows[0]["TotalQTY"].ToString();
                        //    string[] data123 = qtyy.Split('.');
                        //    string qty = data123[0];
                        //    objfs.Qty = qty;
                        //}
                        //}

                        string qry1 = "select ISNULL(OrderItem.BannerProductType,0) AS BannerProductType, Product.Id as ProductId,  Product.Mrp, Product.BuyWith1FriendExtraDiscount, Product.BuyWith5FriendExtraDiscount, (CONVERT(varchar ,Product.EndDate,106)) as pedate , (CONVERT(varchar ,Product.EndDate,100)) as pedate1, [Order].Id as orderid, OrderItem.Id as orderitemid,OrderItem.BuyWith,OrderItem.Quantity,Product.Name,Convert(int,Product.ProductMrp) as ProductMrp from Product Inner join OrderItem on OrderItem.ProductId=Product.Id Inner Join [Order] On [Order].Id = OrderItem.OrderId  where [Order].Id=" + OrderId;

                        //
                        DataTable dt = dbCon.GetDataTable(qry1);

                        string mrp = "";
                        string forcust = "";
                        string date = "";
                        string whatsappmsg = "";
                        string mess = "";
                        int BannerProductType = 1;
                        
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {


                            string flg = dt.Rows[i]["BuyWith"].ToString();
                            if (dt != null && dt.Rows.Count > 0)
                            {

                                mrp = dt.Rows[i]["Mrp"].ToString();
                                date = dt.Rows[i]["pedate"].ToString();
                                string date1 = dt.Rows[i]["pedate1"].ToString();
                                string[] dataaa = date1.Split(' ');
                                int lendata = dataaa.Length;

                                string time = dataaa[lendata - 1];
                                BannerProductType = Convert.ToInt32(dt.Rows[i]["BannerProductType"]);
                                date = date + ' ' + time;



                                if (flg == "1")
                                {
                                    forcust = dt.Rows[i]["Mrp"].ToString();
                                    string[] daaata = forcust.Split('.');
                                    forcust = daaata[0].ToString();
                                    mess = "Share this offer with your friends now!";
                                }
                                else if (flg == "2")
                                {
                                    forcust = dt.Rows[i]["BuyWith1FriendExtraDiscount"].ToString();
                                    //string title = "Final Step";
                                    string[] daaata = forcust.Split('.');
                                    forcust = daaata[0].ToString();
                                    //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                    mess = "Since you have chosen to buy with 1 friend, share offer to ensure your friend buys it by " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                                }
                                else if (flg == "6")
                                {
                                    forcust = dt.Rows[i]["BuyWith5FriendExtraDiscount"].ToString();
                                    string[] daaata = forcust.Split('.');
                                    forcust = daaata[0].ToString();
                                    //string title = "Final Step";

                                    //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + title + "');", true);
                                    mess = "Since you have chosen to buy with 5 friends, share offer to ensure your friends buy it by  " + date + " to pay offer price of only ₹" + forcust + " instead of single-buy price of ₹" + mrp + " at time of delivery.";
                                }
                                //objfs.msg = mess;


                            }

                            if (ccode != "" && flg != "")
                            {
                                string ocode = "";
                                if (ccode != "0" && ccode != "")
                                {
                                    ocode = "/?offercode=" + ccode;
                                }



                                string expprostr = "select Product.Id as ProductId from Product where StartDate<='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' AND EndDate>='" + dbCon.getindiantime().ToString("dd/MMM/yyyy HH:mm:ss tt") + "' ";

                                DataTable dtproducts = dbCon.GetDataTable(expprostr);

                                if (dtproducts.Rows.Count > 0)
                                {
                                    if (flg == "1")
                                    {
                                        MasterDataController msc = new MasterDataController();
                                        whatsappmsg = msc.ReturnMessage("BuyAlone").Message + ocode;
                                    }
                                    else if (flg == "2")
                                    {
                                        MasterDataController msc = new MasterDataController();
                                        whatsappmsg = msc.ReturnMessage("BuyWithOneFrd").Message + ocode;
                                    }
                                    else if (flg == "6")
                                    {
                                        MasterDataController msc = new MasterDataController();
                                        whatsappmsg = msc.ReturnMessage("ButWith4Frd").Message + ocode;
                                    }

                                    string productdetails = string.Empty;
                                    string enddate = string.Empty;

                                    string proddetails = "Select TOP 1 FORMAT(EndDate, 'dd MMM yyy htt') AS ProductEndDate, Product.Name + ' at only Rs ' + CONVERT(nvarchar, Product.Mrp) + ' (MRP ' + CONVERT(nvarchar, Product.ProductMrp) + ') for ' + isnull(Product.Unit, '0') + ' ' + isnull((select UnitName from UnitMaster where UnitMaster.Id = Product.UnitId),'Gram') as productdetails from Product inner join OrderItem ON OrderItem.ProductId = Product.Id Where OrderItem.OrderId =" + orderid + " Order By EndDate Desc";

                                    DataTable dtproductdetail = dbCon.GetDataTable(proddetails);
                                    if (dtproductdetail.Rows.Count > 0)
                                    {
                                        productdetails = dtproductdetail.Rows[0]["productdetails"].ToString();
                                        enddate = dtproductdetail.Rows[0]["ProductEndDate"].ToString();
                                    }
                                    objfs.whatsappmsg = "Hi! I bought " + productdetails + " and other items at great rates. Free shipping with Covid precautions and Cash on delivery. If you buy it before " + enddate + ", you can also get the same discount. Just follow this link: http://www.sosho.in" + ocode;

                                }

                            }
                            else
                            {
                                whatsappmsg = "";
                            }

                            list.Add(new OrderModels.finallistprod
                            {
                                BannerProductType = BannerProductType,
                                ProductName = dt.Rows[i]["Name"].ToString(),
                               // msg = mess,
                                Qty = dt.Rows[i]["Quantity"].ToString()
                                //whatsappmsg = whatsappmsg

                            });
                        }
                        objfs.Response = "1";
                        objfs.Message = "Success";


                    }
                    else
                    {
                        objfs.Response = "0";
                        objfs.Message = "Fail";
                    }

                }
                else
                {
                    
                    objfs.Response = "0";
                    objfs.Message = "Fail";
                }
                objfs.finallistprods = list;
            }
            catch (Exception ex)
            {
                objfs.Response = "0";
                objfs.Message = "Fail";
            }
            return objfs;
        }
    }
}
