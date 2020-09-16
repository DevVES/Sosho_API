using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.WalletManagement;

namespace Test0555.Controllers
{
    public class WalletController : ApiController
    {
        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        //14-09-2020 Developed By :- Vidhi Doshi
        [HttpGet]
        public WalletModel.getWalletDetail GetWalletDetails(string CustomerId = "")
        {
            WalletModel.getWalletDetail objeWalletdt = new WalletModel.getWalletDetail();

            try
            {
                objeWalletdt.response = "1";
                objeWalletdt.message = "Successfully";
                //objeWalletdt.WhatsAppNo = sWhatappNo;
                objeWalletdt.WalletList = new List<WalletModel.WalletDataList>();
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [WC].customer_id=" + CustomerId;
                    string querymain = "Select WC.wallet_id, W.campaign_name, W.wallet_amount, W.coupon_code, W.is_applicable_first_order, " +
                                         " W.per_type, W.per_amount, W.min_order_amount, W.start_date, W.end_date " +
                                         " FROM[dbo].[tblWalletCustomerLink] WC " +
                                         " INNER JOIN[dbo].[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                         where +
                                         " and GETDATE() >=  W.start_date and GETDATE() <= W.end_date ";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        string sWalletId = "";
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            sWalletId = dtmain.Rows[i]["wallet_id"].ToString();

                            WalletModel.WalletDataList objWallet = new WalletModel.WalletDataList();
                            string wid = dtmain.Rows[i]["wallet_id"].ToString();
                            string campaignName = dtmain.Rows[i]["campaign_name"].ToString();
                            string walletAmt = dtmain.Rows[i]["wallet_amount"].ToString();
                            string couponCode = dtmain.Rows[i]["coupon_code"].ToString();
                            string ApplicableFirstOrder = dtmain.Rows[i]["is_applicable_first_order"].ToString();
                            string perType = dtmain.Rows[i]["per_type"].ToString();
                            string perAmt = dtmain.Rows[i]["per_amount"].ToString();
                            string minOrdAmt = dtmain.Rows[i]["min_order_amount"].ToString();
                            string startdate = dtmain.Rows[i]["start_date"].ToString();
                            string enddate = dtmain.Rows[i]["end_date"].ToString();

                            objWallet.wallet_id = wid;
                            objWallet.campaign_name = campaignName;
                            objWallet.wallet_amount = walletAmt;
                            objWallet.coupon_code = couponCode;
                            objWallet.is_applicable_first_order = ApplicableFirstOrder;
                            objWallet.per_type = perType;
                            objWallet.per_amount = perAmt;
                            objWallet.min_order_amount = minOrdAmt;
                            objWallet.start_date = startdate;
                            objWallet.end_date = enddate;
                            objeWalletdt.WalletList.Add(objWallet);
                        }
                        objeWalletdt.response = CommonString.successresponse;
                        objeWalletdt.message = CommonString.successmessage;
                    }
                    else
                    {
                        objeWalletdt.response = CommonString.DataNotFoundResponse;
                        objeWalletdt.message = CommonString.DataNotFoundMessage;
                    }
                }
                return objeWalletdt;
            }
            catch (Exception ex)
            {
                objeWalletdt.response = CommonString.Errorresponse;
                objeWalletdt.message = ex.StackTrace + " " + ex.Message;
                return objeWalletdt;
            }
        }

        [HttpGet]
        public WalletModel.RedeemeWallet GetCustomerOfferDetail(string CustomerId = "")
        {
            WalletModel.RedeemeWallet objRedeemeWalletdt = new WalletModel.RedeemeWallet();

            try
            {
                objRedeemeWalletdt.response = "1";
                objRedeemeWalletdt.message = "Successfully";
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [WC].customer_id=" + CustomerId;
                    //For Wallet
                    string querymain = "Select sum(BalanceAmount) AS BalanceAmount From " +
                                       " ( " +
                                       " Select Sum(H.balance) as BalanceAmount " +
                                       " FROM[tblWalletCustomerLink] WC " +
                                       " LEFT JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                                       " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                        " AND H.balance > 0 " +
                                        " AND W.offer_id = 1 " +
                                        " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id) " +
                                        " Group by H.id , WC.wallet_id " +
                                        " Union " +
                                        " SELECT Sum(W.wallet_amount) as BalanceAmount " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                                        " WHERE WC.customer_id = -1 " +
                                        " AND W.offer_id = 1 " +
                                        " AND(select count(*) From tblWalletCustomerHistory wch " +
                                        "  where wch.wallet_id = W.wallet_ID and wch.customer_id = " + CustomerId + ") = 0 " +
                                        " ) t ";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            //WalletModel.RedeemeWallet objWallet = new WalletModel.RedeemeWallet();
                            string redeemeAmt = dtmain.Rows[i]["BalanceAmount"].ToString();

                            objRedeemeWalletdt.RedeemeAmount = redeemeAmt;

                        }
                        //For CouponCode
                        string queryCouponCode = " Select  w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, H.Balance " +
                                       " FROM[tblWalletCustomerLink] WC " +
                                       " LEFT JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                                       " INNER JOIN [WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                        " AND H.balance > 0 " +
                                        " AND W.offer_id = 3 " +
                                        " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id) " +
                                        " Union " +
                                        " SELECT w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, 0 AS Balance " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                                        " WHERE WC.customer_id = -1 " +
                                        " AND W.offer_id = 3 " +
                                        " AND(select count(*) From tblWalletCustomerHistory wch " +
                                        "  where wch.wallet_id = W.wallet_ID and wch.customer_id = " + CustomerId + ") = 0 ";
                        DataTable dtCouponCode = dbc.GetDataTable(queryCouponCode);
                        objRedeemeWalletdt.CouponCodeList = new List<WalletModel.CouponCodeDataList>();
                        if (dtCouponCode != null && dtCouponCode.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtCouponCode.Rows.Count; i++)
                            {
                                WalletModel.CouponCodeDataList objCouponCode = new WalletModel.CouponCodeDataList();
                                string walletId = dtCouponCode.Rows[i]["wallet_id"].ToString();
                                string couponcode = dtCouponCode.Rows[i]["coupon_code"].ToString();
                                string campaignname = dtCouponCode.Rows[i]["campaign_name"].ToString();
                                string pertype = dtCouponCode.Rows[i]["per_type"].ToString();
                                string perAmt = dtCouponCode.Rows[i]["per_amount"].ToString();
                                string minOrdAmt = dtCouponCode.Rows[i]["min_order_amount"].ToString();
                                string startDate = dtCouponCode.Rows[i]["start_date"].ToString();
                                string endDate = dtCouponCode.Rows[i]["end_date"].ToString();
                                string balance = dtCouponCode.Rows[i]["Balance"].ToString();

                                objCouponCode.wallet_id = walletId;
                                objCouponCode.coupon_code = couponcode;
                                objCouponCode.campaign_name = campaignname;
                                objCouponCode.per_type = pertype;
                                objCouponCode.per_amount = perAmt;
                                objCouponCode.min_order_amount = minOrdAmt;
                                objCouponCode.balance = balance;
                                objCouponCode.start_date = startDate;
                                objCouponCode.end_date = endDate;

                                objRedeemeWalletdt.CouponCodeList.Add(objCouponCode);

                            }

                        }

                        //For Cashback
                        string queryForCashback = " Select  w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, H.Balance " +
                                       " FROM[tblWalletCustomerLink] WC " +
                                       " LEFT JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                                       " INNER JOIN [WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                        " AND H.balance > 0 " +
                                        " AND W.offer_id = 2 " +
                                        " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id) " +
                                        " Union " +
                                        " SELECT w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, 0 AS Balance " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                                        " WHERE WC.customer_id = -1 " +
                                        " AND W.offer_id = 2 " +
                                        " AND(select count(*) From tblWalletCustomerHistory wch " +
                                        "  where wch.wallet_id = W.wallet_ID and wch.customer_id = " + CustomerId + ") = 0 ";
                        DataTable dtCashback = dbc.GetDataTable(queryForCashback);
                        objRedeemeWalletdt.CashbackList = new List<WalletModel.CashbackDataList>();
                        if (dtCashback != null && dtCashback.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtCashback.Rows.Count; i++)
                            {
                                WalletModel.CashbackDataList objCashback = new WalletModel.CashbackDataList();
                                string walletId = dtCashback.Rows[i]["wallet_id"].ToString();
                                string couponcode = dtCashback.Rows[i]["coupon_code"].ToString();
                                string campaignname = dtCashback.Rows[i]["campaign_name"].ToString();
                                string pertype = dtCashback.Rows[i]["per_type"].ToString();
                                string perAmt = dtCashback.Rows[i]["per_amount"].ToString();
                                string minOrdAmt = dtCashback.Rows[i]["min_order_amount"].ToString();
                                string startDate = dtCashback.Rows[i]["start_date"].ToString();
                                string endDate = dtCashback.Rows[i]["end_date"].ToString();
                                string balance = dtCashback.Rows[i]["Balance"].ToString();

                                objCashback.wallet_id = walletId;
                                objCashback.campaign_name = campaignname;
                                objCashback.per_type = pertype;
                                objCashback.per_amount = perAmt;
                                objCashback.min_order_amount = minOrdAmt;
                                objCashback.balance = balance;
                                objCashback.start_date = startDate;
                                objCashback.end_date = endDate;

                                objRedeemeWalletdt.CashbackList.Add(objCashback);

                            }
                        }
                        objRedeemeWalletdt.response = CommonString.successresponse;
                        objRedeemeWalletdt.message = CommonString.successmessage;
                    }

                    else
                    {
                        objRedeemeWalletdt.response = CommonString.DataNotFoundResponse;
                        objRedeemeWalletdt.message = CommonString.DataNotFoundMessage;
                    }
                }
                return objRedeemeWalletdt;
            }
            catch (Exception ex)
            {
                objRedeemeWalletdt.response = CommonString.Errorresponse;
                objRedeemeWalletdt.message = ex.StackTrace + " " + ex.Message;
                return objRedeemeWalletdt;
            }
        }

        [HttpGet]
        public WalletModel.RedeemeWalletFromOrder RedeemeWalletFromOrder(string CustomerId = "", string OrderAmount = "", string RedeemeAmount = "")
        {
            WalletModel.RedeemeWalletFromOrder objeWalletdt = new WalletModel.RedeemeWalletFromOrder();

            try
            {
                objeWalletdt.response = "1";
                objeWalletdt.message = "Successfully";
                //objeWalletdt.WhatsAppNo = sWhatappNo;
                //objeWalletdt.WalletList = new List<WalletModel.WalletDataList>();
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [WC].customer_id=" + CustomerId;
                    string querymain = "SELECT distinct w.wallet_id,campaign_name,wallet_amount,per_type,per_amount, W.min_order_amount " +
                                       " FROM[tblWalletCustomerHistory] H " +
                                       " INNER JOIN[tblWalletCustomerLink] WC ON H.wallet_id = WC.wallet_id " +
                                       " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                       " AND W.offer_id = 1 " +
                                       " AND W.is_active = 1 " +
                                       " AND GETDATE() >=  W.start_date and GETDATE() <= W.end_date ";

//                    " AND W.min_order_amount <= " + OrderAmount +
  //                                     " AND H.balance >= " + RedeemeAmount +

                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        //string sWalletId = "";
                        string type = "";
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {

                            int walletAmt = Convert.ToInt32(dtmain.Rows[i]["wallet_amount"]);
                            int perAmt = Convert.ToInt32(dtmain.Rows[i]["per_amount"]);
                            int minOrdAmt = Convert.ToInt32(dtmain.Rows[i]["min_order_amount"]);

                            if (dtmain.Rows[i]["per_type"].ToString() == "Fixed")
                            {
                                if (Convert.ToInt32(RedeemeAmount) > perAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                }
                                else if (Convert.ToInt32(RedeemeAmount) <= perAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "%")
                            {
                                int redeemPerAmt = (walletAmt * perAmt) / 100;
                                if (Convert.ToInt32(RedeemeAmount) > redeemPerAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + redeemPerAmt + " for this order.";
                                }
                                else if (Convert.ToInt32(RedeemeAmount) <= redeemPerAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "Full Amount Applicable")
                            {
                                if (Convert.ToInt32(RedeemeAmount) > perAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                }
                                else if (Convert.ToInt32(RedeemeAmount) <= perAmt)
                                {
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                }
                            }
                            if(minOrdAmt > Convert.ToInt32(OrderAmount))
                            {
                                objeWalletdt.ValidationMessage = "Wallet money can be used for order amount more than ₹ " + minOrdAmt;
                            }
                            
                        }
                        objeWalletdt.response = CommonString.successresponse;
                        objeWalletdt.message = CommonString.successmessage;
                    }
                    else
                    {
                        objeWalletdt.response = CommonString.DataNotFoundResponse;
                        objeWalletdt.message = CommonString.DataNotFoundMessage;
                        objeWalletdt.ValidationMessage = "";
                    }
                }
                return objeWalletdt;
            }
            catch (Exception ex)
            {
                objeWalletdt.response = CommonString.Errorresponse;
                objeWalletdt.message = ex.StackTrace + " " + ex.Message;
                return objeWalletdt;
            }
        }
    }
}
