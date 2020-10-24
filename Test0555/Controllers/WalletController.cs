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
        public WalletModel.RedeemeWallet GetCustomerOfferDetail(string CustomerId = "",string OrderAmount ="")
        {
            WalletModel.RedeemeWallet objRedeemeWalletdt = new WalletModel.RedeemeWallet();
            try
            {
                objRedeemeWalletdt.response = "1";
                objRedeemeWalletdt.message = "Successfully";
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "WHERE [WC].customer_id=" + CustomerId;
                    //For Wallet
                    String querymain = " Select Top 1 H.balance as BalanceAmount, h.Id " +
                                      " FROM[tblWalletCustomerLink] WC " +
                                      " LEFT OUTER JOIN[tblWalletCustomerHistory] H ON H.wallet_id = WC.wallet_id " +
                                      " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                        where +
                                        " AND H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id and Hi.customer_id =  WC.customer_id) " +
                                        " AND GETDATE() >=  W.start_date and GETDATE() <= W.end_date " +
                                        " order by 2 desc";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        string usagequery = "SELECT w.per_type,w.per_amount,w.min_order_amount " +
                                           " FROM[tblWalletUsageMaster] w " +
                                           " WHERE w.is_active = 1 ";
                        DataTable dtusageQry = dbc.GetDataTable(usagequery);
                        //for (int i = 0; i < dtmain.Rows.Count; i++)
                        //{
                        //    string redeemeAmt = dtmain.Rows[i]["BalanceAmount"].ToString();
                        //    objRedeemeWalletdt.RedeemeAmount = redeemeAmt;
                        //}

                        string balAmt = dtmain.Rows[0]["BalanceAmount"].ToString();
                        objRedeemeWalletdt.RedeemeAmount = balAmt;
                        
                        decimal WalletperAmt = Convert.ToDecimal(dtusageQry.Rows[0]["per_amount"]);
                        decimal WalletminOrdAmt = Convert.ToDecimal(dtusageQry.Rows[0]["min_order_amount"]);
                        string Walletpertype = dtusageQry.Rows[0]["per_type"].ToString();

                        objRedeemeWalletdt.MinimumOrderAmount = WalletminOrdAmt.ToString();

                        if (WalletminOrdAmt > Convert.ToDecimal(OrderAmount))
                        {
                            objRedeemeWalletdt.RedeemableAmount = "0";
                            objRedeemeWalletdt.RedeemDetails = "Wallet money can be redeemed only if minimum order amount is more than ₹ " + WalletminOrdAmt;
                        }
                        else
                        {
                            if (Walletpertype == "Fixed")
                            {
                                objRedeemeWalletdt.RedeemableAmount = WalletperAmt.ToString();
                                objRedeemeWalletdt.RedeemDetails = "Per transaction maximum applicable wallet money is ₹ "+ WalletperAmt;
                            }
                            else if(Walletpertype == "%")
                            {
                                decimal redeemPerAmt = (Convert.ToDecimal(balAmt) * WalletperAmt) / 100;
                                objRedeemeWalletdt.RedeemableAmount = redeemPerAmt.ToString("0.00");
                                objRedeemeWalletdt.RedeemDetails = "Per transaction a maximum of " + WalletperAmt + "% of wallet balance can be used.";

                            }
                            else if (Walletpertype == "Full Amount Applicable")
                            {
                                objRedeemeWalletdt.RedeemableAmount = balAmt;
                                objRedeemeWalletdt.RedeemDetails = "Maximum applicable wallet balance has been applied.";

                            }

                        }

                        //For PromoCode
                        string queryPromoCode = " Select  w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, w.terms, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date,  ISNULL((select isnull(H.balance,0) " +
                                        " from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)),0) as Balance " +
                                        " , O.offer_id, O.offer_name  " +
                                        " FROM[tblWalletCustomerLink] WC " +
                                        " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id  " +
                                        " INNER JOIN tblOfferTypes O ON O.offer_id = W.offer_id " +
                                        where +
                                        " AND ISNULL(W.coupon_code,'') != '' " +
                                        " AND  GETDATE() >= W.start_date and GETDATE() <= W.end_date " +
                                        "AND ISNULL(WC.is_used,0) = 0 " +
                                        " AND (0 < (select isnull(H.balance,0) " +
                                        " from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)) " +
                                        " or (select count(*) from[tblWalletCustomerHistory] H " +
                                        " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.wallet_id = WC.wallet_id)) = 0 " +
                                        " ) " +
                                        " Union " +
                                        " SELECT w.wallet_id, W.coupon_code, w.campaign_name, w.wallet_amount, w.terms, " +
                                        " w.per_type, w.per_amount, w.min_order_amount, w.start_date, w.end_date, 0 AS Balance " +
                                        " , O.offer_id, O.offer_name  " +
                                        " FROM[WalletMaster] W " +
                                        " LEFT JOIN tblWalletCustomerLink WC ON WC.wallet_id = W.wallet_id " +
                                        " INNER JOIN tblOfferTypes O ON O.offer_id = W.offer_id " +
                                        " WHERE WC.customer_id = -1 " +
                                        " AND ISNULL(W.coupon_code,'') != '' " +
                                       "AND  GETDATE() >= W.start_date and GETDATE() <= W.end_date " +
                                        "AND ISNULL(WC.is_used,0) = 0 " +
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
                                string offerId = dtPromoCode.Rows[i]["offer_id"].ToString();
                                string offerName = dtPromoCode.Rows[i]["offer_name"].ToString();
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
                                objPromoCode.OfferId = offerId;
                                objPromoCode.OfferName = offerName;
                                objRedeemeWalletdt.PromoCodeList.Add(objPromoCode);
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
                objeWalletdt.WalletId = "";
                objeWalletdt.WalletLinkId = "";
                objeWalletdt.WalletType = "";
                objeWalletdt.CrAmount = "";
                objeWalletdt.CrDate = "";
                objeWalletdt.CrDescription = "";
                objeWalletdt.balance = "";
                objeWalletdt.ValidationMessage = "";
                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {
                    where = "AND [WC].customer_id=" + CustomerId;
                    string querymain = "SELECT Top 1 w.wallet_id,H.wallet_link_id, W.terms AS CR_description, " +
                                       " WC.created_date AS CR_date,campaign_name,wallet_amount, H.balance " +
                    " FROM[tblWalletCustomerHistory] H " +
                    " INNER JOIN[tblWalletCustomerLink] WC ON H.wallet_id = WC.wallet_id " +
                    " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                     where +
                    " AND W.is_active = 1 " +
                    " AND H.balance > 0 " +
                    " AND GETDATE() >=  W.start_date and GETDATE() <= W.end_date " +
                    " Order by H.Id desc";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        string walletId = dtmain.Rows[0]["wallet_id"].ToString();
                        string usagequery = "SELECT w.per_type,w.per_amount,w.min_order_amount " +
                                           " FROM[tblWalletUsageMaster] w " +
                                           " WHERE w.is_active = 1 ";
                                           
                        DataTable dtusageQry = dbc.GetDataTable(usagequery);
                        //for (int i = 0; i < dtmain.Rows.Count; i++)
                        //{
                            string walletlinkId = dtmain.Rows[0]["wallet_link_id"].ToString();
                            decimal walletAmt = Convert.ToDecimal(dtmain.Rows[0]["wallet_amount"]);
                            decimal perAmt = Convert.ToDecimal(dtusageQry.Rows[0]["per_amount"]);
                            decimal minOrdAmt = Convert.ToDecimal(dtusageQry.Rows[0]["min_order_amount"]);
                            string crDate = dtmain.Rows[0]["CR_date"].ToString();
                            string crDescription = dtmain.Rows[0]["CR_description"].ToString();
                            string balance = dtmain.Rows[0]["balance"].ToString();
                            string pertype = dtusageQry.Rows[0]["per_type"].ToString();

                            objeWalletdt.WalletId = walletId;
                            objeWalletdt.WalletLinkId = walletlinkId;
                            objeWalletdt.WalletType = pertype;
                            objeWalletdt.CrAmount = perAmt.ToString();
                            objeWalletdt.CrDate = crDate;
                            objeWalletdt.CrDescription = crDescription;
                            objeWalletdt.balance = balance;

                            if (Convert.ToDecimal(balance) >= Convert.ToDecimal(RedeemeAmount))
                            {
                                if (dtusageQry.Rows[0]["per_type"].ToString() == "Fixed")
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
                                        objeWalletdt.ValidationMessage = "You redeemed ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (dtusageQry.Rows[0]["per_type"].ToString() == "%")
                                {
                                //decimal redeemPerAmt = (walletAmt * perAmt) / 100;
                                decimal redeemPerAmt = (Convert.ToDecimal(balance) * perAmt) / 100;
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
                                        objeWalletdt.ValidationMessage = "You redeemed ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (dtusageQry.Rows[0]["per_type"].ToString() == "Full Amount Applicable")
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
                                        objeWalletdt.ValidationMessage = "You redeemed ₹ " + RedeemeAmount + " successfully for this order.";
                                    }
                                }
                                if (minOrdAmt > Convert.ToDecimal(OrderAmount))
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "Wallet money can be redeemed only if minimum order amount is more than ₹ " + minOrdAmt;
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
                //}
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
                                       " where H.Id = (select max(id) From[tblWalletCustomerHistory] Hi where Hi.customer_id = Wc.customer_id)),0) as Balance, " +
                                       " O.offer_id, O.offer_name" + 
                                       " FROM[tblWalletCustomerLink] WC  " +
                                       " INNER JOIN[WalletMaster] W ON W.wallet_id = WC.wallet_id " +
                                       " INNER JOIN tblOfferTypes O ON O.offer_id = W.offer_id " +
                                       where +
                                       " AND ISNULL(W.coupon_code,'') != '' " +
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
                            string balance = dtmain.Rows[i]["balance"].ToString();
                            string offerId = dtmain.Rows[i]["offer_id"].ToString();
                            string offerName = dtmain.Rows[i]["offer_name"].ToString();
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
                            objeWalletdt.OfferId = offerId;
                            objeWalletdt.OfferName = offerName;
                            if (dtmain.Rows[i]["per_type"].ToString() == "Fixed")
                            {
                                if (calcAmt > perAmt)
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "Coupon code can only be used for order amount more than ₹ " + minOrdAmt;
                                }
                                else if (calcAmt <= perAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    if (offerName == "Discount")
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a discount of ₹ " + calcAmt + " for this order.";
                                    }
                                    else
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a cashback of ₹ " + calcAmt + " in your wallet.";
                                    }
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "%")
                            {
                                decimal redeemPerAmt = (Convert.ToDecimal(OrderAmount) * perAmt) / 100;
                                if (calcAmt > redeemPerAmt)
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "Coupon code can only be used for order amount more than ₹ " + minOrdAmt;
                                }
                                else if (calcAmt <= redeemPerAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    if (offerName == "Discount")
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a discount of ₹ " + calcAmt + " for this order.";
                                    }
                                    else
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a cashback of ₹ " + calcAmt + " in your wallet.";
                                    }
                                }
                            }
                            if (dtmain.Rows[i]["per_type"].ToString() == "Full Amount Applicable")
                            {
                                if (calcAmt > perAmt)
                                {
                                    objeWalletdt.response = CommonString.DataNotFoundResponse;
                                    objeWalletdt.message = CommonString.DataNotFoundMessage;
                                    objeWalletdt.ValidationMessage = "Coupon code can only be used for order amount more than ₹ " + minOrdAmt;
                                }
                                else if (calcAmt <= perAmt)
                                {
                                    objeWalletdt.response = CommonString.successresponse;
                                    objeWalletdt.message = CommonString.successmessage;
                                    if (offerName == "Discount")
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a discount of ₹ " + calcAmt + " for this order.";
                                    }
                                    else
                                    {
                                        objeWalletdt.ValidationMessage = "Congratulation! you got a cashback of ₹ " + calcAmt + " in your wallet.";
                                    }
                                }
                            }
                            if (minOrdAmt > Convert.ToDecimal(OrderAmount))
                            {
                                objeWalletdt.response = CommonString.DataNotFoundResponse;
                                objeWalletdt.message = CommonString.DataNotFoundMessage;
                                objeWalletdt.ValidationMessage = "Coupon code can only be used for order amount more than ₹ " + minOrdAmt;
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
            DateTime dtCreatedon = DateTime.Now;
            try
            {
                objeWalletdt.response = "1";
                objeWalletdt.message = "Successfully";
                objeWalletdt.WalletHistoryList = new List<WalletModel.WalletHistoryList>();

                string WalletMasterQry = " SELECT wallet_id, wallet_amount, campaign_name, offer_id from WalletMaster " +
                                         " WHERE ISNULL(is_apply_all_customer,0) = 1 " +
                                         " AND ISNULL(is_active,0) = 1 " +
                                         " AND GETDATE() >=  start_date and GETDATE() <= end_date ";
                DataTable dtWalletMasterQry = dbc.GetDataTable(WalletMasterQry);
                if (dtWalletMasterQry != null && dtWalletMasterQry.Rows.Count > 0)
                {
                    for (int iCtr = 0; iCtr < dtWalletMasterQry.Rows.Count; iCtr++)
                    {
                        string walletId = dtWalletMasterQry.Rows[iCtr]["wallet_id"].ToString();
                        string walletAmt = dtWalletMasterQry.Rows[iCtr]["wallet_amount"].ToString();
                        string campName = dtWalletMasterQry.Rows[iCtr]["campaign_name"].ToString();
                        string offerId = dtWalletMasterQry.Rows[iCtr]["offer_id"].ToString().TrimEnd();
                        string chkHistoryRecord = " SELECT * FROM tblWalletCustomerHistory " +
                                                  " WHERE customer_id = " + CustomerId +
                                                  " AND wallet_id = " + walletId;

                        string chkLinkRecord = " SELECT * FROM tblWalletCustomerLink " +
                                                  " WHERE customer_id = " + CustomerId +
                                                  " AND wallet_id = " + walletId;
                        DataTable dtHistoryRecord = dbc.GetDataTable(chkHistoryRecord);
                        DataTable dtLinkRecord = dbc.GetDataTable(chkLinkRecord);
                        int linkVAL = 0;
                        if (dtHistoryRecord == null || dtHistoryRecord.Rows.Count == 0)
                        {
                            string balHistory = " SELECT top 1 id,balance From tblWalletCustomerHistory " +
                                       " WHERE customer_id=" + CustomerId +
                                       "AND wallet_id not in("+walletId+")"+
                                       " order by 1 desc";
                            DataTable dtmainBal = dbc.GetDataTable(balHistory);
                            Decimal balAmt = 0;
                            Decimal walletBalance = 0;
                            if (dtmainBal != null && dtmainBal.Rows.Count > 0)
                            {
                                walletBalance = Convert.ToDecimal(dtmainBal.Rows[0]["balance"]);
                                
                            }
                            balAmt = Convert.ToDecimal(walletAmt) + walletBalance;
                            if ((dtHistoryRecord == null || dtHistoryRecord.Rows.Count == 0) && (dtLinkRecord == null || dtLinkRecord.Rows.Count == 0))
                            {
                                string[] para2 = { walletId, CustomerId, "1", dtCreatedon.ToString(), CustomerId };
                                string customerlinkinsertquery = "INSERT INTO [dbo].[tblWalletCustomerLink] ([wallet_id],[customer_id],[is_active],[created_date],[created_by]) VALUES (@1,@2,@3,@4,@5) SELECT SCOPE_IDENTITY();";
                                linkVAL = dbc.ExecuteQueryWithParamsId(customerlinkinsertquery, para2);
                            }
                            if (offerId == "1")
                            {
                                if (dtHistoryRecord == null || dtHistoryRecord.Rows.Count == 0)
                                {
                                    string[] para3 = { walletId, CustomerId, linkVAL.ToString(), dtCreatedon.ToString(), campName, walletAmt.ToString(), "", "", "0", balAmt.ToString(), "1", dtCreatedon.ToString(), CustomerId };
                                    string customerwallethistoryQuery = "INSERT INTO [dbo].[tblWalletCustomerHistory] ([wallet_id],[customer_id],[wallet_link_id],[Cr_date],[Cr_description],[Cr_amount],[Dr_date],[Dr_description],[Dr_amount],[balance],[is_active],[created_date],[created_by]) VALUES (@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13);";
                                    dbc.ExecuteQueryWithParamsId(customerwallethistoryQuery, para3);
                                }
                            }
                        }
                        else
                        {

                        }
                        
                    }

                    //objeWalletdt.WalletBalance = walletBalance;

                }

                string where = "";
                if (CustomerId != "" && CustomerId != null)
                {

                    where = "AND [O].CustomerId=" + CustomerId;

                    
                    string balHistory = " SELECT top 1 id,balance From tblWalletCustomerHistory " +
                                        " WHERE customer_id=" + CustomerId +
                                        " order by 1 desc";
                    DataTable dtmainBal = dbc.GetDataTable(balHistory);
                    if (dtmainBal != null && dtmainBal.Rows.Count > 0)
                    {
                        string walletBalance = dtmainBal.Rows[0]["balance"].ToString();
                        objeWalletdt.WalletBalance = walletBalance;

                    }
                    string querymain = "  SELECT [H].customer_id,CAST(H.Cr_date AS DATE) as Cr_date,CAST(H.Dr_date AS DATE) as Dr_date, " +
                                       " (case isnull(H.order_id, 0) when 0 then isnull(H.Cr_description, '') else 'Order Id ' + CAST(O.Id AS varchar) end) AS OrderId, " +
                                       " H.Cr_amount, H.Dr_amount, H.balance " +
                                       " FROM tblWalletCustomerHistory H " +
                                       " LEFT OUTER JOIN[Order] O ON H.customer_id = O.CustomerId  AND O.Id = H.order_id " +
                                       " WHERE H.customer_id = " + CustomerId +
                                       " order by H.Id desc";
                    //string querymain = " SELECT x.CustomerId, x.Date, X.OrderId, x.Cr_amount, x.Dr_amount, x.balance FROM " +
                    //                    " ( " +
                    //                    " SELECT [O].CustomerId, H.created_date AS[Date], 'Order Id ' + CAST(O.Id AS varchar) AS OrderId, " +
                    //                    " H.Cr_amount, H.Dr_amount, H.balance " +
                    //                    " FROM[Order] O " +
                    //                    " INNER JOIN tblWalletCustomerHistory H ON H.customer_id = O.CustomerId " +
                    //                    " UNION ALL " +
                    //                    " SELECT H.customer_id, H.created_date AS[Date], W.campaign_name AS OrderId, " +
                    //                    " H.Cr_amount, H.Dr_amount, H.balance " +
                    //                    " FROM tblWalletCustomerHistory H " +
                    //                    " LEFT JOIN WalletMaster W ON W.wallet_id = H.wallet_id " +
                    //                    " ) x " +
                    //                    " WHERE X.CustomerId = " + CustomerId +
                    //                     " ORDER BY x.Date desc ";
                    //string querymain = "SELECT H.created_date AS [Date], O.Id AS OrderId, H.Cr_amount, " +
                    //               " H.Dr_amount,H.balance " +
                    //               " FROM[Order] O " +
                    //               " INNER JOIN tblWalletCustomerHistory H ON H.customer_id = O.CustomerId " +
                    //               where +
                    //               " ORDER BY H.created_date desc ";
                    DataTable dtmain = dbc.GetDataTable(querymain);
                    if (dtmain != null && dtmain.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {

                            WalletModel.WalletHistoryList objHistory = new WalletModel.WalletHistoryList();
                            //string date = dtmain.Rows[i]["Date"].ToString();
                            string orderId = dtmain.Rows[i]["OrderId"].ToString();
                            string CrAmt = dtmain.Rows[i]["Cr_amount"].ToString();
                            string DrAmt = dtmain.Rows[i]["Dr_amount"].ToString();
                            string balance = dtmain.Rows[i]["balance"].ToString();
                            string crDate = dtmain.Rows[i]["Cr_date"] is DBNull ? string.Empty : Convert.ToDateTime(dtmain.Rows[i]["Cr_date"]).ToString("dd/MM/yyyy");
                            string drDate = dtmain.Rows[i]["Dr_date"] is DBNull ? string.Empty : Convert.ToDateTime(dtmain.Rows[i]["Dr_date"]).ToString("dd/MM/yyyy");

                            //objHistory.Date = date;
                            objHistory.Summary = orderId;
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
                            if (string.IsNullOrEmpty(crDate))
                            {
                                objHistory.Date = drDate;
                            }
                            else
                            {
                                objHistory.Date = crDate;
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
