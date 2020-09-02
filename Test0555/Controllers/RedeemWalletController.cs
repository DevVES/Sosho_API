﻿using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models;

namespace Test0555.Controllers
{
    public class RedeemWalletController : ApiController
    {
        dbConnection dbCon = new dbConnection();
        [HttpGet]
        public RedeemWalletModel RedeemWallet(String CustomerId, String OrderTotal, String Redemeamount = "")
        {
            RedeemWalletModel objredeem = new RedeemWalletModel();
            try
            {
                 decimal redemAmount = Convert.ToDecimal(Redemeamount);
                 decimal amount = Convert.ToDecimal(OrderTotal);
                 if (amount - redemAmount >= 0)
                 {
                     if (amount > 0)
                     {
                         DataTable dtWallet = dbCon.GetDataTable("select customerid,(sum(Cr_Amount)-sum(Dr_Amount)) as balance from Customer_Wallet_History where customerid=" + CustomerId + "  group by customerid");

                         if (dtWallet != null && dtWallet.Rows.Count > 0)
                         {
                             decimal redemeamount = Convert.ToDecimal(dtWallet.Rows[0]["balance"].ToString()) - Convert.ToDecimal(Redemeamount);
                             if (redemeamount < 0)
                             {
                                 objredeem.resultflag = "0";
                                 objredeem.Message = "Amount should be less than or equal to " + dtWallet.Rows[0]["balance"].ToString();
                                 objredeem.Amount =  OrderTotal;
                                 objredeem.WalletAmount = "" + dtWallet.Rows[0]["balance"].ToString();
                                 objredeem.WalletAmountText = "MONEY IN YOUR WALLET :";
                                 objredeem.TotalAmount =  OrderTotal;
                             }
                             else
                             {
                                 decimal finalamount = 0;
                                 finalamount = Convert.ToDecimal(OrderTotal) - Convert.ToDecimal(Redemeamount);
                                 if (finalamount < 0)
                                 {
                                     finalamount = 0;
                                 }
                                 if (finalamount == 0)
                                 {
                                     objredeem.RedeemAmount =  Redemeamount;
                                     objredeem.RedeemAmountText = "(-) Redeemed Amount";
                                 }
                                 else
                                 {
                                     objredeem.RedeemAmount = Redemeamount;
                                     objredeem.RedeemAmountText = "(-) Redeemed Amount";
                                 }
                                 objredeem.resultflag = "1";
                                 objredeem.Message = "You have successfully redeemed Rs. " + Redemeamount + ". Available balance is Rs." + redemeamount;
                                 objredeem.Amount = "" + OrderTotal;
                                 objredeem.WalletAmount = "" + redemeamount;
                                 objredeem.WalletAmountText = "MONEY IN YOUR WALLET :";
                                 objredeem.TotalAmount =  finalamount.ToString();
                             }
                         }
                         else
                         {
                             objredeem.resultflag = "0";
                             objredeem.Message = "Insufficient Wallet Balance";
                         }
                     }
                     else
                     {
                         objredeem.resultflag = "0";
                         objredeem.Message = "Redeem amount can not be more the total amount";
                     }
                 }
                 else
                 {
                     objredeem.resultflag = "0";
                     objredeem.Message = "Please enter valid amount";
                 }
            }
            catch(Exception ex)
            {
                objredeem.resultflag = "0";
                objredeem.Message = "Service is not available";
            }
            return objredeem;
        }

        [HttpGet]
        public AvalilableBalance AvailableBal(String Custid)
        {
            AvalilableBalance objredeem = new AvalilableBalance();
            try
            {

                dbConnection dbc = new dbConnection();

                DataTable dtWallet = dbc.GetDataTable("select CustomerId,(sum(Cr_Amount)-sum(Dr_Amount)) as avlbal,SUM(Dr_Amount)as Usedamt from Customer_Wallet_History where CustomerId=" + Custid + "  group by customerid");
                if (dtWallet.Rows.Count > 0)
                {
                    objredeem.UsedBalance = dtWallet.Rows[0]["Usedamt"].ToString();
                    objredeem.AvalilableBal = dtWallet.Rows[0]["avlbal"].ToString();
                    objredeem.resultflag = "1";
                    objredeem.Message = "Success";
                }
                else
                {
                    objredeem.UsedBalance = "0.00";
                    objredeem.AvalilableBal = "0.00";
                    objredeem.resultflag = "1";
                    objredeem.Message = "Success";
                }
            }
            catch (Exception ex)
            {
                objredeem.resultflag = "0";
                objredeem.Message = "Service is not available";
            }
            return objredeem;
        }
    }
}
