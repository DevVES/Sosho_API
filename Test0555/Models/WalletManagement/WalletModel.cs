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
            public string coupon_code;
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
            public List<CouponCodeDataList> CouponCodeList { get; set; }

        }

        public class CouponCodeDataList
        {
            public string wallet_id;
            public string campaign_name;
            public string wallet_amount;
            public string coupon_code;
            public string per_type;
            public string per_amount;
            public string min_order_amount;
            public string start_date;
            public string end_date;
        }
    }
}