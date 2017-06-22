using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using YYXB.Filter;

using YYXB.Core;
using YYXB.Model;
using System.Security.Cryptography;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;


namespace YYXB.BaseControllers
{
    [ExceptionFilter]
    public class BaseController : Controller
    {
        public static string mall = "";

        public Int32 UserId = 0;
        public string UserName = "";
        public Boolean isadmin = false;

        protected string dbname = System.Configuration.ConfigurationManager.AppSettings["db"];
        protected string m_key = System.Configuration.ConfigurationManager.AppSettings["key"];
        protected string UseDefaultKey = System.Configuration.ConfigurationManager.AppSettings["UseDefaultKey"];

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                base.OnActionExecuting(filterContext);

                DateTime serverTime = DateTime.Parse("2900-01-01");
                string errMsg = "";
                if (!GetDateTimeNow(out serverTime, out errMsg))
                {
                    serverTime = DateTime.Parse("2900-01-01");
                }

                if (serverTime > DateTime.Parse("2017-08-01"))
                {
                    Response.Redirect("/Home/Index/", true);
                    return;
                }

                object s = filterContext.RequestContext.RouteData.Route.GetRouteData(filterContext.HttpContext);
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

        public static bool RoleIdValidate(int roleid)
        {
            if (roleid == 1)
            {
                return true;
            }
            return false;
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        protected string GetValue(string name, string defValue = "0")
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
            string value = GetValue(name,defValue.ToString());
            return ParseValueInt(value, defValue);
        }

        protected double GetValueDouble(string name, double defValue = 0)
        {
            string value = GetValue(name, defValue.ToString());
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

        public float ParseValueFloat(string strValue, float defaultVal = 0)
        {
            if (strValue == null || strValue.Trim() == "")
                return defaultVal;

            float retDouble = defaultVal;
            if (!float.TryParse(strValue, out retDouble))
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

        public static string GenerateOutTradeNo()
        {
            return string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), new Random().Next(100, 999).ToString());
        }

        public string UploadFile2()
        {
            string returnrul = "";
            string uploadPath = "/YYXBImages/" + DateTime.Now.ToString("yyyyMMdd") + "/";

            string filepath = Server.MapPath(uploadPath);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            HttpFileCollectionBase fs = Request.Files;
            for (int i = 0; i < fs.Count; i++)
            {
                HttpPostedFileBase file = fs[i];
                string filename = System.Guid.NewGuid().ToString();
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                string fullFileName = Path.Combine(filepath, string.Format("{0}{1}", filename, fileExt));

                Stream st = file.InputStream;
                BinaryReader br = new BinaryReader(st);
                byte[] imgdata = br.ReadBytes(Int32.Parse(st.Length.ToString()));
                FileStream bignewFile = new FileStream(fullFileName, FileMode.Create);
                bignewFile.Write(imgdata, 0, imgdata.Length);
                bignewFile.Close();
                br.Close();
                st.Close();

                returnrul += "," + string.Format("http://{0}/{1}/{2}{3}", Request.Url.Authority, uploadPath, filename, fileExt);
            }

            if (returnrul.Length > 0)
                returnrul = returnrul.Substring(1);

            return returnrul;
        }

        public bool GetDateTimeNow(out DateTime serverTime, out string errMsg)
        {
            errMsg = "";
            serverTime = DateTime.Now;
            try
            {
                string url = string.Format("http://tmcgw.zgyey.com/GetServerTime.ashx");
                string json = GetResponseString(url);
                if (json != null && json.Length > 0)
                {
                    if (DateTime.TryParse(json, out serverTime))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "接口网关GetServerTime不存在";
                        return false;
                    }
                }
                else
                {
                    errMsg = "接口网关GetServerTime不存在";
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 请求网关返回字符串
        /// </summary>
        /// <param name="url">网关</param>
        /// <returns>返回字符串结果</returns>
        public static string GetResponseString(string url)
        {
            byte[] byteresult = GetResponseBytes(url);
            return Encoding.UTF8.GetString(byteresult);
        }

        /// <summary>
        /// 请求网关返回字节流
        /// </summary>
        /// <param name="url">网关地址</param>
        /// <returns>字节流</returns>
        public static byte[] GetResponseBytes(string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return Encoding.UTF8.GetBytes(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return (new byte[0]);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        #region  获取IP

        public static string IPAddress
        {
            get
            {
                string result = String.Empty;

                result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (result != null && result != String.Empty)
                {

                    if (result.IndexOf(".") == -1)
                        result = null;
                    else
                    {
                        if (result.IndexOf(",") != -1)
                        {

                            result = result.Replace(" ", "").Replace("'", "");
                            string[] temparyip = result.Split(",;".ToCharArray());
                            for (int i = 0; i < temparyip.Length; i++)
                            {
                                if (IsIPAddress(temparyip[i])
                                    && temparyip[i].Substring(0, 3) != "10."
                                    && temparyip[i].Substring(0, 7) != "192.168"
                                    && temparyip[i].Substring(0, 7) != "172.16.")
                                {
                                    return temparyip[i];
                                }
                            }
                        }
                        else if (IsIPAddress(result))
                            return result;
                        else
                            result = null;
                    }

                }

                string IpAddress = (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];



                if (null == result || result == String.Empty)
                    result = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                if (result == null || result == String.Empty)
                    result = System.Web.HttpContext.Current.Request.UserHostAddress;

                return result;
            }
        }

        public static bool IsIPAddress(string str1)
        {
            if (str1 == null || str1 == string.Empty || str1.Length < 7 || str1.Length > 15) return false;

            string regformat = @"^/d{1,3}[/.]/d{1,3}[/.]/d{1,3}[/.]/d{1,3}$";

            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }

        #endregion
    }
}
