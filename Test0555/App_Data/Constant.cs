using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public static class Constant
    {
        public static class Message
        {
            public static String Registration_Fail = "Fail To Register";
            public static String Default_rate = "57";
            public static String Default_rate_64 = "64";
            public static String Default_rate_69 = "69";
            public static String Default_gram = "400";
            public static String RightSideBannerTitle = "FEATURES";
            public static String NewProductsTitle = "New Products";
            public static String PopularProductTitle = "Popular Products";
            public static String PopularBrandTitle = "Popular Brands";
            public static String TopTenProductTitle = "Top 10 Selling Products";
            public static String UniqueProductTitle = "Unique Products";
            public static String SweetsTitle = "Sweets & Snacks";
            public static String regionalProductsTitle = "Regional Specialities";

            public static String ErrorMessage = "Service is not available";
            public static String NoDataAvail = "No product is available";
            public static String SuccessMessage = "Success";
            public static String CartUpdated = "Cart has been Updated Succesfully";
            public static String FeedBackListMethod_Link = @"https://www.youtube.com/embed/Zj7RWm60Xyk";
            //  public static String AuthoriseToken = "D4B82DC5E75FB2E4BADD7AC32F9FB";
            public static String AuthoriseToken = "F7FA3945AF29AB983A2D5BDF5C78D";
            public static String AuthoriseTokenIos = "A56EAEC98E34EC9988CFC5F386D14";
            //for play store key=//E558FAB2C42A8E89C54C6D7728232
            public static String AuthoriseToken_Version_2 = "E558FAB2C42A8E89C54C6D7728232";
            public static String AuthoriseToken_Version_2_7 = "ED67ACC837D7DFB3C59F7552E525C";
            public static String AuthoriseToken_Version_111 = "8C2CBE4C7FA2CBC7A8AB151641C41";
            public static String AuthoriseToken_Version_211 = "7C8A7FC41CBF33DC6EFDE86D674E7";
            public static String MsgUnAuthorisedUser = "Unauthorised User";
            //public static String MsgAddtoCartSuccessFully = "Add to Cart SuccessFully";
            // public static String MsgAddtoCartFailed = "Not Added to Your Cart";
            public static String MsgWishlistEmpty = "Your Wishlist is empty";
            public static String MsgCartEmpty = "Your Cart is empty";
            public static String MsgAddressUpdateSuccessfully = "Your address has been updated ";
            public static String MsgAddressUpdateFail = "Error in updating your address. Please try again";
            public static String MsgOrderCancelSucess = "Your order has been cancelled";
            public static String MsgOrderCancelFail = "Error in cancelling your order. Please try again";
            public static String MsgWrongPassword = "Password you entered is incorrect";
            public static String MsgCustomerNotExist = "Username or password you entered is incorrect";
            public static String MsgAlreadyRegisteredEmail = "Email you entered already exists";
            public static String MsgAlreadyRegisteredMobile = "Mobile You entered already exists";
            public static String MsgCustomerNotExistRegistered = "Email you entered doesn't exist";
            public static String MsgCustomerSuggestionEmail = "Please Enter Valid Email";
            public static String MsgBundleBlankMessage = "Bundle Is Not Available";
            public static String MsgNewProductsHomepageTitle = "New Arrivals";
            public static String MsgPopularProductHomepageTitle = "Popular Products";
            public static String RecentlyViewProductsTitle = "Recently Viewed";
            // Blank Message 
            public static String BlankMessage = " is Blank";
            public static String EmailBlankMessage = "Email is Blank";
            public static String PasswordBlankMessage = "Password is Blank";
            public static String SENameBlankMessage = "SEName is Blank";
            public static String CustomerIdBlankMessage = "CustomerId is Blank";
            public static String NameBlankMessage = "Name is Blank";
            public static String MobileBlankMessage = "Mobile No is Blank";
            public static String PincodeBlankMessage = "Pincode is Blank";
            public static String productidBlankMessage = "productid is Blank";
            public static String VariantIdBlankMessage = "variantid is Blank";
            public static String quantityBlankMessage = "Minimum 1 quantity is required";
            public static String quantityMaximumMessage = "Maximum 9999 quantity is required";
            public static String vendoridBlankMessage = "vendorid is Blank";
            public static String IndexBlankMessage = "Index is Blank";
            public static String SortByBlankMessage = "SortBy is Blank";
            public static String FilterIdBlankMessage = "FilterId is Blank";
            public static String SubFilterIdBlankMessage = "SubFilterId is Blank";
            public static String MinValueBlankMessage = "MinValue is Blank";
            public static String MaxValueBlankMessage = "MaxValue is Blank";
            public static String PageTypeBlankMessage = "PageType is Blank";
            public static String IdBlankMessage = "Id is Blank";
            public static String BlogCategoryIdBlankMessage = "BlogCategoryId is Blank";
            public static String SearchIdBlankMessage = "SearchId is Blank";
            public static String AddressIdBlankMessage = "AddressId is Blank";
            public static String FirstNameBlankMessage = "FirstName is Blank";
            public static String LastNameBlankMessage = "LastName is Blank";
            public static String TagNameBlankMessage = "TagName is Blank";
            public static String CountryIdBlankMessage = "CountryId is Blank";
            public static String AddressBlankMessage = "Address is Blank";
            public static String stateBlankMessage = "state is Blank";
            public static String cityBlankMessage = "city is Blank";
            public static String OrderIdBlankMessage = "OrderId is Blank";
            public static String TitleBlankMessage = "Title is Blank";
            public static String DescriptionBlankMessage = "Description is Blank";
            public static String RatingBlankMessage = "Rating is Blank";
            public static String NewPasswordBlankMessage = "NewPassword is Blank";
            public static String OldPasswordBlankMessage = "OldPassword is Blank";
            public static String CommentBlankMessage = "Comment is Blank";
            public static String savecardtokenBlankMessage = "savecardtoken is Blank";
            public static String VersionCodeBlankMessage = "VersionCode is Blank";
            public static String HomeTownBlankMessage = "HomeTown is Blank";
            public static String ModelBlankMessage = "Model is Blank";
            public static String ManufacturerBlankMessage = "Manufacturer is Blank";
            public static String DeviceVersionBlankMessage = "DeviceVersion is Blank";
            public static String WidthBlankMessage = "Width is Blank";
            public static String HeightBlankMessage = "Height is Blank";
            public static String AppVersionBlankMessage = "AppVersion is Blank";
            public static String MsgForgotSucessfullEmail = "Password reset information has been sent in email";
            public static String MsgAddtoCartSuccessfully = "Product has been added to cart ";
            public static String MsgAddtoCartFail = "Product has not been added to cart ";
            public static String MsgRemoveFromCartSuccessfully = "Product has been removed from cart";
            public static String MsgRemoveFromCartFail = "Product has not been removed to cart";

            public static String MsgAddtoWishlistSuccessfull = "Product has been added to wishlist";
            public static String MsgAddtoWishlistFail = "Product has not been added to wishlist";
            public static String MsgRemoveFromWishlistSuccessfully = "Product has been removed from wishlist";
            public static String MsgRemoveFromWishlistFail = "Product has not been removed from wishlist";
            public static String DeviceIdBlankMessage = "DeviceId is Blank";
            public static String FCMRegistrationIdBlankMessage = "FCMRegistrationId is Blank";
            public static String IsSecretSantaFlag = "false";
            public static String SecretSantaLink = "CustomerBids";
            public static String SecretSantaTitle = "My Bids";
            public static String SecretSantaFlagv1 = "true";
            public static String SecretSantaLinkV1 = "MySantaGroup";
            public static String SecretSantaTitleV1 = "Secret Santa";
            public static String LoginInvalidEmail = "Invalid Email Id";
            public static String AddReviewSuccessfull = "Thank you for your feedback";
            public static String AddReviewFail = "Feedback not saved. Please try again.";
            public static String PersonalInformationSuccess = "New changes saved";
            public static String PersonalInformationFail = "Could not save new changes. Please try again.";
            public static String InsertNewAddressSuccess = "Address has been added";
            public static String InsertNewAddressFail = "Address has not been added";
            public static String DeleteAddressSuccess = "Address has been removed";
            public static String DeleteAddressFail = "Address has not been removed";
            public static String MsgChangePasswordSuccessfull = "Your password has been changed";
            public static String MsgChangePasswordFail = "Password could not be reset. Please try again.";

            public static String SessionIdBlankMessage = "Session Id Is Blank";
            public static String UtmSourceBlankMessage = "Utm Source is Blank";
            public static String SettingSuccessMessage = "Your settings successfully saved.";
            public static String SettingFailedMessage = "settings doesn't Change";

            public static String DeliveryWithInAhmedabad = "Delivery within Ahmedabad in 24 Hours";
            //public static String DeliveryWithInIndia = "Mumbai delivery in 24 hrs & rest of India in 4-7 working days. Price to vary as per location.";
            public static String DeliveryWithInIndia = "Orders will be dispatched on/after April 2, 2018";
            public static String DeliveryAir = " Delivery within India in 4-7 business days";
            public static String DeliveryAirInternational = "International Delivery in 8-10 business days.";
            public static String DeliveryKesarMessage = "Delivery within India in 4-7 business days ";
            public static String DeliveryBangapaliMessage = "Delivery within India in 4-7 business days <br> Orders to be dispatched from June 8, 2017";
            public static String DeliveryWithBoth = "Delivery within India in 4-7 business days. <br> International Delivery in 8-10 business days. ";
            public static String DeliveryBothKesarMessage = "Delivery within India in 4-7 business days. <br> International Delivery in 8-10 business days. ";
            public static String DeliveryBothBangapaliMessage = "Delivery within India in 4-7 business days. <br> International Delivery in 8-10 business days. <br> Orders to be dispatched from June 8, 2017";


            public static String GuestUserBlankMessage = "GuestUserId is Blank";

            public static String CategoryNameBlankMessage = "Enter Category Name";
            public static String CategoryNameWrongMessage = "Invalid Category Name";
            public static String LinkBlankMessage = "Link is Blank";
            public static String SubOrderIdBlankMessage = "SubOrderId is Blank";

            public static String NotificationFlagBlank = "NotificationFlag is blank";
            public static String SmsFlagBlankMessage = "SmsFlag is blank";
            public static String EmailFlagBlankMessage = "EmailFlag is blank";
            public static String CommentIdBlankMessage = "CommentId is Blank";
            public static String LikeValueBlankMessage = "LikeValue is Blank";
            public static String DisLikeValueBlankMessage = "DislikeValue is Blank";
            public static String SendOtpTextMessage = " is Your SaleBhai OTP.";
            public static String CustomerRoleNotDefineMessage = "Customer Role is not Generated";

            public static String HomePageBannerNotAvailable = "Homepage banner Service is not available";
            public static String RecommendedBundleTitle = "Recommended Bundles";
            public static String InternationalRecommendedBundle = "Combos";
            public static int IndiaCountryCode = 41;
            public static String ShippingHint = "Shipping charges will be less when purchasing in bulk from same vendor";
            public static String SchemeMessage = "This Offer is Restricted to first-time users only";
            public static String SchemeMessage1 = "Allowed Quantity for this product :1";


            public static String UpdateCartFail = "Cart has not been Updated Successfully";
            public static String MyCardNoCard = "No cards saved";
            public static String RemoveCardSuccess = "Card Removed Successfully";
            public static String RemoveCardFail = "Failed";

            public static String PopularBrandImageUrl = "http://www.salebhai.com/Content/images/thumbs/popular-brand-logo.png";
            public static String PopularProductImageUrl = "http://www.salebhai.com/Content/Images/Thumbs/popular-produts.png";
            public static String UniqueProductsImageUrl = "http://www.salebhai.com/Content/images/thumbs/popular-brand-logo.png";
            public static String RegionalProductImageUrl = "http://www.salebhai.com/Content/images/thumbs/popular-brand-logo.png";

            public static String UniqueProductsImageUrlV1 = "http://www.salebhai.com//Themes/salebhai/Content/images/Unique_Product_Icon.png";
            public static String RegionalProductImageUrlV1 = "http://www.salebhai.com//Themes/salebhai/Content/images/Regional_Specialities_Icon.png";

            public static String ReorderTitle = "Your order history is awesome. How about a repeat?";
            //  public static String ReorderImageUrl = "http://www.salebhai.com/Content/Images/Thumbs/reorder_icon.png";
            public static String ReorderImageUrl = "http://www.salebhai.com//Themes/salebhai/Content/images/home-reorder.png";
            public static String CartListImageUrl = "http://www.salebhai.com//Themes/salebhai/Content/images/home-cart.png";
            public static String HomePageBlogTitle = "Our Blog";
            public static String HomePageBlogImageUrl = "https://d1iqctulejj45h.cloudfront.net/0023845.png";

            public static String QueueEmailToAddress = "tejas.patel@xpditesolutions.com";
            //public static String QueueEmailCCAddress = "jaydeep.joshi.203599@gmail.com";
            public static String QueueEmailCCAddress = "tejas.patel@xpditesolutions.com";

            public static String CustomerProductSuggestionTitle = "Didn't find something you wanted? Tell us about it and help us make a better pool of your favourites.";
            //public static String CustomerProductSuggestionSuccess = "Your suggestion has been received.";
            public static String CustomerProductSuggestionSuccess = "Thank you. You will soon receive an email from us with a link to confirm your subscription.";
            public static String CustomerProductSuggestionFail = "Your suggestion has not been send.";
            public static String CustomerOtpInvalid = "OTP you entered is incorrect";
            public static String SimilarProductsTitle = "Similar Products";

            public static String NewsLetterSubScribeToAddress = "service@salebhai.com";
            public static String CommentToAddress = "";

            public static String InsertBlogCommentTextSuccessfully = "Thanks! Your comment will be published after review.";
            public static String InsertBlogCommentTextFail = "Sorry! Your Comment is not added";

            public static String IndianRupeesSymbol = "₹";
            public static String IsOtpRequired = "1";

            #region International Filter
            public static String InternationalFilterShowAll = "Show All";
            public static String InternationalFilterWithInIndia = "Within India";
            public static String InternationalFilterInternational = "International";

            public static String FilterInternationalTitle = "PRODUCT AVAILABILITY";
            public static String PopularBrandButtonText = "Details";

            public static String Version1_1 = "1.1";
            public static String Version2 = "2.0";
            public static String Version1 = "1.0";
            public static String Version2_1 = "2.1";
            public static String Version2_2 = "2.2";
            public static String Version2_3 = "2.3";
            public static String Version2_4 = "2.4";
            public static String Version2_5 = "2.5";
            public static String Version2_6 = "2.6";
            public static String Version2_7 = "2.7";
            public static String Version2_8 = "2.8";
            public static String Version2_9 = "2.9";
            public static String Version3_0 = "3.0";
            public static String Version3_1 = "3.1";
            public static String Version3_2 = "3.2";
            public static String Version3_3 = "3.3";
            public static String Version3_4 = "3.4";
            public static String Version3_5 = "3.5";
            public static String Version_111 = "111";
            public static String Version_211 = "211";

            public static String Wallet_TC_Link = "<a style='color:#ff5722' href='https://www.salebhai.com/mast-may-loyalty-days-offer-tc' target='_blank'>Loyalty Day May 2019 | T&C</a>";
            //public static String Wallet_TC_Link = "<a style='color:#ff5722' href='https://www.salebhai.com/awesome-april-2018-loyalty-day-tc' target='_blank'>Loyalty Day April 2018 | T&C</a>";
            //Wallet_TC_Condition
            // public static String Wallet_TC_Link = "<a style='color:#ff5722' href='http://www.salebhai.com/loyalty-day-tc-15-16-november' target='_blank'>#For loyalty day T&Cs, click here.</a>";
            public static String UniqueProductButtonText = "";
            //public static String PopularBrandButtonText = "Details";
            public static String regionalproductbuttontext = "View Now";
            public static String CustomerSuggestionButton = "Tell Us";
            public static String HandpickedProductsTitle = "Hand-picked For You";
            public static String IsCommentVisible = "false";
            #endregion

            #region Wallet Keys

            //public static String PG_ReturnURL_Paym = "http://apptest.salebhai.com/SalebhaiPaymentResponsePaytm.aspx";
            //public static String PG_ReturnURL_Mobikwik = "http://apptest.salebhai.com/SalebhaiPaymentResponseMobikwik.aspx";
            //public static String PG_ReturnURL_PhonePe = "http://apptest.salebhai.com/SalebhaiPaymentResponsePhonePe.aspx";
            //public static String PG_ReturnURL_Freecharge = "http://apptest.salebhai.com/SalebhaiPaymentResponseFreecharge.aspx";
            //public static String PG_ReturnURL_BillDesk = "http://apptest.salebhai.com/SalebhaiPaymentResponseBillDesk.aspx";
            //public static String PG_ReturnURL_Freecharge_Fail = "http://apptest.salebhai.com/SalebhaiPaymentResponseFreechargeFail.aspx";
            //public static String PG_ReturnURL_AmazonPay = "http://apptest.salebhai.com/SalebhaiPaymentResponseamazonPay.aspx";

            public static String PG_ReturnURL_Paym = "http://salebhai.in/SalebhaiPaymentResponsePaytm.aspx";
            public static String PG_ReturnURL_Mobikwik = "http://salebhai.in/SalebhaiPaymentResponseMobikwik.aspx";
            public static String PG_ReturnURL_PhonePe = "http://salebhai.in/SalebhaiPaymentResponsePhonePe.aspx";
            public static String PG_ReturnURL_Freecharge = "http://salebhai.in/SalebhaiPaymentResponseFreecharge.aspx";
            public static String PG_ReturnURL_BillDesk = "http://salebhai.in/SalebhaiPaymentResponseBillDesk.aspx";
            public static String PG_ReturnURL_Freecharge_Fail = "http://salebhai.in/SalebhaiPaymentResponseFreechargeFail.aspx";
            public static String PG_ReturnURL_AmazonPay = "http://salebhai.in/SalebhaiPaymentResponseamazonPay.aspx";


            //public static String PG_ReturnURL_Paym = "http://localhost:3152/SalebhaiPaymentResponsePaytm.aspx";
            //public static String PG_ReturnURL_Mobikwik = "http://localhost:3152/SalebhaiPaymentResponseMobikwik.aspx";
            //public static String PG_ReturnURL_Freecharge = "http://localhost:3152/SalebhaiPaymentResponseFreecharge.aspx";
            //public static String PG_ReturnURL_Freecharge_Fail = "http://localhost:3152/SalebhaiPaymentResponseFreechargeFail.aspx";
            //public static String PG_ReturnURL_BillDesk = "http://localhost:3152/SalebhaiPaymentResponseBillDesk.aspx";
            //public static String PG_ReturnURL_PhonePe = "http://localhost:3152/SalebhaiPaymentResponsePhonePe.aspx";

            #region PAYTM

            #region LIVE
            public static string PAYTM_MID = "SaleBh86906809247506";
            public static string PAYTM_Industry_Type = "Retail110";
            public static string PAYTM_merchantKey = "Se2eLdySraE&l_Vy";
            public static string PAYTM_URL = "https://secure.paytm.in/oltp-web/processTransaction?orderid=";
            public static string PAYTM_website = "SaleBhaiweb";
            public static string PAYTM_channel_id = "WEB";

            #endregion

            #region TEST KEY
            public static string PAYTM_MID_test = "salebh02233498843314";
            public static string PAYTM_Industry_Type_test = "Retail";
            public static string PAYTM_merchantKey_test = "QJB&vgos0rQFvILY";
            public static string PAYTM_URL_test = "https://pguat.paytm.com/oltp-web/processTransaction?orderid=";
            public static string PAYTM_website_test = "salebhweb";


            #endregion

            #endregion

            #region MOBIKWIK

            #region TEST

            public static String Mer_Id_test = "MBK9002";
            public static String secretKey_test = "ju6tygh7u7tdg554k098ujd5468o";
            public static String Mer_Name_test = "mobikwik";
            public static string MobikwikURL_test = "https://test.mobikwik.com/wallet";

            #endregion

            #region LIVE

            public static String secretKey = "oCGlWYFyeSzuEabKfamimyGVHP9E";
            public static String Mer_Name = "SaleBhai";
            public static String Mer_Id = "MBK7769";
            public static string MobikwikURL = "https://www.mobikwik.com/wallet";

            #endregion

            #endregion
            #endregion

            #region PaymentUrl
            public static String PaymentUrl = "http://search.salebhai.com/Payment.aspx?txnId=";
            #endregion
        }
    }
}