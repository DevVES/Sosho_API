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
                            string promoCode = dtmain.Rows[i]["coupon_code"].ToString();
                            string ApplicableFirstOrder = dtmain.Rows[i]["is_applicable_first_order"].ToString();
                            string perType = dtmain.Rows[i]["per_type"].ToString();
                            string perAmt = dtmain.Rows[i]["per_amount"].ToString();
                            string minOrdAmt = dtmain.Rows[i]["min_order_amount"].ToString();
                            string startdate = dtmain.Rows[i]["start_date"].ToString();
                            string enddate = dtmain.Rows[i]["end_date"].ToString();

                            objWallet.wallet_id = wid;
                            objWallet.campaign_name = campaignName;
                            objWallet.wallet_amount = walletAmt;
                            objWallet.PromoCode = promoCode;
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
                                       " LEFT OUTER JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                                       " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                        " AND H.balance > 0 " +
                                        " AND W.offer_id = 1 " +
                                        " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id) " +
                                        " Group by H.id , WC.wallet_id " +
                                        " Union " +
                                        " SELECT Sum(W.wallet_amount) as BalanceAmount " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT OUTER JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
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
                        //For PromoCode
                        string queryPromoCode = " Select  w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, w.terms, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date,  (select isnull(H.balance,0) " +
                                        " from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)) as Balance " +
                                        " FROM[tblWalletCustomerLink] WC " +
                                        " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id  " +
                                        where +
                                        " AND W.offer_id = 3 " +
                                        " AND (0 < (select isnull(H.balance,0) " +
                                        " from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)) " +
                                        " or (select count(*) from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)) = 0 " +
                                        " ) " +
                                        " Union " +
                                        " SELECT w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, w.terms, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, 0 AS Balance " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                                        " WHERE WC.customer_id = -1 " +
                                        " AND W.offer_id = 3 " +
                                        " AND(select count(*) From tblWalletCustomerHistory wch " +
                                        "  where wch.wallet_id = W.wallet_ID and wch.customer_id = " + CustomerId + ") = 0 ";
                        DataTable dtPromoCode = dbc.GetDataTable(queryPromoCode);
                        objRedeemeWalletdt.PromoCodeList = new List<WalletModel.PromoCodeDataList>();
                        if (dtPromoCode != null && dtPromoCode.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtPromoCode.Rows.Count; i++)
                            {
                                WalletModel.PromoCodeDataList objPromoCode = new WalletModel.PromoCodeDataList();
                                string walletId = dtPromoCode.Rows[i]["wallet_id"].ToString();
                                string promocode = dtPromoCode.Rows[i]["coupon_code"].ToString();
                                string campaignname = dtPromoCode.Rows[i]["campaign_name"].ToString();
                                string pertype = dtPromoCode.Rows[i]["per_type"].ToString();
                                string perAmt = dtPromoCode.Rows[i]["per_amount"].ToString();
                                string minOrdAmt = dtPromoCode.Rows[i]["min_order_amount"].ToString();
                                string startDate = dtPromoCode.Rows[i]["start_date"].ToString();
                                string endDate = dtPromoCode.Rows[i]["end_date"].ToString();
                                string balance = dtPromoCode.Rows[i]["Balance"].ToString();
                                string terms = dtPromoCode.Rows[i]["terms"].ToString();

                                objPromoCode.wallet_id = walletId;
                                objPromoCode.PromoCode = promocode;
                                objPromoCode.campaign_name = campaignname;
                                objPromoCode.per_type = pertype;
                                objPromoCode.per_amount = perAmt;
                                objPromoCode.min_order_amount = minOrdAmt;
                                objPromoCode.balance = balance;
                                objPromoCode.start_date = startDate;
                                objPromoCode.end_date = endDate;
                                objPromoCode.terms = terms;
                                objRedeemeWalletdt.PromoCodeList.Add(objPromoCode);

                            }

                        }

                        //For Cashback
                        //string queryForCashback = " Select  w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                        //                " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, H.Balance " +
                        //               " FROM[tblWalletCustomerLink] WC " +
                        //               " LEFT JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                        //               " INNER JOIN [WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                        //                where +
                        //                " AND H.balance > 0 " +
                        //                " AND W.offer_id = 2 " +
                        //                " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id) " +
                        //                " Union " +
                        //                " SELECT w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, " +
                        //                " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, 0 AS Balance " +
                        //                " FROM[WalletMaster] W " +
                        //                " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                        //                " WHERE WC.customer_id = -1 " +
                        //                " AND W.offer_id = 2 " +
                        //                " AND(select count(*) From tblWalletCustomerHistory wch " +
                        //                "  where wch.wallet_id = W.wallet_ID and wch.customer_id = " + CustomerId + ") = 0 ";
                        //DataTable dtCashback = dbc.GetDataTable(queryForCashback);
                        //objRedeemeWalletdt.CashbackList = new List<WalletModel.CashbackDataList>();
                        //if (dtCashback != null && dtCashback.Rows.Count > 0)
                        //{
                        //    for (int i = 0; i < dtCashback.Rows.Count; i++)
                        //    {
                        //        WalletModel.CashbackDataList objCashback = new WalletModel.CashbackDataList();
                        //        string walletId = dtCashback.Rows[i]["wallet_id"].ToString();
                        //        string couponcode = dtCashback.Rows[i]["coupon_code"].ToString();
                        //        string campaignname = dtCashback.Rows[i]["campaign_name"].ToString();
                        //        string pertype = dtCashback.Rows[i]["per_type"].ToString();
                        //        string perAmt = dtCashback.Rows[i]["per_amount"].ToString();
                        //        string minOrdAmt = dtCashback.Rows[i]["min_order_amount"].ToString();
                        //        string startDate = dtCashback.Rows[i]["start_date"].ToString();
                        //        string endDate = dtCashback.Rows[i]["end_date"].ToString();
                        //        string balance = dtCashback.Rows[i]["Balance"].ToString();

                        //        objCashback.wallet_id = walletId;
                        //        objCashback.campaign_name = campaignname;
                        //        objCashback.per_type = pertype;
                        //        objCashback.per_amount = perAmt;
                        //        objCashback.min_order_amount = minOrdAmt;
                        //        objCashback.balance = balance;
                        //        objCashback.start_date = startDate;
                        //        objCashback.end_date = endDate;

                        //        objRedeemeWalletdt.CashbackList.Add(objCashback);

                        //    }
                        //}
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
                    string querymain = "SELECT Top 1 w.wallet_id,H.wallet_link_id, W.terms AS CR_description, " +
                                       " WC.created_date AS CR_date,campaign_name,wallet_amount,per_type,per_amount, " +
                    " W.min_order_amount, H.balance " +
                    " FROM[tblWalletCustomerHistory] H " +
                    " INNER JOIN[tblWalletCustomerLink] WC ON H.wallet_id = WC.wallet_id " +
                    " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                     where +
                    " AND W.offer_id = 1 " +
                    " AND W.is_active = 1 " +
                    " AND H.balance > 0 " +
                    " AND GETDATE() >=  W.start_date and GETDATE() <= W.end_date " +
                    " Order by H.Id desc";

                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string walletId = dtmain.Rows[i]["wallet_id"].ToString();
                            string walletlinkId = dtmain.Rows[i]["wallet_link_id"].ToString();
                            decimal walletAmt = Convert.ToDecimal(dtmain.Rows[i]["wallet_amount"]);
                            decimal perAmt = Convert.ToDecimal(dtmain.Rows[i]["per_amount"]);
                            decimal minOrdAmt = Convert.ToDecimal(dtmain.Rows[i]["min_order_amount"]);
                            string crDate = dtmain.Rows[i]["CR_date"].ToString();
                            string crDescription = dtmain.Rows[i]["CR_description"].ToString();
                            string balance = dtmain.Rows[i]["balance"].ToString();
                            string pertype = dtmain.Rows[i]["per_type"].ToString();

                            objeWalletdt.WalletId = walletId;
                            objeWalletdt.WalletLinkId = walletlinkId;
                            objeWalletdt.WalletType = pertype;
                            objeWalletdt.CrAmount = perAmt.ToString();
                            objeWalletdt.CrDate = crDate;
                            objeWalletdt.CrDescription = crDescription;
                            objeWalletdt.balance = balance;

                            if (Convert.ToDecimal(balance) >= Convert.ToDecimal(RedeemeAmount))
                            {
                                if (dtmain.Rows[i]["per_type"].ToString() == "Fixed")
                                {
                                    if (Convert.ToDecimal(RedeemeAmount) > perAmt)
                                    {
                                        objeWalletdt.response = CommonString.DataNotFoundResponse;
                                        objeWalletdt.message = CommonString.DataNotFoundMessage;
                                        objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                    }
                                    else if (Convert.ToDecimal(RedeemeAmount) <= perAmt)
                                    {
                                        objeWalletdt.response = CommonString.successresponse;
                                        objeWalletdt.message = CommonString.successmessage;
                                        objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (dtmain.Rows[i]["per_type"].ToString() == "%")
                                {
                                    decimal redeemPerAmt = (walletAmt * perAmt) / 100;
                                    if (Convert.ToDecimal(RedeemeAmount) > redeemPerAmt)
                                    {
                                        objeWalletdt.response = CommonString.DataNotFoundResponse;
                                        objeWalletdt.message = CommonString.DataNotFoundMessage;
                                        objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + redeemPerAmt + " for this order.";
                                    }
                                    else if (Convert.ToDecimal(RedeemeAmount) <= redeemPerAmt)
                                    {
                                        objeWalletdt.response = CommonString.successresponse;
                                        objeWalletdt.message = CommonString.successmessage;
                                        objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (dtmain.Rows[i]["per_type"].ToString() == "Full Amount Applicable")
                                {
                                    if (Convert.ToDecimal(RedeemeAmount) > perAmt)
                                    {
                                        objeWalletdt.response = CommonString.DataNotFoundResponse;
                                        objeWalletdt.message = CommonString.DataNotFoundMessage;
                                        objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                    }
                                    else if (Convert.ToDecimal(RedeemeAmount) <= perAmt)
                                    {
                                        objeWalletdt.response = CommonString.successresponse;
                                        objeWalletdt.message = CommonString.successmessage;
                                        objeWalletdt.ValidationMessage = "You can redeem ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (minOrdAmt > Convert.ToDecimal(OrderAmount))
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "Wallet money can be used for order amount more than ₹ " + minOrdAmt;
                                }

                            }
                            else
                            {
                                objeWalletdt.response = CommonString.DataNotFoundResponse;
                                objeWalletdt.message = CommonString.DataNotFoundMessage;
                                objeWalletdt.ValidationMessage = "Your Wallet money is ₹ " + balance;
                            }


                        }

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

        [HttpGet]
        public WalletModel.RedeemePromoCodeFromOrder RedeemePromoCodeFromOrder(string CustomerId = "", string OrderAmount = "", string PromoCode = "")
        {
            WalletModel.RedeemePromoCodeFromOrder objeWalletdt = new WalletModel.RedeemePromoCodeFromOrder();

            try
            {
                objeWalletdt.response = "1";
                objeWalletdt.message = "Successfully";
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [WC].customer_id=" + CustomerId;

                    string querymain = " SELECT Top 1 w.wallet_id,WC.Id as wallet_link_id, W.terms AS CR_description,  WC.created_date AS CR_date,W.per_type," +
                                       " W.per_amount,wallet_amount,per_type,per_amount,  W.min_order_amount, ISNULL((select isnull(H.balance,0) " +
                                       " from [tblWalletCustomerHistory] H " +
                                       " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)),0) as Balance " +
                                       " FROM[tblWalletCustomerLink] WC  " +
                                       " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                       where +
                                       " AND W.offer_id = 3 " +
                                       " AND W.is_active = 1 " +
                                       " AND ISNULL(WC.is_used,0) = 0 " +
                                       " AND GETDATE() >=  W.start_date and GETDATE() <= W.end_date " +
                                       " AND W.coupon_code = '" + PromoCode + "'" +
                                       " order by WC.created_date  asc ";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string walletId = dtmain.Rows[i]["wallet_id"].ToString();
                            string walletlinkId = dtmain.Rows[i]["wallet_link_id"].ToString();
                            decimal walletAmt = Convert.ToDecimal(dtmain.Rows[i]["wallet_amount"]);
                            string pertype = dtmain.Rows[i]["per_type"].ToString();
                            decimal perAmt = Convert.ToDecimal(dtmain.Rows[i]["per_amount"]);
                            decimal minOrdAmt = Convert.ToDecimal(dtmain.Rows[i]["min_order_amount"]);
                            string crDate = dtmain.Rows[i]["CR_date"].ToString();
                            string crDescription = dtmain.Rows[i]["CR_description"].ToString();
                            //string crAmount = dtmain.Rows[i]["CR_amount"].ToString();
                            string balance = dtmain.Rows[i]["balance"].ToString();
                            decimal calcAmt = 0;
                            if (dtmain.Rows[i]["per_type"].ToString() == "Fixed" || dtmain.Rows[i]["per_type"].ToString() == "Full Amount Applicable")
                            {
                                calcAmt = perAmt;
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "%")
                            {
                                calcAmt = (Convert.ToDecimal(OrderAmount) * perAmt) / 100;
                            }

                            objeWalletdt.PromoCodeId = walletId;
                            objeWalletdt.PromoCodeLinkId = walletlinkId;
                            objeWalletdt.PromoCodetype = pertype;
                            objeWalletdt.PromoCodeCrAmount = perAmt.ToString();
                            objeWalletdt.PromoCodeCrDate = crDate;
                            objeWalletdt.PromoCodeCrDescription = crDescription;
                            objeWalletdt.PromoCodebalance = balance;
                            objeWalletdt.PromoCodeCalcAmount = calcAmt.ToString();
                            if (dtmain.Rows[i]["per_type"].ToString() == "Fixed")
                            {
                                if (calcAmt > perAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                }
                                else if (calcAmt <= perAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + calcAmt + " successfully for this order.";
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "%")
                            {
                                decimal redeemPerAmt = (Convert.ToDecimal(OrderAmount) * perAmt) / 100;
                                if (calcAmt > redeemPerAmt)
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + redeemPerAmt + " for this order.";
                                }
                                else if (calcAmt <= redeemPerAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + calcAmt + " successfully for this order.";
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "Full Amount Applicable")
                            {
                                if (calcAmt > perAmt)
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "You can redeem maximun ₹ " + perAmt + " for this order.";
                                }
                                else if (calcAmt <= perAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    objeWalletdt.ValidationMessage = "You can redeem ₹ " + calcAmt + " successfully for this order.";
                                }
                            }
                            if (minOrdAmt > Convert.ToDecimal(OrderAmount))
                            {
                                objeWalletdt.response = CommonString.DataNotFoundResponse;
                                objeWalletdt.message = CommonString.DataNotFoundMessage;
                                objeWalletdt.ValidationMessage = "Wallet money can be used for order amount more than ₹ " + minOrdAmt;
                            }
                        }
                    }
                    else
                    {
                        objeWalletdt.PromoCodeId = "0";
                        objeWalletdt.PromoCodeLinkId = "0";
                        objeWalletdt.PromoCodetype = "";
                        objeWalletdt.PromoCodeCrAmount = "0";
                        objeWalletdt.PromoCodeCrDate = "";
                        objeWalletdt.PromoCodeCrDescription = "";
                        objeWalletdt.PromoCodebalance = "0";
                        objeWalletdt.PromoCodeCalcAmount = "0";
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

        [HttpGet]
        public WalletModel.getWalletHistory GetWalletHistory(string CustomerId = "")
        {
            WalletModel.getWalletHistory objeWalletdt = new WalletModel.getWalletHistory();

            try
            {
                objeWalletdt.response = "1";
                objeWalletdt.message = "Successfully";
                objeWalletdt.WalletHistoryList = new List<WalletModel.WalletHistoryList>();
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {

                    where = "AND [O].CustomerId=" + CustomerId;

                    string balHistory = " SELECT top 1 id,balance FROm tblWalletCustomerHistory " +
                                        " WHERE customer_id=" + CustomerId +
                                        " order by 1 desc";
                    DataTable dtmainBal = dbc.GetDataTable(balHistory);
                    if (dtmainBal != null && dtmainBal.Rows.Count > 0)
                    {
                        string walletBalance = dtmainBal.Rows[0]["balance"].ToString();
                        objeWalletdt.WalletBalance = walletBalance;

                    }

                    string querymain = "SELECT H.created_date AS [Date], O.Id AS OrderId, H.Cr_amount, " +
                                   " H.Dr_amount,H.balance " +
                                   " FROM[Order] O " +
                                   " INNER JOIN tblWalletCustomerHistory H ON H.customer_id = O.CustomerId " +
                                   where +
                                   " ORDER BY H.created_date desc ";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {

                            WalletModel.WalletHistoryList objHistory = new WalletModel.WalletHistoryList();
                            string date = dtmain.Rows[i]["Date"].ToString();
                            string orderId = dtmain.Rows[i]["OrderId"].ToString();
                            string CrAmt = dtmain.Rows[i]["Cr_amount"].ToString();
                            string DrAmt = dtmain.Rows[i]["Dr_amount"].ToString();
                            string balance = dtmain.Rows[i]["balance"].ToString();

                            objHistory.Date = date;
                            objHistory.Summary = "Order Id " + orderId;
                            if (DrAmt != "0")
                            {
                                objHistory.CrDrAmount = DrAmt;
                                objHistory.type = "Debit";
                            }
                            else
                            {
                                objHistory.CrDrAmount = CrAmt;
                                objHistory.type = "Credit";
                            }
                            objHistory.Balance = balance;
                            objeWalletdt.WalletHistoryList.Add(objHistory);
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
    }
}
