using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models.RedeemWallet;

namespace Test0555.Controllers
{
    public class CancelWalletController : ApiController
    {
         dbConnection dbCon = new dbConnection();
         [HttpGet]
         public CancelWalletModel CancelWallet(String CustomerId, String OrderTotal, String Redemeamount = "")
         {
             CancelWalletModel objcancelwallet = new CancelWalletModel();
             try
             {
                  decimal redemAmount = Convert.ToDecimal(Redemeamount);
                  decimal amount = Convert.ToDecimal(OrderTotal);

                  DataTable dtWallet = dbCon.GetDataTable("select customerid,(sum(Cr_Amount)-sum(Dr_Amount))as balance from Customer_Wallet_History where customerid=" + CustomerId + " group by customerid");
                  
                  if (dtWallet != null && dtWallet.Rows.Count > 0)
                  {
                      objcancelwallet.resultflag = "1";
                      objcancelwallet.Message = "";
                      objcancelwallet.Amount = (amount + redemAmount).ToString();
                      objcancelwallet.WalletAmount = "" + dtWallet.Rows[0]["balance"].ToString();
                      objcancelwallet.WalletAmountText = "MONEY IN YOUR WALLET :";
                      objcancelwallet.TotalAmount = (amount + redemAmount).ToString();
                      
                  }
                  else
                  {
                      objcancelwallet.resultflag = "0";
                      objcancelwallet.Message = "Service is not available";
                  }
             }
             catch(Exception ex)
             {

             }
             return objcancelwallet;
         }
    }
}
