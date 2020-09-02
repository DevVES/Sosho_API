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
    public class BannerController : ApiController
    {
        dbConnection dbc = new dbConnection();
        [HttpGet]

        public BannerModel.BnnerImage getbannerimag()
        {
            BannerModel.BnnerImage objbaner = new BannerModel.BnnerImage();
            try
            {
                //DateTime datemain = dbc.getindiantime().ToString("dd/MMM/yyyy");
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                string querystr = "select top 1 * from HomepageBanner where IsActive=1 and IsDeleted=0 and Doc>='" + startdate + "' and Doc<='" + startend + "' order by Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                    DataTable dtpath = dbc.GetDataTable(querydata);
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        objbaner.response = "1";
                        objbaner.message = "Successfully";
                        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                        objbaner.BannerImageList = new List<BannerModel.BannerDataList>();
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string Title1 = dtmain.Rows[i]["Title"].ToString();
                            string AltText1 = dtmain.Rows[i]["AltText"].ToString();
                            string DataLink1 = dtmain.Rows[i]["Link"].ToString();
                            string ImageName1 = dtmain.Rows[i]["ImageName"].ToString();
                            objbaner.BannerImageList.Add(new BannerModel.BannerDataList
                            {
                                ImgUrl = urlpath + ImageName1,
                                Title = Title1,
                                AltText = AltText1,
                                DataLink = DataLink1

                            });
                        }
                    }
                    else
                    {
                        objbaner.response = "0";
                        objbaner.message = "Details Not Found";
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Details Not Found";

                }

                return objbaner;
            }
            catch (Exception ee)
            {
                objbaner.response = "-1";
                objbaner.message = "Something Wrong ";
                return objbaner;
            }
        }

        [HttpGet]

        public BannerModel.NewBnnerImage GetDashBoardBannerImag()
        {
            BannerModel.NewBnnerImage objbaner = new BannerModel.NewBnnerImage();
            try
            {
                string startdate = dbc.getindiantime().AddDays(-50).ToString("dd/MMM/yyyy") + " 00:00:00";
                string startend = dbc.getindiantime().ToString("dd/MMM/yyyy") + " 23:59:59";

                int iBannerPosition = (ConfigurationManager.AppSettings["BannerPosition"] != null && ConfigurationManager.AppSettings["BannerPosition"].Trim() != "") ? Convert.ToInt16(ConfigurationManager.AppSettings["BannerPosition"].Trim()) : 0;
                objbaner.BannerPosition = iBannerPosition.ToString();
                string querystr = "select top 1 * from HomepageBanner where IsActive=1 and IsDeleted=0 and Doc>='" + startdate + "' and Doc<='" + startend + "' order by Id desc";
                DataTable dtmain = dbc.GetDataTable(querystr);

                if (dtmain != null && dtmain.Rows.Count > 0)
                {
                    string querydata = "select KeyValue from StringResources where KeyName='BannerImageUrl'";
                    DataTable dtpath = dbc.GetDataTable(querydata);
                    if (dtpath != null && dtpath.Rows.Count > 0)
                    {
                        objbaner.response = "1";
                        objbaner.message = "Successfully";

                        string urlpath = dtpath.Rows[0]["KeyValue"].ToString();
                        objbaner.BannerImageList = new List<BannerModel.IntermediateBannerImage>();
                        for (int i = 0; i < dtmain.Rows.Count; i++)
                        {
                            string ImageName1 = dtmain.Rows[i]["ImageName"].ToString();
                            string Id = dtmain.Rows[i]["Id"].ToString();
                            objbaner.BannerImageList.Add(new BannerModel.IntermediateBannerImage
                            {
                                bannerURL = urlpath + ImageName1,
                                bannerId = Id,
                                action = "",
                                categoryId = "",
                                categoryName = "",
                                openUrlLink = ""

                            });
                        }
                    }
                    else
                    {
                        objbaner.response = "0";
                        objbaner.message = "Details Not Found";
                        objbaner.BannerPosition = "0";
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Details Not Found";
                    objbaner.BannerPosition = "0";

                }
                //Intermediate Banner
                string bannerqry = "Select Distinct TypeId From IntermediateBanners where IsActive=1 and IsDeleted=0";
                DataTable dtBanner = dbc.GetDataTable(bannerqry);

                objbaner.IntermediateBannerImages = new List<BannerModel.IntermediateBannerImage>();
                if (dtBanner != null && dtBanner.Rows.Count > 0)
                {
                    string sTypeId = "";
                    for (int i = 0; i < dtBanner.Rows.Count; i++)
                    {
                        sTypeId = dtBanner.Rows[i]["TypeId"].ToString();

                        string qry = "Select  cg.CategoryName,* From IntermediateBanners Im Left join category cg on  cg.categoryId = Im.categoryId where Im.IsActive=1 and Im.IsDeleted=0 and StartDate>='" + startdate + "' and StartDate<='" + startend + "' and TypeId = " + sTypeId + "  order by Id desc";
                        DataTable dtMainBanner = dbc.GetDataTable(qry);

                        string Id = "", ImageName1 = "", sAction = "", sCategoryId = "", sCategoryName = "", sopenUrlLink="";
                        if (dtMainBanner != null && dtMainBanner.Rows.Count > 0)
                        {
                            string imagepathqry = "select KeyValue from StringResources where KeyName='TopBannerImageUrl'";
                            DataTable dtimagepath = dbc.GetDataTable(imagepathqry);
                            if (dtimagepath != null && dtimagepath.Rows.Count > 0)
                            {
                                objbaner.response = "1";
                                objbaner.message = "Successfully";
                                string urlpath1 = dtimagepath.Rows[0]["KeyValue"].ToString();

                                Id = ""; ImageName1 = ""; sAction = ""; sCategoryId = ""; sCategoryName = ""; sopenUrlLink = "";
                                for (int j = 0; j < dtMainBanner.Rows.Count; j++)
                                {
                                    Id = dtMainBanner.Rows[j]["Id"].ToString();
                                    ImageName1 = dtMainBanner.Rows[j]["ImageName"].ToString();
                                    sAction = dtMainBanner.Rows[j]["Action"].ToString();

                                    if (sAction.ToString() == "Navigate To Category")
                                    {
                                        sCategoryId = dtMainBanner.Rows[j]["CategoryID"].ToString();
                                        sCategoryName = dtMainBanner.Rows[j]["CategoryName"].ToString();
                                    }
                                    else
                                    {
                                        sCategoryId = "0";
                                        sCategoryName = "";
                                    }
                                    if (sAction.ToString() == "Open Url")
                                        sopenUrlLink = dtMainBanner.Rows[j]["Link"].ToString();
                                    else
                                        sopenUrlLink = "";

                                    objbaner.IntermediateBannerImages.Add(new BannerModel.IntermediateBannerImage
                                    {
                                        bannerURL = urlpath1 + ImageName1,
                                        bannerId = Id,
                                        action = sAction,
                                        categoryId = sCategoryId,
                                        categoryName = sCategoryName,
                                        openUrlLink = sopenUrlLink
                                    });

                                }
                            }
                            else
                            {
                                objbaner.response = "0";
                                objbaner.message = "Intermediate Banner Details Not Found";
                            }
                        }
                    }
                }
                else
                {
                    objbaner.response = "0";
                    objbaner.message = "Intermediate Banner Details Not Found";
                }

                return objbaner;
            }
            catch (Exception ee)
            {
                objbaner.response = "-1";
                objbaner.message = "Something Wrong ";
                return objbaner;
            }
        }
    }
}
