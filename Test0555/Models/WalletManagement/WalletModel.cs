using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.WalletManagement
{
    public class WalletModel
    {
        public class getWalletDetail
        {
            public string response;
            public string message;
            public List<WalletDataList> WalletList { get; set; }

        }

        public class WalletDataList
        {
            public string wallet_id;
            public string campaign_name;
            public string wallet_amount;
            public string PromoCode;
            public string is_applicable_first_order;
            public string per_type;
            public string per_amount;
            public string min_order_amount;
            public string start_date;
            public string end_date;
        }

        public class RedeemeWallet
        {
            public string response;
            public string message;
            public string RedeemeAmount;
            public List<PromoCodeDataList> PromoCodeList { get; set; }
            //public List<CashbackDataList> CashbackList { get; set; }

        }

        public class PromoCodeDataList
        {
            public string wallet_id;
            public string campaign_name;
            public string PromoCode;
            public string per_type;
            public string per_amount;
            public string min_order_amount;
            public string balance;
            public string start_date;
            public string end_date;
            public string terms;
            public string OfferId;
            public string OfferName;
        }

        //public class CashbackDataList
        //{
        //    public string wallet_id;
        //    public string campaign_name;
        //    public string per_type;
        //    public string per_amount;
        //    public string min_order_amount;
        //    public string balance;
        //    public string start_date;
        //    public string end_date;
        //}

        public class RedeemeWalletFromOrder
        {
            public string response;
            public string message;
            public string WalletId;
            public string WalletLinkId;
            public string WalletType;
            public string CrAmount;
            public string CrDate;
            public string CrDescription;
            public string balance;
            public string ValidationMessage;
        }

        public class RedeemePromoCodeFromOrder
        {
            public string response;
            public string message;
            public string PromoCodeId;
            public string PromoCodeLinkId;
            public string PromoCodetype;
            public string PromoCodeCalcAmount;
            public string PromoCodeCrAmount;
            public string PromoCodeCrDate;
            public string PromoCodeCrDescription;
            public string OfferId;
            public string OfferName;
            public string PromoCodebalance;
            public string ValidationMessage;
        }

        public class getWalletHistory
        {
            public string response;
            public string message;
            public string WalletBalance;
            public List<WalletHistoryList> WalletHistoryList { get; set; }

        }

        public class WalletHistoryList
        {
            
            public string Date;
            public string Summary;
            public string type;
            public string CrDrAmount;
            public string Balance;

        }

    }
}