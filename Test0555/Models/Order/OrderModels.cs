using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Order
{
    public class OrderModels
    {
        public class PlaceOrder
        {
            public String resultflag = "";
            public String Message = "";
            public String txnId = "";
        }

        public class orderlist
        {
            public String Responce = "";
            public String Message = "";
        
            public List<ListOrder> ListOrder { get; set; }

        }

        public class ListOrder
        {
            public String orderid = "";
            public string orderdate = "";
            public string totalamt = "";
            public string productname = "";            
            public string productimg = "";
            public string expdate = "";
            public string whatsappflag = "";
            public string whatsappMessage = "";
            public string IsCancel = "";
            public String OrderStatusText = "";
            public string OrderStatus = "";


        }
        public class custorderdetails
        {
            public string Response;
            public string Message;
            public string Orderid;
            public string Buywith;
            public string OrderDate;
            public string OrderTime;
            public string Custoffercode;
            public string ReferCode;
            public string ProductEnddate;
            public string ProductStartDate;
            public string ProductId;
            public string OrderItemId;
            
        }


        public class orderdetail
        {
            public string CustName;
            public string CustAddress;
            public string CustMob;
            public string OrderId;
            public string OrderDate;
            public string Amount;
            public string Qty;
            public string PaymentMode;
            public string Response;
            public string Message;
            public string IsCancel;
            public string OrderStatus;
            public string OrderStatusText;
            public string ProductImg;
            public string ProductName;
            public string WhatsappbtnShowStatus;
            public string WhatsappMsg;
            public string ProductEnddate;
            public string Weight;
            public string MRP;
        }

        public class orderdetailformultiple
        {
            public string CustName;
            public string CustAddress;
            public string CustMob;
            public string OrderId;
            public string OrderDate;
            public string Amount;
            public string WhatsappMsg = "";
            //public string Qty;
            public string PaymentMode;
            public string Response;
            public string Message;
            public string IsCancel;
            public string OrderStatus;
            public string OrderStatusText;
            public List<ProductList> products { get; set; }


        }
        public class ProductList
        {
            public string ProductImg;
            public string ProductName;
            public string WhatsappbtnShowStatus;
            public string WhatsappMsg;
            public string ProductEnddate;
            public string Weight;
            public string MRP;
            public string Qty;
            public string SoshoPrice;
            // public string BuyWith;
        }
        public class finallist
        {
            public string ProductName;
            public string CustomerDetail;
            public string Qty;
            public string paymentmode;
            public string msg;
            public string deliverymsg;
            public string whatsappmsg;
            public string Response;
            public string Message;
        }

        public class finallistformulti
        {
           
            public string CustomerDetail;
            public string paymentmode;
            public string deliverymsg;
            public string whatsappmsg;
            public string facebookMsg;
            public string Response;
            public string Message;
            public string OrderTotal;
            public string PromoCode;
            public string CashbackAmount;
            public string ReedeemAmount;
            public string DiscountAmount;
            public List<finallistprod> finallistprods { get; set; }

        }
        
        public class finallistprod
        {
            public int BannerProductType;
            public string ProductName;
            public string Qty;
            //public string msg;
        }

        public class custorderdetails1
        {
            public string Response;
            public string Message;
            public string Orderid;
            public string Buywith;
            public string OrderDate;
            public string OrderTime;
            public string Custoffercode;
            public string ReferCode;
            public string ProductEnddate;
            public string ProductStartDate;
            public string ProductId;
            public string OrderItemId;
        }
    }
}