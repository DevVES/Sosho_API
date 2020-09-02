using InquiryManageAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GarageXAPINEW
{
    public class ApplicationExceptions:IApplicationExceptions
    {
        #region Props
        dbConnection dbCon = new dbConnection();
        #endregion
        public void AddApplicationError(string message, string stack)
        {
            try
            {
                dbCon.ExecuteQuery("INSERT INTO [dbo].[ApplicationErrorLog]([Message], [Stack] ,[DOC]) VALUES ('" + message.Replace("'", "''") + "', '" + stack.Replace("'", "''") + "' ,'" + dbCon.getindiantimeString() + "')");
            }
            catch (Exception E)
            {
                //Can Log In TXT File HERE
            }
        }
        public void AddError(string message, string stack,string userId)
        {
            try
            {
                dbCon.ExecuteQuery("INSERT INTO [dbo].[ErrorLogs]([Message], [Stack], [UserId] ,[DOC]) VALUES ('" + message.Replace("'", "''") + "', '" + stack.Replace("'", "''") + "'," + userId + " ,'" + dbCon.getindiantimeString() + "')");
            }
            catch (Exception E)
            {
                //Can Log In TXT File HERE
            }
        }
    }
}