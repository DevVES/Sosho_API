using GarageXAPINEW;
using InquiryManageAPI.Controllers;
using System;
using System.Web.Http;
using static Test0555.Models.Frenchies.FranchiesModel;

namespace Test0555.Controllers
{
    public class FranchiesController : ApiController
    {
        dbConnection dbc = new dbConnection();
        CommonString cms = new CommonString();

        [HttpPost]
        public AddFranchiesResponse BecomeNewFranchise(AddFranchies model)
        {
            Logger.InsertLogsApp("Insert Franchisee start ");

            AddFranchiesResponse objFranchies = new AddFranchiesResponse();
            try
            {
                string[] insert = { model.Name.ToString(), model.Email.ToString(), model.Mobile.ToString(), model.PinCode.ToString(), model.Address.ToString(), model.CreatedBy.ToString() };
                string insertFrenchies = "INSERT INTO [dbo].[FranchiesMaster]([Name],[Email],[Mobile],[PinCode],[Address],[IsActive],[CreatedDate],[CreatedBy]) VALUES (@1,@2,@3,@4,@5,1,GetDate(),@6);Select SCOPE_IDENTITY()";
                int sourceid = dbc.ExecuteScalarQueryWithParams(insertFrenchies, insert);
                if(sourceid > 0)
                {
                    objFranchies.response = "1";
                    objFranchies.FranchieId = sourceid.ToString();
                    objFranchies.message = "Franchisee Detail Save Successfully";
                    ClsCommon.SendFranchiesEmail(model.Email,model.Name, model.Mobile, model.Address, model.PinCode);
                }
                else
                {
                    objFranchies.response = "0";
                    objFranchies.FranchieId = "0";
                    objFranchies.message = "Please Try Again!!";
                }
            }
            catch(Exception ex)
            {
                objFranchies.response = "0";
                objFranchies.FranchieId = "0";
                objFranchies.message = "Please Try Again!!";
            }
            return objFranchies;
        }
    }
}
