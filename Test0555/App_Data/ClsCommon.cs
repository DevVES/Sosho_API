using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
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

        /*For Sending Email*/
        public static string GetEmailBody(string sSource, string sContent)
        {
            StringBuilder sEmailBody = new StringBuilder();

            sEmailBody.Append("<table>");
            sEmailBody.Append("<tr>");
            sEmailBody.Append("<td style=\"font-size:27px;color:cornflowerblue;\">" + sSource + "</td>");
            sEmailBody.Append("</tr>");
            sEmailBody.Append("<tr><td>&#160;</td></tr>");
            sEmailBody.Append(sContent);
            sEmailBody.Append("</table>");
            return sEmailBody.ToString();
        }

        public static bool SendEmailForFranchies(string sTo, string sFrom, string sSubject, string sBody)
        {
            bool functionReturnValue = false;
            dbConnection dbCon = new dbConnection();

            try
            {
                var settingData = " SELECT TOP 1 * FROM tblGeneralSettings ";
                DataTable dtsettingData = dbCon.GetDataTable(settingData);
                if (dtsettingData != null && dtsettingData.Rows.Count > 0)
                {
                    var mail = new System.Net.Mail.MailMessage(dtsettingData.Rows[0]["SMTPUsername"].ToString(), sTo);
                    mail.Subject = sSubject;
                    mail.Body = sBody;
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = dtsettingData.Rows[0]["SMTPServer"].ToString();
                    smtp.EnableSsl = false;
                    NetworkCredential networkCredential = new NetworkCredential(dtsettingData.Rows[0]["SMTPUsername"].ToString(), dtsettingData.Rows[0]["SMTPPassword"].ToString());
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = networkCredential;
                    smtp.Port = Convert.ToInt32(dtsettingData.Rows[0]["portNum"]);
                    smtp.Send(mail);
                }
                functionReturnValue = true;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            return functionReturnValue;
        }

        public static void SendFranchiesEmail(string Email,string Name, string Mobile, string Address, string PinCode)
        {
            dbConnection dbCon = new dbConnection();
            StringBuilder sContent = new StringBuilder();
            string sEmailBody = "";
            var settingData = " SELECT TOP 1 * FROM tblGeneralSettings ";
            DataTable dtsettingData = dbCon.GetDataTable(settingData);
            if (dtsettingData != null && dtsettingData.Rows.Count > 0)
            {
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\">Hello,</td></tr>");
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\"> Inquiry For Franchies </td></tr>");
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\"> Name: "+ Name +"</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\"> Mobile No: "+Mobile+ " </td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\"> Email: " + Email + " </td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\"> Address: " + Address + ", "+ PinCode +" </td></tr>");
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\">Thank You!!</td></tr>");
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td>&#160;</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\">Regards,</td></tr>");

                sContent.Append("<tr><td style=\"color:cornflowerblue;\">" + dtsettingData.Rows[0]["ContactPerson"].ToString() + "</td></tr>");
                sContent.Append("<tr><td style=\"color:cornflowerblue;\">" + dtsettingData.Rows[0]["Designation"].ToString() + "</td></tr>");
                sEmailBody = GetEmailBody("", sContent.ToString());
                SendEmailForFranchies(dtsettingData.Rows[0]["EmailAddress"].ToString(), "", "Inquiry For Franchies", sEmailBody);
            }
        }
    }
}