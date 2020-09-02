using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test0555.Models.Order
{
    public class CouponValidation
    {
        public string OrderTotal;
        public int BuyWith;
        public int ProductId;
    }

    public class CouponValidationInput
    {
        public string couponcode;
    }

    public class NewCouponCode
    {
        public string couponcode;
    }
}