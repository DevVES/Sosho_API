using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using GarageXAPINEW.Models;
using System.Web.Http.Controllers;
using InquiryManageAPI.Controllers;

namespace GarageXAPINEW.Controllers
{
    public class NotificationController : ApiController
    {
        #region Props
        dbConnection dbCon = new dbConnection();
        #endregion

        [HttpGet]
        public LocationModel.clsCommonResponce RegisterDeviceDetailCustomer(String deviceid, String FCMRegistrationid, String mobilenumber, string isIOS, string UserId, string AppVersion)
        {
            LocationModel.clsCommonResponce data = new LocationModel.clsCommonResponce();
            try
            {

                if (isIOS != "" && isIOS != null && isIOS == "ANDROID")
                {
                    isIOS = "0";
                }

                //string str = HttpRequestContext(request.GetRequestContext().VirtualPathRoot);
                string[] param = { mobilenumber, deviceid };
                DataTable dt = dbCon.GetDataTableWithParams("select * from [dbo].[DevicedetailsCustomer] where mobilenumber=@1 and DeviceId=@2", param);
                if (dt.Rows.Count > 0)
                {
                    string[] param1 = { FCMRegistrationid, mobilenumber, deviceid };
                    int i = dbCon.ExecuteQueryWithParams("Update  [dbo].[DevicedetailsCustomer] set [FCMRegistrationId]=@1,dom=DATEADD(MINUTE, 330, GETUTCDATE()), IsIOS = " + isIOS + "  where [mobilenumber]=@2 and [DeviceId]=@3", param1);
                    if (i > 0)
                    {
                        data.Message = "Success";
                        data.ResultFlag = "1";
                    }
                    else
                    {
                        data.Message = "Fail";
                        data.ResultFlag = "0";
                    }
                }
                else
                {
                    string[] param2 = { deviceid, FCMRegistrationid, mobilenumber, isIOS };
                    int i = dbCon.ExecuteQueryWithParams("INSERT INTO [dbo].[DevicedetailsCustomer] ([DeviceId],[FCMRegistrationId],mobilenumber,[doc],dom,IsIOS) VALUES (@1,@2,@3,DATEADD(MINUTE, 330, GETUTCDATE()) ,DATEADD(MINUTE, 330, GETUTCDATE()),@4)", param2);
                    if (i > 0)
                    {
                        data.Message = "Success";
                        data.ResultFlag = "1";
                    }
                    else
                    {
                        data.Message = "Fail";
                        data.ResultFlag = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                WCL(" Err:" + ex.Message + "::::::" + ex.StackTrace);
            }
            return data;
        }

        [HttpGet]
        public bool SendCustomerNotificatio(String Mobile, String Message = "", String ImgUrl = "", String UserId = "", String RequestId = "")
        {
            int batchid = 55; string type = "1"; int sendtype = 0;
            string applicationID = ClsCommon.apiKeyForNotificationCustomer;
            string SENDER_ID = ClsCommon.senderIdForNotificationCustomer;
            return SendImageNotification(Mobile, batchid, type, sendtype, applicationID, SENDER_ID, Message, ImgUrl, UserId, RequestId);
        }

        //type ={ image/text /update} //sendtype={1= One by one /  0= bulk}
        public bool SendImageNotification(String Mobile, int batchid, string type, int sendtype, string applicationID, string SENDER_ID, String Message = "", String ImgUrl = "", String UserId = "", String RequestId = "", bool isCustomer = true)
        {
            Boolean res = false;
            //string[] param = { Mobile };
            string strTaaza_Notification_Detail = string.Empty;
            string Devicedetails = string.Empty;
            if (isCustomer)
            {
                strTaaza_Notification_Detail = "_Customer";
                Devicedetails = "Customer";
            }
            try
            {
                //  WCL("SendImageNotification() 5228 call BatchId:" + batchid + " MobileNo:" + Mobile + " Msg:" + Message + " ImgUrl:" + ImgUrl);
                if (!String.IsNullOrWhiteSpace(Mobile))
                {
                    bool isAll = (Mobile.ToLower().Contains("all") ? true : false);
                    String Str = "";
                    String mobileNo = "";
                    if (isAll)
                    {
                        Str = "select * from [dbo].[Devicedetails" + Devicedetails + "]";
                    }
                    else
                    {
                        string[] parts = Mobile.Split(',');
                        if (parts.Length > 0)
                        {
                            for (int i = 0; i < parts.Length; i++)
                            {
                                mobileNo += "'" + parts[i] + "',";
                            }
                        }
                        mobileNo = mobileNo.TrimEnd(',');
                        Str = "select isnull(deviceid,'') as deviceid,isnull(fcmregistrationid,'') as fcmregistrationid ,mobilenumber,[doc],dom,Id from [dbo].[Devicedetails" + Devicedetails + "] where mobilenumber in(" + mobileNo + ")";
                    }
                    DataTable dt = dbCon.GetDataTable(Str);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (ImgUrl != null && ImgUrl != "")
                        {
                            int a = 0;
                            if (sendtype == 0) //Bulk Notification
                            {
                                int c = 0;
                                List<string> lis = new List<string>();
                                DataTable dttemp = dt.Clone();
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    DataRow dr = dt.Rows[i];
                                    if (!String.IsNullOrWhiteSpace(dr["fcmregistrationid"].ToString()) && !String.IsNullOrWhiteSpace(dr["deviceid"].ToString()))
                                    {
                                        lis.Add(dr["fcmregistrationid"].ToString());
                                        a++;
                                        c++;
                                        dttemp.ImportRow(dr);
                                    }
                                    if (c == 10)
                                    {
                                        String response = SendPushNotification((isCustomer ? "Customer" : "Store"), lis, Message, type, batchid, applicationID, SENDER_ID, ImgUrl, UserId, RequestId);


                                        c = 0;

                                        BulkNotificationResponse obj = jsonToDataTableNotify(response);

                                        List<string> paramlist = new List<string>();
                                        StringBuilder sb = new StringBuilder();
                                        int count = 0;

                                        sb.Append("INSERT INTO [dbo].[Taaza_Notification_Detail" + strTaaza_Notification_Detail + "] ([DeviceId],[FCMRegistrationId],[Message],[ResponseId],[Type],[DOC],[Mobile],[Batchmasterid]) VALUES ");



                                        for (int k = 0; k < dttemp.Rows.Count; k++)
                                        {
                                            DataRow dr1 = dttemp.Rows[k];
                                            string ReturnMessageId = "0";

                                            if (obj.results[k].message_id != null)
                                            {
                                                string[] resulsplit = obj.results[k].message_id.Split(':');
                                                ReturnMessageId = resulsplit[1].ToString();
                                            }

                                            sb.Append("(");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["deviceid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["fcmregistrationid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(Message);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(ReturnMessageId);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(type);

                                            sb.Append("dateadd(minute, 330, getutcdate()),");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["mobilenumber"].ToString());

                                            count++;
                                            sb.Append("@" + count);
                                            paramlist.Add(batchid.ToString());

                                            sb.Append("),");
                                        }
                                        string qry = sb.ToString().TrimEnd(new char[] { ',' }) + ";";
                                        string[] parm = paramlist.Select(j => j.ToString()).ToArray();
                                        int i2 = dbCon.ExecuteQueryWithParams(qry, parm);



                                        lis = new List<string>();
                                        dttemp.Clear();
                                    }
                                    else if (a == dt.Rows.Count)
                                    {
                                        String response = SendPushNotification((isCustomer ? "Customer" : "Store"), lis, Message, type, batchid, applicationID, SENDER_ID, ImgUrl, UserId, RequestId);

                                        BulkNotificationResponse obj = jsonToDataTableNotify(response);

                                        List<string> paramlist = new List<string>();
                                        StringBuilder sb = new StringBuilder();
                                        int count = 0;

                                        sb.Append("INSERT INTO [dbo].[Taaza_Notification_Detail" + strTaaza_Notification_Detail + "] ([DeviceId],[FCMRegistrationId],[Message],[ResponseId],[Type],[DOC],[Mobile],[Batchmasterid]) VALUES ");



                                        for (int k = 0; k < dttemp.Rows.Count; k++)
                                        {
                                            DataRow dr1 = dttemp.Rows[k];
                                            string ReturnMessageId = "0";

                                            if (obj.results[k].message_id != null)
                                            {
                                                string[] resulsplit = obj.results[k].message_id.Split(':');
                                                ReturnMessageId = resulsplit[1].ToString();
                                            }


                                            sb.Append("(");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["deviceid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["fcmregistrationid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(Message);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(ReturnMessageId);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(type);

                                            sb.Append("dateadd(minute, 330, getutcdate()),");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["mobilenumber"].ToString());

                                            count++;
                                            sb.Append("@" + count);
                                            paramlist.Add(batchid.ToString());

                                            sb.Append("),");
                                        }
                                        string qry = sb.ToString().TrimEnd(new char[] { ',' }) + ";";
                                        string[] parm = paramlist.Select(j => j.ToString()).ToArray();
                                        int i2 = dbCon.ExecuteQueryWithParams(qry, parm);

                                        c = 0;
                                        lis = new List<string>();
                                        dttemp.Clear();
                                    }

                                }
                            }
                            else // Individual Notification
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (!String.IsNullOrWhiteSpace(dr["fcmregistrationid"].ToString()) && !String.IsNullOrWhiteSpace(dr["deviceid"].ToString()))
                                        NotifyDevice(dr["fcmregistrationid"].ToString(), Message, type + "", dr["deviceid"].ToString(), batchid, applicationID, SENDER_ID, dr["mobilenumber"].ToString(), ImgUrl, UserId, RequestId, isCustomer: isCustomer);
                                    else
                                    {
                                        WCL("SendImageNotification() 5367: No Device found for this mobile no:" + Mobile);
                                        // Insert Log 
                                    }
                                }
                            }
                        }
                        else
                        {
                            WCL("SendImageNotification() 5374 TotalRecordFound:" + dt.Rows.Count);
                            int a = 0;
                            if (sendtype == 0) //Bulk Notification
                            {
                                int c = 0;
                                List<string> lis = new List<string>();
                                DataTable dttemp = dt.Clone();
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    DataRow dr = dt.Rows[i];
                                    if (!String.IsNullOrWhiteSpace(dr["fcmregistrationid"].ToString()) && !String.IsNullOrWhiteSpace(dr["deviceid"].ToString()))
                                    {
                                        lis.Add(dr["fcmregistrationid"].ToString());
                                        a++;
                                        c++;
                                        dttemp.ImportRow(dr);
                                    }
                                    if (c == 10)
                                    {
                                        String response = SendPushNotification((isCustomer ? "Customer" : "Store"), lis, Message, type, batchid, applicationID, SENDER_ID, ImgUrl, UserId, RequestId);


                                        c = 0;

                                        BulkNotificationResponse obj = jsonToDataTableNotify(response);

                                        List<string> paramlist = new List<string>();
                                        StringBuilder sb = new StringBuilder();
                                        int count = 0;

                                        sb.Append("INSERT INTO [dbo].[Taaza_Notification_Detail" + strTaaza_Notification_Detail + "] ([DeviceId],[FCMRegistrationId],[Message],[ResponseId],[Type],[DOC],[Mobile],[Batchmasterid]) VALUES ");



                                        for (int k = 0; k < dttemp.Rows.Count; k++)
                                        {
                                            DataRow dr1 = dttemp.Rows[k];
                                            string ReturnMessageId = "0";

                                            if (obj.results[k].message_id != null)
                                            {
                                                string[] resulsplit = obj.results[k].message_id.Split(':');
                                                ReturnMessageId = resulsplit[1].ToString();
                                            }

                                            sb.Append("(");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["deviceid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["fcmregistrationid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(Message);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(ReturnMessageId);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(type);

                                            sb.Append("dateadd(minute, 330, getutcdate()),");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["mobilenumber"].ToString());

                                            count++;
                                            sb.Append("@" + count);
                                            paramlist.Add(batchid.ToString());

                                            sb.Append("),");
                                        }
                                        string qry = sb.ToString().TrimEnd(new char[] { ',' }) + ";";
                                        string[] parm = paramlist.Select(j => j.ToString()).ToArray();
                                        int i2 = dbCon.ExecuteQueryWithParams(qry, parm);



                                        lis = new List<string>();
                                        dttemp.Clear();
                                    }
                                    else if (a == dt.Rows.Count)
                                    {
                                        String response = SendPushNotification((isCustomer ? "Customer" : "Store"), lis, Message, type, batchid, applicationID, SENDER_ID, ImgUrl, UserId, RequestId);

                                        BulkNotificationResponse obj = jsonToDataTableNotify(response);

                                        List<string> paramlist = new List<string>();
                                        StringBuilder sb = new StringBuilder();
                                        int count = 0;

                                        sb.Append("INSERT INTO [dbo].[Taaza_Notification_Detail" + strTaaza_Notification_Detail + "] ([DeviceId],[FCMRegistrationId],[Message],[ResponseId],[Type],[DOC],[Mobile],[Batchmasterid]) VALUES ");



                                        for (int k = 0; k < dttemp.Rows.Count; k++)
                                        {
                                            DataRow dr1 = dttemp.Rows[k];
                                            string ReturnMessageId = "0";

                                            if (obj.results[k].message_id != null)
                                            {
                                                string[] resulsplit = obj.results[k].message_id.Split(':');
                                                ReturnMessageId = resulsplit[1].ToString();
                                            }

                                            sb.Append("(");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["deviceid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["fcmregistrationid"].ToString());

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(Message);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(ReturnMessageId);

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(type);

                                            sb.Append("dateadd(minute, 330, getutcdate()),");

                                            count++;
                                            sb.Append("@" + count + ",");
                                            paramlist.Add(dr1["mobilenumber"].ToString());

                                            count++;
                                            sb.Append("@" + count);
                                            paramlist.Add(batchid.ToString());

                                            sb.Append("),");
                                        }
                                        string qry = sb.ToString().TrimEnd(new char[] { ',' }) + ";";
                                        string[] parm = paramlist.Select(j => j.ToString()).ToArray();
                                        int i2 = dbCon.ExecuteQueryWithParams(qry, parm);
                                        c = 0;
                                        lis = new List<string>();
                                        dttemp.Clear();
                                    }

                                }
                            }
                            else // Individual Notification
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    //testing jd
                                    if (!String.IsNullOrWhiteSpace(dr["fcmregistrationid"].ToString()) && !String.IsNullOrWhiteSpace(dr["deviceid"].ToString()))
                                        NotifyDevice(dr["fcmregistrationid"].ToString(), Message, type, dr["deviceid"].ToString(), batchid, applicationID, SENDER_ID, dr["mobilenumber"].ToString(), UserId: UserId, RequestId: RequestId, isCustomer: isCustomer);
                                    else
                                    {
                                        WCL("SendImageNotification() 5381: No Device found for this mobile no:" + Mobile);
                                        // Insert Log 
                                    }
                                }
                            }
                        }
                        // }

                        res = true;
                    }
                    else
                    {
                        WCL("SendImageNotification() 5392: No UserFound Query:" + Str);
                    }
                }
                else
                {
                    WCL("SendImageNotification() 5397: Mobileno or imgurl not proper Mobile:" + Mobile + "  Imgurl:" + ImgUrl);
                }
                return res;
            }
            catch (Exception ex)
            {
                WCL("SendImageNotification() 5401 call MobileNo:" + Mobile + " Msg:" + Message + " ImgUrl:" + ImgUrl + " \nException:" + ex.Message + " ::::::::: " + ex.StackTrace);
                //   dbCon.InsertLogs(LOGS.LogLevel.Error, "SendImageNotification Error", "SendImageNotification() 3897 call Msg:" + Message + " ImgUrl:" + ImgUrl + " \nException:" + ex.Message + " ::::::::: " + ex.StackTrace, "-10");
                return false;
            }
        }
        public BulkNotificationResponse jsonToDataTableNotify(string json)
        {
            DataTable dt = new DataTable();
            BulkNotificationResponse m = JsonConvert.DeserializeObject<BulkNotificationResponse>(json);
            return m;
        }
        [HttpGet]
        public string SendPushNotification(string SendType, List<string> regID, string message, string type, int batchId, string applicationID, string SENDER_ID, string ImageUrl = "", String UserId = "", String RequestId = "")
        {
            string stringregIds = null;

            List<string> regIDs = regID;

            stringregIds = string.Join("\",\"", regIDs);

            try
            {
                string apiKey = applicationID;
                var senderId = SENDER_ID;
                var value = message;
                int notificationId = 0;
                WebRequest tRequest;
                tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = " application/json";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", apiKey));
                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                //string postData =
                //    "{\"collapse_key\":\"taazafood\",\"time_to_live\":108,\"delay_while_idle\":true,\"data\": {\"category\" : " +
                //    "\"" + category + "\",\"notificationId\" : " +
                //    "\"" + notificationId + "\",\"type\" : " +
                //    "\"" + type + "\",\"header\" : " +
                //    "\"" + "Taaza Food" + "\",\"mobile\" : " +
                //    "\"" + stringmobileNo + "\", \"message\" : " +
                //    "\"" + value + "\",\"time\": " + "\"" + System.DateTime.Now.ToString() +
                //    "\"},\"registration_ids\":[\"" + stringregIds + "\"]}";
                string postData =
                    "{\"collapse_key\":\"" + ClsCommon.projectName + "_" + SendType + "\",\"time_to_live\":108,\"delay_while_idle\":true,\"data\": {\"UserId\" : " +
                    "\"" + UserId + "\",\"RequestId\" : " +
                    "\"" + RequestId + "\",\"notificationId\" : " +
                    "\"" + notificationId + "\",\"type\" : " +
                    "\"" + type + "\",\"header\" : " +
                    "\"" + ClsCommon.projectName + "\",\"imgurl\" : " +
                    "\"" + ImageUrl + "\",\"message\" : " +
                    "\"" + value + "\",\"time\": " + "\"" + System.DateTime.Now.ToString() +
                    "\"},\"registration_ids\":[\"" + stringregIds + "\"]}";

                Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                tRequest.ContentLength = byteArray.Length;
                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse tResponse = tRequest.GetResponse();
                dataStream = tResponse.GetResponseStream();
                StreamReader tReader = new StreamReader(dataStream);
                String sResponseFromServer = tReader.ReadToEnd();
                HttpWebResponse httpResponse = (HttpWebResponse)tResponse;
                string statusCode = httpResponse.StatusCode.ToString();
                tReader.Close();
                dataStream.Close();
                tResponse.Close();
                return sResponseFromServer;
            }
            catch
            {
                throw new Exception();
            }
        }

        [HttpGet]
        public string NotifyDevice(string regId, string msg, string type, string DeviceId, int batchId, string applicationID, string SENDER_ID, string Mobile = "", string ImageUrl = "", string UserId = "", string RequestId = "", bool isCustomer = true)
        {
            string strTaaza_Notification_Detail = string.Empty;
            string Devicedetails = string.Empty;
            if (isCustomer)
            {
                strTaaza_Notification_Detail = "_Customer";
                Devicedetails = "Customer";
            }
            string retmsgid = "";
            string ReturnMessageId = "0";
            string ResponceString = "";
            int notificationId = 0;
            try
            {
                //  WCL("NotifyDevice() 5261 Msg:" + msg + " Type:" + type + " DeviceId:" + DeviceId + " BatchId:" + batchId + " mobile:" + Mobile + " RgeId:" + regId);
                //var applicationID = ClsCommon.apiKeyForNotification;
                //var SENDER_ID =ClsCommon.senderIdForNotification;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                httpWebRequest.Method = "POST";
                string json = "collapse_key=" + ClsCommon.projectName + "&data.header=" + ClsCommon.projectName + "&registration_id=" + regId + "&data.type=" + type + "&data.notificationId=" + notificationId + "&data.message=" + msg + "&data.mobile=" + Mobile + "&data.UserId=" + UserId + "&data.RequestId=" + RequestId;

                if (ImageUrl != null && ImageUrl != "")
                {
                    json += "&data.imgurl=" + ImageUrl;
                }
                httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
                httpWebRequest.Headers.Add(string.Format("Sender: key={0}", SENDER_ID));

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //Console.WriteLine(json);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                    using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            Console.WriteLine(result);
                            retmsgid = result.ToString();
                            if (retmsgid.Trim() != "")
                            {
                                ResponceString = result.ToString();
                                string[] msgsplits = retmsgid.Split(',');
                                string[] msg1 = msgsplits[0].ToString().Split(':');
                                ReturnMessageId = msg1[1].ToString();
                                // WCL("NotifyDevice() 5295 ReturnMsgId:" + ReturnMessageId);
                            }
                            else
                            {
                                ReturnMessageId = "0";
                                // WCL("NotifyDevice() 5303 ReturnMsgId: 0");
                            }
                        }
                        httpResponse.Close();
                        httpResponse.Dispose();
                        httpWebRequest = null;
                    }
                }
            }
            catch (Exception ex)
            {
                WCL("NotifyDevice()5450 DeviceId:" + DeviceId + " Result:" + ResponceString + " Msg:" + msg + " Type:" + type + " Regid:" + regId + " Mobile:" + Mobile + " ImgUrl:" + ImageUrl + " Err:" + ex.Message + "::::::" + ex.StackTrace);
                //dbCon.InsertLogs(LOGS.LogLevel.Error, "NotifyDevice Service 3375", "NotifyDevice()5450 DeviceId:" + DeviceId + " Result:" + ResponceString + " Msg:" + msg + " Type:" + type + " Regid:" + regId + " Mobile:" + Mobile + " ImgUrl:" + ImageUrl + " Err:" + ex.Message + "::::::" + ex.StackTrace, "-10");
            }
            try
            {
                string[] parm = { DeviceId, regId, msg, ReturnMessageId, type, Mobile, batchId.ToString() };
                string qry = "INSERT INTO [dbo].[Taaza_Notification_Detail" + strTaaza_Notification_Detail + "] ([DeviceId],[FCMRegistrationId],[Message],[ResponseId],[Type],[DOC],[Mobile],[Batchmasterid]) VALUES (@1,@2,@3,@4,@5,dateadd(minute, 330, getutcdate()),@6,@7)";
                int i = dbCon.ExecuteQueryWithParams(qry, parm);
            }
            catch (Exception ex)
            {
                // dbCon.InsertLogs(LOGS.LogLevel.Error, "NotifyDevice Service 3385", "NotifyDevice()5594 DeviceId:" + DeviceId + " Result:" + ResponceString + " Msg:" + msg + " Type:" + type + " Regid:" + regId + " Mobile:" + Mobile + " ImgUrl:" + ImageUrl + " Err:" + ex.Message + "::::::" + ex.StackTrace, "-10");
                WCL("NotifyDevice()5594 DeviceId:" + DeviceId + " Result:" + ResponceString + " Msg:" + msg + " Type:" + type + " Regid:" + regId + " Mobile:" + Mobile + " ImgUrl:" + ImageUrl + " Err:" + ex.Message + "::::::" + ex.StackTrace);
            }
            return ReturnMessageId;
        }


        #region Notification

        [HttpGet]
        public string Notification(string CustomerId)
        {
            dbConnection dbc = new dbConnection();
            string responce = "";
            try
            {
                string custdetail = "select Id,FirstName +' ' + LastName as Name,Mobile from Customer where Customer.Id=" + CustomerId;
                DataTable dtCust = dbc.GetDataTable(custdetail);
                if (dtCust != null && dtCust.Rows.Count > 0)
                {
                    string mobileno = dtCust.Rows[0]["Mobile"].ToString();
                    bool restult = SendCustomerNotificatio(mobileno, "New Product Available Now! ", "", dtCust.Rows[0]["Id"].ToString(), "");
                    if (restult == true)
                    {
                        return responce = "Success";
                    }
                    else
                    {
                        return responce = "Fail";
                    }
                }
                else
                {
                    return responce = "Fail";
                }

            }
            catch (Exception)
            {
                return responce = "Fail";
            }
        }
        #endregion


        public void WCL(string Log)
        {
            try
            {
                GarageXAPINEW.IApplicationExceptions Ex = new GarageXAPINEW.ApplicationExceptions();
                Ex.AddError("Notification Error " + dbCon.getindiantimeString(), Log, "1");
            }
            catch (Exception ex)
            {

            }
        }
        public class BulkNotificationResponse
        {
            public String multicast_id;
            public String success;

            public String failure;
            public String canonical_ids;

            public List<NotifiResults> results { get; set; }
        }
        public class NotifiResults
        {
            public String error;
            public String message_id;

        }

    }


}
