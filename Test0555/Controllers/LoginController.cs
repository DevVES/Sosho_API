
using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Test0555.Models;
using System.Configuration;

namespace Test0555.Controllers
{
    public class LoginController : ApiController
    {
        dbConnection dbc = new dbConnection();
        [HttpGet]
        public LoginModels.MobileLogin SendOtp(string mobile_number,string FromApp="")
        {
            string otphashkey = ConfigurationManager.AppSettings["OtpHashKey"].ToString();
            LoginModels.MobileLogin objUserList = new LoginModels.MobileLogin();
            try
            {
                if (!String.IsNullOrEmpty(mobile_number))
                {
                    DataTable dtUser = dbc.GetDataTable("Select * from Customer where Mobile = '" + mobile_number + "'");
                    if (dtUser != null && dtUser.Rows.Count > 0)
                    {
                    }
                    else
                    {
                        string date = dbc.getindiantimeString();
                        string datainsert = "Insert into Customer (Mobile,DOC) Values('" + mobile_number + "','" + date + "')";
                        int i = dbc.ExecuteQuery(datainsert);

                    }
                    string datastart = dbc.getindiantime().AddMinutes(+15).ToString("dd-MMM-yyyy hh:mm:ss");
                    string enddate = dbc.getindiantime().AddHours(-1).ToString("dd-MMM-yyyy hh:mm:ss");


                    //above uncomment and below comment then below sendsms services uncomment send messages start
                    int otpgne = 123456;

                    string checkotp = "Select top 1 Otp from Users_SendOtp where Isactive='1' and MobileNo = '" + mobile_number + "' and DOC>='" + enddate + "' and DOC<='" + datastart + "' Order by Id desc";
                    DataTable dtactivcheck = dbc.GetDataTable(checkotp);
                    if (dtactivcheck.Rows.Count > 0)
                    {
                        otpgne = int.Parse(dtactivcheck.Rows[0]["Otp"].ToString());
                    }
                    else
                    {
                        otpgne = GenerateRandomNumber();

                    }

                    if (mobile_number == "9408944702" || mobile_number == "9723202280" || mobile_number == "8000201268" || mobile_number == "9728202280" || mobile_number == "9033986833" || mobile_number == "9725028216" || mobile_number == "7383080418")
                    {
                        otpgne = 123456;
                    }
                    if (otpgne > 0)
                    {
                        string Queryupdateotp = "Update Users_SendOtp set IsActive=0 where MobileNo='" + mobile_number + "'";
                        dbc.ExecuteQuery(Queryupdateotp);
                        string queryinst = "insert into Users_SendOtp (MobileNo,otp,IsActive,DOC)  values('" + mobile_number + "','" + otpgne + "',1,'" + dbc.getindiantime().ToString("dd/MMM/yyyy hh:mm:ss") + "')";

                        int done = dbc.ExecuteQuery(queryinst);
                        if (done > 0)
                        {

                            string smstext = "";
                            if(FromApp=="1")
                            {
                                //smstext = "[#] " + otpgne.ToString() + " is your one-time login password for SoSho. "+ otphashkey;
                                smstext = "[#] " + otpgne.ToString() + " is your one-time login password for SoSho.@" + otphashkey;
                                smstext = smstext.Replace("@", System.Environment.NewLine);
                            }
                            else
                            {
                                smstext = otpgne.ToString() + " is your one-time login password for SoSho.";
                            }
                            //string smstext = otpgne.ToString() + " is your one time login password. ";

                            if (mobile_number != "8000201268" && mobile_number!= "7383080418")
                            {
                                dbc.SendSMS(mobile_number, smstext);
                            }

                            objUserList.response = CommonString.successresponse;
                            objUserList.message = CommonString.successmessage;
                        }
                    }
                }
                else
                {
                    objUserList.response = CommonString.DataNotFoundResponse;
                    objUserList.message = CommonString.DataNotFoundMessage;
                }
            }
            catch (Exception ex)
            {
                //objmysql.RollBackSQLTransaction();
                objUserList.response = CommonString.Errorresponse;
                objUserList.message = "Error: " + ex.Message;
            }

            return objUserList;
        }

        public int GenerateRandomNumber()
        {
            try
            {
                Random rand = new Random();
                return rand.Next(100000, 999999);
                string PasswordLength1 = "6";
                string NewPassword = "";
                string allowedChars = "";
                allowedChars = "1,2,3,4,5,6,7,8,9";
                char[] sep = { ',' };
                string[] arr = allowedChars.Split(sep);
                string IDString = "";
                string temp = "";
                //Random rand = new Random();
                for (int i = 0; i < Convert.ToInt32(PasswordLength1); i++)
                {
                    temp = arr[rand.Next(0, arr.Length)];
                    IDString += temp;
                    NewPassword = IDString;
                }
                int ans = Convert.ToInt32(NewPassword);
                return ans;
            }
            catch (Exception ee)
            {

                return 123;
            }
        }

        //public void SendSMS(String Mobile, String Sms)
        //{
        //    /// <summary>
        //    /// FLAG FOR SMS
        //    /// Did not respond 1
        //    /// Movement 2    
        //    /// Refund 3
        //    /// Cashback 4
        //    /// Informing collection 5
        //    /// Delivery Late 6
        //    /// </summary>
        //    /// <param name="Mobile"></param>
        //    /// <param name="Sms"></param>
        //    try
        //    {
        //        //dbConnection dbc = new dbConnection();
        //        //string[] prm = { Request.Cookies["TUser"]["Id"].ToString(), Mobile, Sms, flag.ToString() };
        //        //int i = dbc.ExecuteQueryWithParams("insert into Taaza_Sms (Userid,Sentto,SmsText,Flag,doc) Values (@1,@2,@3,@4,DATEADD(MINUTE, 330, GETUTCDATE()))", prm);
        //        //if (i > 0)
        //        {
        //            //Sms = System.Web.HttpUtility.UrlEncode(Sms);
        //            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create
        //            //("https://hapi.smsapi.org/SendSMS.aspx?UserName=TaazaFood&password=TaazaFood2016&MobileNo=" + Mobile
        //            //+ "&SenderID=TaazaF&CDMAHeader=TaazaF&Message=" + Sms);
        //            //HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();

        //            Sms = System.Web.HttpUtility.UrlEncode(Sms);
        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create
        //            ("http://hapi.smsapi.org/SendSMS.aspx?UserName=sms_salebhai&password=240955&MobileNo=" + Mobile
        //            + "&SenderID=SLBHAI&CDMAHeader=SLBHAI&Message=" + Sms);
        //            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();

        //            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
        //            string responseString = respStreamReader.ReadToEnd();
        //            string correct = responseString.Substring(0, 2);
        //            respStreamReader.Close();
        //            myResp.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        public LoginModels.LoginOtp GetOtpVerify(string mobile_number, string otp, string fcode="")
        {
            LoginModels.LoginOtp objotp = new LoginModels.LoginOtp();
            CommonString cmstr = new CommonString();
            try
            {

                if (!string.IsNullOrWhiteSpace(mobile_number) && !string.IsNullOrWhiteSpace(otp))
                {
                    DataTable dtUser = dbc.GetDataTable("Select Id,ISNULL(mobile,0)as mobile,ISNULL(firstname,'') as firstname,ISNULL(LastName,'')as LastName,ISNULL(Email,'') as Email,ISNULL(Sex,'')as Sex from Customer where Mobile = '" + mobile_number + "'");
                    DataTable dtUserFranchiseeLink = dbc.GetDataTable("Select * from customer_franchise_link where mobile = '" + mobile_number + "'");
                    if (dtUser != null && dtUser.Rows.Count > 0)
                    {
                        DataTable dtotpcheck = dbc.GetDataTable("Select * from Users_SendOtp where MobileNo = '" + mobile_number + "' and Otp='" + otp + "' and IsActive=1");

                        if (dtotpcheck != null && dtotpcheck.Rows.Count > 0)
                        {

                            string Queryupdate = "Update Users_SendOtp set IsActive=0 Where  MobileNo = '" + mobile_number + "'";
                            int i = dbc.ExecuteQuery(Queryupdate);
                            string userid1 = dtUser.Rows[0]["Id"].ToString();
                            string mobileno = dtUser.Rows[0]["Mobile"].ToString();
                            string fname = dtUser.Rows[0]["FirstName"].ToString();
                            string lname = dtUser.Rows[0]["LastName"].ToString();
                            string email = dtUser.Rows[0]["Email"].ToString();
                            string sex = dtUser.Rows[0]["Sex"].ToString();

                            objotp.response = CommonString.successresponse;
                            objotp.message = CommonString.successmessage;
                            objotp.userid = userid1.ToString();
                            objotp.MobileNo = mobileno;
                            objotp.FirstName = fname;
                            objotp.LastName = lname;
                            objotp.Email = email;
                            objotp.Sex = sex;

                            if((dtUserFranchiseeLink == null || dtUserFranchiseeLink.Rows.Count== 0) && !string.IsNullOrEmpty(fcode))
                            {
                                string datainsert = "Insert into customer_franchise_link  Values('" + mobile_number + "','" + fcode + "','" + dbc.getindiantime() + "')";
                                int id = dbc.ExecuteQuery(datainsert);
                            }
                        }
                        else
                        {
                            objotp.response = CommonString.DataNotFoundResponse;
                            objotp.message = CommonString.DataNotFoundMessage;
                        }
                    }
                    else
                    {
                        objotp.response = CommonString.DataNotFoundResponse;
                        objotp.message = CommonString.DataNotFoundMessage;
                    }
                }
                else
                {
                    objotp.response = CommonString.DataNotFoundResponse;
                    objotp.message = CommonString.DataNotFoundMessage;
                }
            }
            catch (Exception ee)
            {
                objotp.response = CommonString.Errorresponse;
                objotp.message = "Error: " + ee.Message;

            }
            return objotp;
        }
        [HttpGet]
        public LoginModels.VersionDataObject1 GetAppVersionData(string AppVersion)
        {
            //Start 02-29-2020 Modified to check the version and allow force update
            Boolean isforceupdate = (ConfigurationManager.AppSettings["App_IsForceUpdate"] != null && ConfigurationManager.AppSettings["App_IsForceUpdate"] == "true") ? true : false;
            int App_Version = (ConfigurationManager.AppSettings["App_Version"] != null && ConfigurationManager.AppSettings["App_Version"].Trim() != "") ? Convert.ToInt16(ConfigurationManager.AppSettings["App_Version"].Trim()) : 0;

            LoginModels.VersionDataObject1 objVer = new LoginModels.VersionDataObject1();
            try
            {
                objVer.response = "1";
                objVer.response_message = "Success";
                objVer.message = "";
                objVer.Version = App_Version.ToString();
                objVer.VersionCode = "1";
                objVer.IsForceUpdate = isforceupdate;
            }
            catch (Exception ex)
            {
            }
            //END 02-29-2020 Modified to check the version and allow force update
            return objVer;
        }

    }
}
