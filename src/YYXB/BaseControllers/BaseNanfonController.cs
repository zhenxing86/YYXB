using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nanfon.Filter;

using Nanfon.Core;
using Nanfon.Model;
using System.Security.Cryptography;
using System.Globalization;
using System.Data;
using Nanfon.Core.DataProxy;


namespace Nanfon.BaseControllers
{
    [ExceptionFilter]
    public class BaseNanfonController : Controller
    {
        public static string mall = "";

        public Int32 UserId = 0;
        public Int32 UserType = 0;
        public string UserName = "";
        public Boolean isadmin = false;
        public Users CurrentUser = null;
        protected string dbname = System.Configuration.ConfigurationManager.AppSettings["db"];
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                base.OnActionExecuting(filterContext);
                object s = filterContext.RequestContext.RouteData.Route.GetRouteData(filterContext.HttpContext);
                UserId = SessionManager.GetSessionInstance(filterContext.HttpContext.Request).UserID;
                IList < TextAndValue > tv = new List<TextAndValue>();
                tv.Add(new TextAndValue("@userid", UserId.ToString()));
                DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.users_GetModel");
                CurrentUser = ( from d in ds.Tables[0].AsEnumerable()
                           select new Users()
                           {
                               ID = d.Field<int?>("userid") ?? 0,
                               Account = d.Field<string>("account"),
                               Name = d.Field<string>("name"),
                               UserType = d.Field<int>("usertype")
                           }).FirstOrDefault();

                if (CurrentUser == null)
                {
                    Response.Redirect("/Login/Index/");
                    return;
                }

                ViewBag.UserID = UserId;
                UserType = CurrentUser.UserType;
                ViewBag.UserType = UserType;
                UserName = CurrentUser.Name;
                ViewData["path"] = filterContext.HttpContext.Request.Url.OriginalString;
                //ViewData["host"] = AppSetting.AppCenter;

                if (this.CurrentUser.ID == 0)
                {
                    Response.Redirect("/Login/Index");
                    return;
                }

                var ss = filterContext.RequestContext.HttpContext.Request.Browser.Type.ToLower().IndexOf("firefox");
                if (Convert.ToInt32(ss) >= 0)
                {
                    var ajaxRequestBeforeRedirect = TempData["__isajaxrequest"] as string;
                    if (ajaxRequestBeforeRedirect != null)
                        Request.Headers.Add("X-Requested-With", ajaxRequestBeforeRedirect);
                }
            }
            else
            {
                throw new ArgumentException("filterContext");
            }

        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var ss = filterContext.RequestContext.HttpContext.Request.Browser.Type.ToLower().IndexOf("firefox");
            if (Convert.ToInt32(ss) >= 0)
            {
                if (IsRedirectResult(filterContext.Result) && Request.Headers["X-Requested-With"] != null)
                    TempData["__isajaxrequest"] = Request.Headers["X-Requested-With"];
            }
        }

        private bool IsRedirectResult(ActionResult result)
        {
            return result.GetType().Name.ToLower().Contains("redirect");
        }

        protected string GetValue(string name, string defValue = "")
        {
            string value = null;
            value = Request.QueryString[name];
            if (value == null)
            {
                value = Request.Form[name];
            }
            if (value == null)
            {
                value = defValue;
            }
            return value;
        }

        protected int GetValueInt(string name, int defValue = 0)
        {
            string value = GetValue(name);
            return ParseValueInt(value, defValue);
        }

        protected double GetValueDouble(string name, double defValue = 0)
        {
            string value = GetValue(name);
            return ParseValueDouble(value, defValue);
        }

        public double ParseValueDouble(string strValue, double defaultVal = 0)
        {
            if (strValue == null || strValue.Trim() == "")
                return defaultVal;

            double retDouble = defaultVal;
            if (!double.TryParse(strValue, out retDouble))
            {
                retDouble = defaultVal;
            }

            return retDouble;
        }

        public int ParseValueInt(string strValue, int defaultVal = 0)
        {
            if (strValue == null || strValue.Trim() == "")
                return defaultVal;

            int retInt = defaultVal;
            if (!int.TryParse(strValue, out retInt))
            {
                retInt = defaultVal;
            }

            return retInt;
        }

        public IList<Category> GetCategoryList()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            DataSet dsCategory = KinProxyData.GetDataSet(tv, dbname + ".dbo.Category_GetList");
            IList<Category> categoryLst = (from d in dsCategory.Tables[0].AsEnumerable()
                                           select new Category()
                                           {
                                               //cname,cimage,istry,viewtype
                                               id = d.Field<int?>("id") ?? 0,
                                               cname = d.Field<string>("cname"),
                                               cimage = d.Field<string>("cimage"),
                                               istry = d.Field<int?>("istry") ?? 0,
                                               viewtype = d.Field<int?>("viewtype") ?? 0
                                           }).ToList();

            return categoryLst;
        }

        public WebDomain GetWebDomain(int id)
        {
            WebDomain domain = new WebDomain();
            int domainid = GetValueInt("domainid");
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@domainid", domainid.ToString()));

            DataSet dsDomain = KinProxyData.GetDataSet(tv, dbname + ".dbo.GetWebDomain");
            domain = (from d in dsDomain.Tables[0].AsEnumerable()
                      select new WebDomain()
                      { 
                          //id,url,descript,imgurl  
                          id = d.Field<int?>("id") ?? 0,
                          url = d.Field<string>("url"),
                          descript = d.Field<string>("descript"),
                          imgurl = d.Field<string>("imgurl"),
                          qq = d.Field<string>("qq"),
                          third_party_url = d.Field<string>("third_party_url")
                      }).FirstOrDefault();


            return domain;

        }

        public IList<Channel> GetChannelList(int userid)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", userid.ToString()));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.Channel_GetList");
            IList<Channel> channelLst = ds.Tables[0].AsEnumerable().Select(x => new Channel
            {
                //id,channelid,passwd,[delay],[role]
                id = x.Field<int>("id"),
                channelid = x.Field<string>("channelid"),
                passwd = x.Field<string>("passwd"),
                delay = x.Field<int>("delay"),
                role = x.Field<int>("role"),
                discount = x.Field<int>("discount"),
                contact = x.Field<string>("contact"),
                remark = x.Field<string>("remark"),
                ftype = x.Field<int>("ftype")

            }).ToList();

            return channelLst;
        }

    }
}
