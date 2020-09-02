using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GarageXAPINEW
{
    public class ClsCommon
    {
        #region Props
        dbConnection dbCon = new dbConnection();
        #endregion
        //Local
       // public static string strStoreUrl = "http://192.168.1.113:8089/";
        //Live
        public static string strStoreUrl = "http://api.salebhai.in";
        public static string strVehicleImagesFolderName = "VehicleImages";
        public static string successMessage = "Success";
        public static string successFlag = "1";
        public static string failMessage = "Fail";
        public static string failFlag = "0";

        public static string apiKeyForNotificationCustomer = "AIzaSyChZoSYidjcnjHY56kXv9YR0Qzb2aT0_ZI";
        public static string senderIdForNotificationCustomer = "1055963124498";
        public static string projectName = "sosho_Customer";
        public static string StoreNewRequestNotificationText = "New Product Available Now";
        public static int PickUpAllowedDays = 5;
        //public string getStoreMobileNumberByStoreId(string Id)
        //{
        //    try
        //    {
        //       return dbCon.GetDataTable("SELECT [mobile] FROM [dbo].[Store] where id="+Id).Rows[0][0].ToString();
        //    }
        //    catch (Exception E)
        //    {
        //        return "";
        //    }
        //}
        //public string getCustomerMobileNumberByCustomerId(string Id)
        //{
        //    try
        //    {
        //        return dbCon.GetDataTable("SELECT [mobile] FROM [dbo].[Store] where id=" + Id).Rows[0][0].ToString();
        //    }
        //    catch (Exception E)
        //    {
        //        return "";
        //    }
        //}

        public static int GenerateRandomNumber()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            //int numIterations = 0;
            //numIterations = rand.Next(1, 100);
            //Response.Write(numIterations.ToString());


            //Random rand = new Random();
            return rand.Next(100000, 999999);
            string PasswordLength = "6";
            string NewPassword = "";
            string allowedChars = "";
            allowedChars = "1,2,3,4,5,6,7,8,9";
            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string IDString = "";
            string temp = "";
            Random rand1 = new Random();
            for (int i = 0; i < Convert.ToInt32(PasswordLength); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                IDString += temp;
                NewPassword = IDString;
            }
            int ans = Convert.ToInt32(NewPassword);
            return ans;
        }
    }
}