using System;
using System.Collections.Generic;
using System.Web.Mvc;
using YYXB.Model;
using YYXB.Core.DataProxy;
using YYXB.Core;
using System.Text;
using System.Configuration;
using System.Data;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.IO;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Domain;
using Aop.Api.Response;
using System.Web;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using FastJSON;
using Top.Api.Domain;
using System.Collections.Specialized;
using Aop.Api.Util;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Net.Security;
using Senparc.Weixin.MP.TenPayLibV3;
using Senparc.Weixin.MP.AdvancedAPIs;
using WxPayAPI;
using Senparc.Weixin.MP;

namespace YYXB.Controllers
{
    public class mainController : BaseControllers.BaseController
    {
        public static string sms_url = System.Configuration.ConfigurationManager.AppSettings["sms_url"];
        public static string sms_appkey = System.Configuration.ConfigurationManager.AppSettings["sms_appkey"];
        public static string sms_secret = System.Configuration.ConfigurationManager.AppSettings["sms_secret"];
        public static string sms_template_code = System.Configuration.ConfigurationManager.AppSettings["sms_template_code"];

        public static string SSLCERT_PASSWORD = System.Configuration.ConfigurationManager.AppSettings["SSLCERT_PASSWORD"];


        public JsonResult UpdateLocation()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            int userid = GetValueInt("userid");
            tv.Add(new TextAndValue("@userid", userid.ToString()));
            tv.Add(new TextAndValue("@lng", GetValue("lng")));  //经度 
            tv.Add(new TextAndValue("@lat", GetValue("lat")));  //纬度
            tv.Add(new TextAndValue("@ip", IPAddress)); 
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_location_update"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult Register()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@account", GetValue("account")));
            tv.Add(new TextAndValue("@password", GetValue("password")));
            tv.Add(new TextAndValue("@code", GetValue("code")));
            tv.Add(new TextAndValue("@realpassword", GetValue("realpassword")));

            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_register"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 登陆(用户端)
        /// </summary>
        /// <returns></returns>
        public JsonResult uLogin()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("account", GetValue("account")));
            tv.Add(new TextAndValue("password", GetValue("password")));
            tv.Add(new TextAndValue("@device_id", GetValue("device_id")));
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_Login");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "登陆失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                string retMsg = ds.Tables[0].Rows[0][0].ToString();
                if (retMsg != "0")
                {
                    bmrow = new BaseModel<object>(1, ds.Tables[0].Rows[0][1].ToString(), new object());
                }
                else
                {
                    //用户基本信息
                    var user = ds.Tables[1].AsEnumerable().Select(x => new
                    {
                        userid = x.Field<long>("userid"),
                        account = x.Field<string>("account"),
                        realname = x.Field<string>("realname"),
                        nickname = x.Field<string>("nickname"),
                        avatar = x.Field<string>("avatar"),
                        gender = x.Field<int>("gender"),
                        birthdate = x.Field<string>("birthdate"),
                        tel = x.Field<string>("tel"),
                        idcard = x.Field<string>("idcard"),
                        age = x.Field<int>("age"),
                        star = x.Field<int>("star"),
                        area = x.Field<int>("area"),
                        area_name = x.Field<string>("area_name")

                    }).FirstOrDefault();

                    //banner图
                    var banners = ds.Tables[2].AsEnumerable().Select(x => new
                    {
                        bannerurl = x.Field<string>("bannerurl")
                    });

                    bmrow = new BaseModel<object>(0, "", new
                    {
                        user = user,
                        banners = banners
                    });
                }
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 用户主动退出
        /// </summary>
        /// <returns></returns>
        public JsonResult Logout()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            int userid = GetValueInt("userid");
            string device_id = GetValue("device_id");
            tv.Add(new TextAndValue("@userid", userid.ToString()));
            tv.Add(new TextAndValue("@device_id", device_id));
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));
            tv.Add(new TextAndValue("@app_type", GetValue("app_type")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_logout"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发送手机验证码
        /// </summary>
        /// <returns>0验证码已发送，请留意接收，-1验证码发送失败，请重新发送, 验证码varchar, 手机号</returns>
        public JsonResult sendPhoneCode()
        {

            //需要调用短信接口
            string phone = GetValue("phone");
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@phone", phone));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_sendPhoneCode");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "请求失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                string verification_code = ds.Tables[0].Rows[0][2].ToString();

                //发送验证码 调用第三方sdk
                AlibabaAliqinFcSmsNumSendResponse ret = SendCode(phone, verification_code);
                if (!ret.IsError)
                {
                    bmrow = new BaseModel<object>(0, "", verification_code);
                }
                else
                {
                    bmrow = new BaseModel<object>(-1, ret.SubErrMsg, new object());
                }
                //bmrow = new BaseModel<object>(0, "", "11111");
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        //public int sendsms(string mobile, string code)
        //{
        //    try
        //    {
        //        string content = string.Format("【医依相伴】您的验证码为：{0}，请在15分钟内按页面提示提交验证码，切勿将验证码泄露于他人！", code);
        //        string param = string.Format("action=send&userid={0}&account={1}&password={2}&content={3}&mobile={4}", smsUserId, account, password, content, mobile);
        //        byte[] postBytes = Encoding.UTF8.GetBytes(param);
        //        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(PostUrl);
        //        req.Method = "POST";
        //        req.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
        //        req.ContentLength = postBytes.Length;
        //        using (Stream reqStream = req.GetRequestStream())
        //        {
        //            reqStream.Write(postBytes, 0, postBytes.Length);
        //        }
        //        System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
        //        using (WebResponse wr = req.GetResponse())
        //        {
        //            StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.UTF8);
        //            System.IO.StreamReader xmlStreamReader = sr;
        //            xmlDoc.Load(xmlStreamReader);
        //        }
        //        if (xmlDoc == null)
        //        {
        //            //MessageBox.Show("请求发生异常");
        //            return -1;
        //        }
        //        else
        //        {
        //            String message = xmlDoc.GetElementsByTagName("message").Item(0).InnerText.ToString();
        //            if (message == "ok")
        //            {
        //                //MessageBox.Show("发送成功");
        //                return 1;
        //            }
        //            else
        //            {
        //                //MessageBox.Show(message);
        //                return 0;
        //            }
        //        }
        //    }
        //    catch (System.Net.WebException WebExcp)
        //    {
        //        //MessageBox.Show("网络错误，无法连接到服务器！");
        //        return -1;
        //    }
        //}

        public AlibabaAliqinFcSmsNumSendResponse SendCode(string mobile, string code)
        {
            //正式环境 	http://gw.api.taobao.com/router/rest 	https://eco.taobao.com/router/rest

            ITopClient client = new DefaultTopClient(sms_url, sms_appkey, sms_secret, "json");
            AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
            req.Extend = "123456";
            req.SmsType = "normal";
            req.SmsFreeSignName = "护康相伴";
            req.SmsParam = "{\"code\":\"" + code + "\"}";  //"{\"code\":\"1234\",\"product\":\"alidayu\"}";  //如果模板的参数多个，使用逗号隔开
            req.RecNum = mobile;
            req.SmsTemplateCode = sms_template_code;
            AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);

            return rsp;
        }


        /// <summary>
        /// 更新设备号【使用中】
        /// </summary>
        /// <returns></returns>
        public JsonResult updateClientID()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));
            tv.Add(new TextAndValue("@device_id", GetValue("device_id")));
            tv.Add(new TextAndValue("@app", GetValueInt("app").ToString()));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updateClientID"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取用户所在城市的医院列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMsgs()
        {
            //string path = Request.PhysicalApplicationPath;
            //log4net.LogManager.GetLogger("PhysicalApplicationPath").Info("path=" + path);

            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));
            tv.Add(new TextAndValue("@app", GetValue("app", "-1"))); //1：用户端，2：护工端
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_GetMsgs");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //msgid,userid,touid,msgtype,title,content,status,adddate,receivedate,action,senddate,url,crtdate
                    msgid = x.Field<long>("msgid"),
                    userid = x.Field<long>("userid"),
                    touid = x.Field<long>("touid"),
                    msgtype = x.Field<int>("msgtype"),
                    pushtype = x.Field<string>("pushtype"),
                    title = x.Field<string>("title"),
                    content = x.Field<string>("content"),
                    adddate = x.Field<DateTime>("adddate").ToString("yyyy-MM-dd HH:mm:ss"),
                    action = x.Field<Int16>("action")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <returns></returns>
        public JsonResult SendMsg()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));
            tv.Add(new TextAndValue("@touid", GetValue("touid")));
            tv.Add(new TextAndValue("@title", GetValue("title")));
            tv.Add(new TextAndValue("@content", GetValue("content")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_sendmsg"), JsonRequestBehavior.AllowGet);

        }

        #region 首页

        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetCitys()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_GetCitys");
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "获取失败", new object()),JsonRequestBehavior.AllowGet);
            }
            else
            {
                //热门城市
                var hotCitys = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    // id,province_id,province,city_id,city,district_id,district,lv,is_hot
                    id = x.Field<int>("id"),
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district"),
                    lv = x.Field<int>("lv"),
                    is_hot = x.Field<int>("is_hot")
                });

                //城市列表
                var citys = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    // id,province_id,province,city_id,city,district_id,district,lv,is_hot
                    id = x.Field<int>("id"),
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district"),
                    lv = x.Field<int>("lv"),
                    is_hot = x.Field<int>("is_hot")
                });

                return this.Json(new BaseModel<object>(0, "", new
                {
                    hotCitys = hotCitys,
                    citys = citys
                }), JsonRequestBehavior.AllowGet);
                
            }
            
        }

        /// <summary>
        /// 获取城市id
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetCity()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            //tv.Add(new TextAndValue("@appver", GetValue("appver")));
            //tv.Add(new TextAndValue("@client", GetValue("client")));
            tv.Add(new TextAndValue("@province", GetValue("province"))); //省份
            tv.Add(new TextAndValue("@city", GetValue("city")));     //地市
            tv.Add(new TextAndValue("@district", GetValue("district")));     //区

            //province=广东&city=广州&district=天河
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_GetCity");
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "获取失败", new object()),JsonRequestBehavior.AllowGet);
            }
            else
            {
                //城市id
                var city = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    // province_id,province,city_id,city,district_id,district
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district")
                }).FirstOrDefault();

                return this.Json(new BaseModel<object>(0, "", city), JsonRequestBehavior.AllowGet);
                
            }
            
        }
        /// <summary>
        /// 获取用户所在城市的医院列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHospitals()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@hospital_name", GetValue("hospital_name")));
            tv.Add(new TextAndValue("@city_id", GetValue("city_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_GetHospitals");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    id = x.Field<int>("id"),
                    hospital_name = x.Field<string>("name"),
                    lv = x.Field<string>("lv"),
                    avatar = x.Field<string>("avatar"),
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district"),
                    addr = x.Field<string>("addr"),
                    contact = x.Field<string>("contact"),
                    tel = x.Field<string>("tel"),
                    nurse_cnt = x.Field<int>("nurse_cnt")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }
        /// <summary>
        /// 获取模糊查找医院
        /// </summary>
        /// <returns></returns>
        public JsonResult uFindHospitals()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@hospital_name", GetValue("hospital_name")));
            tv.Add(new TextAndValue("@city_id", GetValue("city_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_FindHospitals");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    id = x.Field<int>("id"),
                    hospital_name = x.Field<string>("name"),
                    lv = x.Field<string>("lv"),
                    avatar = x.Field<string>("avatar"),
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district"),
                    addr = x.Field<string>("addr"),
                    contact = x.Field<string>("contact"),
                    tel = x.Field<string>("tel"),
                    nurse_cnt = x.Field<int>("nurse_cnt")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 获取医院护理类型
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHospNursingCategory()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("hospitalid", GetValue("hospitalid")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_GetHospNursingCategory");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(1, "请求失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                //id,hospitalid,n_type,cost,content_descr,time_descr
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    id = x.Field<int>("id"),
                    hospitalid = x.Field<int>("hospitalid"),
                    n_type = x.Field<int>("n_type"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    content_descr = x.Field<string>("content_descr"),
                    time_descr = x.Field<string>("time_descr")
                }).ToList();

                bmrow = new BaseModel<object>(0, "", r);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        
        /// <summary>
        /// 获取医院护士、护工列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHospNurses()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@hospitalid", GetValue("hospitalid")));
            tv.Add(new TextAndValue("@city_id", GetValue("city_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gethospnurses");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    userid = x.Field<long>("userid"),
                    account = x.Field<string>("account"),
                    realname = x.Field<string>("name"),
                    nickname = x.Field<string>("nickname"),
                    avatar = x.Field<string>("avatar"),
                    tel = x.Field<string>("tel"),
                    idcard = x.Field<string>("idcard"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    gender = x.Field<int>("gender"),
                    bio = x.Field<string>("bio"),
                    star = x.Field<int>("star"),
                    seniority = x.Field<int>("seniority"),
                    area = x.Field<int>("area"),
                    area_name = x.Field<string>("area_name"),
                    lng = ParseValueFloat(x["lng"].ToString()),
                    lat = ParseValueFloat(x["lat"].ToString())
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 获取家庭陪护护士、护工、陪诊列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHouseNurses()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@job", GetValue("job")));// 1陪诊，2护工，3护士
            tv.Add(new TextAndValue("@city_id", GetValue("city_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gethousenurses");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    userid = x.Field<long>("userid"),
                    account = x.Field<string>("account"),
                    realname = x.Field<string>("name"),
                    nickname = x.Field<string>("nickname"),
                    avatar = x.Field<string>("avatar"),
                    tel = x.Field<string>("tel"),
                    idcard = x.Field<string>("idcard"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    gender = x.Field<int>("gender"),
                    bio = x.Field<string>("bio"),
                    star = x.Field<int>("star"),
                    seniority = x.Field<int>("seniority"),
                    area = x.Field<int>("area"),
                    area_name = x.Field<string>("area_name"),
                    unit = x.Field<string>("unit"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    pickup_cost = ParseValueFloat(x["pickup_cost"].ToString()),
                    lng = ParseValueFloat(x["lng"].ToString()),
                    lat = ParseValueFloat(x["lat"].ToString())
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 获取护士、护工详情
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetNurseDetail()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@job", GetValue("job")));  //0:医院护士、1:陪诊,2:护工,3:护士

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getnursedetail");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    userid = x.Field<long>("userid"),
                    account = x.Field<string>("account"),
                    realname = x.Field<string>("name"),
                    nickname = x.Field<string>("nickname"),
                    avatar = x.Field<string>("avatar"),
                    tel = x.Field<string>("tel"),
                    idcard = x.Field<string>("idcard"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    gender = x.Field<int>("gender"),
                    bio = x.Field<string>("bio"),
                    star = x.Field<int>("star"),
                    seniority = x.Field<int>("seniority"),
                    area = x.Field<int>("area"),
                    area_name = x.Field<string>("area_name")

                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 获取护士、护工的评价信息
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetNurseComments()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getnursecomments");
            BaseModelEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //cmtid,userid,realname,touid,toname,star,content,create_time
                    cmtid = x.Field<long>("cmtid"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    touid = x.Field<long>("touid"),
                    toname = x.Field<string>("toname"),
                    star = x.Field<int>("star"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取病人列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetPatients()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getpatients");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("name"),
                    avatar = x.Field<string>("avatar"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    idcard = x.Field<string>("idcard"),
                    gender = x.Field<int>("gender"),
                    tel = x.Field<string>("tel"),
                    is_default = x.Field<int>("is_default"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 新增病人
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uAddPatient()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", "0"));
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@name", GetValue("realname")));
            tv.Add(new TextAndValue("@avatar", GetValue("avatar")));
            tv.Add(new TextAndValue("@birthdate", GetValue("birthdate", "1900-01-01")));
            tv.Add(new TextAndValue("@idcard", GetValue("idcard")));
            tv.Add(new TextAndValue("@gender", GetValue("gender")));
            tv.Add(new TextAndValue("@tel", GetValue("tel")));
            tv.Add(new TextAndValue("@is_default", GetValue("is_default")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_addOrUpdatePatient"), JsonRequestBehavior.AllowGet);

        }

        
        /// <summary>
        /// 编辑病人
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uEditPatient()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@name", GetValue("realname")));
            tv.Add(new TextAndValue("@avatar", GetValue("avatar")));
            tv.Add(new TextAndValue("@birthdate", GetValue("birthdate","1900-01-01")));
            tv.Add(new TextAndValue("@idcard", GetValue("idcard")));
            tv.Add(new TextAndValue("@gender", GetValue("gender")));
            tv.Add(new TextAndValue("@tel", GetValue("tel")));
            tv.Add(new TextAndValue("@is_default", GetValue("is_default")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_addOrUpdatePatient"), JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 删除病人
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uDeletePatient()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));
            tv.Add(new TextAndValue("@userid", GetValue("userid")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_DeletePatient"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取医院智能硬件
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHospHardwares()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@hospitalid", GetValue("hospitalid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gethosphardwares");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    //id,hospitalid,s_type,cost,unit
                    id = x.Field<int>("id"),
                    hospitalid = x.Field<int>("hospitalid"),
                    s_type = x.Field<int>("s_type"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    unit = x.Field<string>("unit")
                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取家庭陪护的护理项目
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHouseServices()
        {
            int n_type = GetValueInt("n_type");
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@n_type", n_type.ToString()));  //1普通护理,2	VIP护理,3	智能护理,4	护工,5	护士,6	陪诊

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gethouseservices");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                if (n_type <= 3)
                {
                    var r = ds.Tables[0].AsEnumerable().Select(x => new
                    {
                        //id,hospitalid,s_type,cost,unit
                        id = x.Field<int>("id"),
                        hospitalid = x.Field<int>("hospitalid"),
                        s_type = x.Field<int>("s_type"),
                        cost = ParseValueFloat(x["cost"].ToString()),
                        unit = x.Field<string>("unit")
                    }).ToList();

                    return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var r = ds.Tables[0].AsEnumerable().Select(x => new
                    {
                        //id,userid,s_type,cost,unit
                        id = x.Field<long>("id"),
                        userid = x.Field<long>("userid"),
                        s_type = x.Field<int>("s_type"),
                        cost = ParseValueFloat(x["cost"].ToString()),
                        unit = x.Field<string>("unit")
                    }).ToList();

                    return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }


        public JsonResult uAddOrder()
        {
            string orderno = GenerateOutTradeNo();
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));//用户端登陆人userid
            tv.Add(new TextAndValue("p_userid", GetValue("p_userid")));//病人userid
            tv.Add(new TextAndValue("orderno", orderno));//订单号
            tv.Add(new TextAndValue("begin_time", GetValue("begin_time")));//开始时间
            tv.Add(new TextAndValue("end_time", GetValue("end_time",DateTime.Now.ToString("yyyy-MM-dd"))));//结束时间
            tv.Add(new TextAndValue("o_type", GetValue("o_type")));//订单类型
            tv.Add(new TextAndValue("n_type", GetValue("n_type")));//护理项目类型
            tv.Add(new TextAndValue("acceptor", GetValue("acceptor")));//接单人userid（护士/护工）
            tv.Add(new TextAndValue("hospitalid", GetValue("hospitalid")));//医院id(医院陪护、陪诊才要，其他填0)
            tv.Add(new TextAndValue("hospital_name", GetValue("hospital_name")));//医院名称(陪诊手动写下的才要)
            tv.Add(new TextAndValue("area", GetValue("area")));//地区id（精确到县、区）
            tv.Add(new TextAndValue("addr", GetValue("addr")));//详细地址
            tv.Add(new TextAndValue("pickup", GetValue("pickup")));//是否接送（陪诊才有）
            tv.Add(new TextAndValue("pickup_cost", GetValue("pickup_cost")));//接送费用（陪诊才有）
            tv.Add(new TextAndValue("nursing_unit_price", GetValue("nursing_unit_price")));//护理类型价格（只有医院陪护才有）
            tv.Add(new TextAndValue("nursing_cnt", GetValue("nursing_cnt")));//护理次数（只有医院陪护才有）

            tv.Add(new TextAndValue("hardware_cost", GetValue("hardware_cost")));//智能硬件金额
            tv.Add(new TextAndValue("cost", GetValue("cost")));//总金额
            tv.Add(new TextAndValue("services", GetValue("services")));//护理项目及金额及次数，多个用逗号隔开   s_type|cost|cnt,s_type|cost|cnt,s_type|cost|cnt,s_type|cost|cnt
            tv.Add(new TextAndValue("remark1", GetValue("remark1")));//（护士陪护）你的病情详情/  （护工陪护）你的需求信息
            tv.Add(new TextAndValue("audio_url1", GetValue("audio_url1")));//录音
            tv.Add(new TextAndValue("remark2", GetValue("remark2")));//（护士陪护）是否有必备的药品或工具
            tv.Add(new TextAndValue("audio_url2", GetValue("audio_url2")));//录音
            tv.Add(new TextAndValue("client", GetValue("client")));//1：安卓，2：ios  

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_addorder");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //1、订单详情
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),

                    o_type = x.Field<int>("o_type"),
                    n_type = x.Field<int>("n_type"),
                    acceptor = x.Field<long>("acceptor"),
                    state = x.Field<int>("state"),
                    hospitalid = x.Field<int>("hospitalid"),
                    hospital_name = x.Field<string>("hospital_name"),
                    area = x.Field<int>("area"),
                    addr = x.Field<string>("addr"),
                    pickup = x.Field<int>("pickup"),
                    pickup_cost = ParseValueFloat(x["pickup_cost"].ToString()),
                    nursing_unit_price = ParseValueFloat(x["nursing_unit_price"].ToString()),
                    nursing_cnt = x.Field<int>("nursing_cnt"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    acceptor_name = x.Field<string>("acceptor_name"),

                    //2、护理项目
                    services = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        hospitalid = s.Field<int>("hospitalid"),
                        userid = x.Field<long>("userid"),
                        s_type = s.Field<int>("s_type"),
                        cost = ParseValueFloat(s["cost"].ToString()),
                        unit = s.Field<string>("unit")
                    }),
                    //3、护工/护士信息
                    nurses = ds.Tables[3].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        realname = s.Field<string>("name"),
                        gender = s.Field<int>("gender"),
                        avatar = s.Field<string>("avatar"),
                        birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                        age = s.Field<int>("age"),
                        seniority = s.Field<int>("seniority"),  //护龄
                        star = s.Field<int>("star"),
                        area = s.Field<int>("area"),
                        area_name = s.Field<string>("area_name")
                    }),
                    //4、患者信息
                    patients = ds.Tables[4].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        realname = s.Field<string>("name"),
                        gender = s.Field<int>("gender"),
                        avatar = s.Field<string>("avatar"),
                        birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                        age = s.Field<int>("age"),
                        idcard = s.Field<string>("idcard"),  //护龄
                        tel = s.Field<string>("tel")
                    }),
                    //5、备注信息
                    remarks = ds.Tables[5].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        or_type = s.Field<int>("or_type"),    //备注类型
                        content = s.Field<string>("content"), //备注内容
                        audio_url = s.Field<string>("audio_url") //备注语音
                    })
                }).FirstOrDefault();
                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 我的订单
        /// <summary>
        /// 获取用户订单信息
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetOrders()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));       //用户端登陆人userid
            tv.Add(new TextAndValue("@o_type", GetValue("o_type")));       //订单类型(1医院陪护,2家庭陪护,3陪诊)
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getorders");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //1、订单详情
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),

                    o_type = x.Field<int>("o_type"),
                    n_type = x.Field<int>("n_type"),
                    acceptor = x.Field<long>("acceptor"),
                    state = x.Field<int>("state"),
                    hospitalid = x.Field<int>("hospitalid"),
                    hospital_name = x.Field<string>("hospital_name"),
                    area = x.Field<int>("area"),
                    addr = x.Field<string>("addr"),
                    pickup = x.Field<int>("pickup"),
                    pickup_cost = ParseValueFloat(x["pickup_cost"].ToString()),
                    nursing_unit_price = ParseValueFloat(x["nursing_unit_price"].ToString()),
                    nursing_cnt = x.Field<int>("nursing_cnt"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    acceptor_name = x.Field<string>("acceptor_name"),
                    hospital_avatar = x.Field<string>("hospital_avatar"),

                    time_descr = x.Field<string>("time_descr"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    refund_reason = x.Field<string>("refund_reason"),
                    refund_cost = ParseValueFloat(x["refund_cost"].ToString()),
                    //2、护理项目
                    services = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                {
                    hospitalid = s.Field<int>("hospitalid"),
                    userid = x.Field<long>("userid"),
                    s_type = s.Field<int>("s_type"),
                    cost = ParseValueFloat(s["cost"].ToString()),
                    unit = s.Field<string>("unit")
                }),
                    //3、护工/护士信息
                    nurses = ds.Tables[3].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                {
                    realname = s.Field<string>("name"),
                    gender = s.Field<int>("gender"),
                    avatar = s.Field<string>("avatar"),
                    birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = s.Field<int>("age"),
                    seniority = s.Field<int>("seniority"),  //护龄
                    star = s.Field<int>("star"),
                    area = s.Field<int>("area"),
                    area_name = s.Field<string>("area_name"),
                    tel = s.Field<string>("tel")
                }),
                    //4、患者信息
                    patients = ds.Tables[4].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                   {
                       realname = s.Field<string>("name"),
                       gender = s.Field<int>("gender"),
                       avatar = s.Field<string>("avatar"),
                       birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                       age = s.Field<int>("age"),
                       idcard = s.Field<string>("idcard"),  //护龄
                       tel = s.Field<string>("tel")
                   }),
                    //5、备注信息
                    remarks = ds.Tables[5].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        or_type = s.Field<int>("or_type"),
                        content = s.Field<string>("content"),
                        audio_url = s.Field<string>("audio_url")
                    }),
                    //6、订单评论
                    comment = ds.Tables[6].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Count() > 0 ?
                    ds.Tables[6].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        content = s.Field<string>("content"),
                        star = s.Field<int>("star")
                    }).FirstOrDefault() : new { content = "", star = 0 }
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uCancleOrder()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_cancleorder"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uApplyRefund()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@refund_reason", GetValue("refund_reason")));
            tv.Add(new TextAndValue("@refund_cost", GetValue("refund_cost","0")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_ApplyRefund"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 评价护工/护士
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult uCommentOrder()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@touid", GetValue("touid")));
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@star", GetValue("star")));
            tv.Add(new TextAndValue("@content", GetValue("content")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_commentOrder"), JsonRequestBehavior.AllowGet);

        }

        #endregion 

        #region 健康档案

        /// <summary>
        /// 获取诊断记录
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetDiagnosis()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getdiagnosis");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    orderid = x.Field<long>("orderid"),
                    postion = x.Field<string>("postion"),
                    in_time = x.Field<DateTime>("in_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    out_time = x.Field<DateTime>("out_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    descr = x.Field<string>("descr"),
                    avatar = x.Field<string>("avatar"),
                    acceptor = x.Field<long>("acceptor"),
                    acceptor_name = x.Field<string>("acceptor_name"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取病患记录
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetScadas()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid

            //scada_type int,  --病患采集类型（1体温,2血压,3血糖,4胎心仪,5心电图,6血氧计,7体脂称）
            tv.Add(new TextAndValue("@scada_type", GetValue("scada_type")));
            tv.Add(new TextAndValue("@bgntime", GetValue("bgntime")));
            tv.Add(new TextAndValue("@endtime", GetValue("endtime"))); 
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getscadas");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //orderid,scada_type,deviceid,record_time,value1,value2
                    orderid = x.Field<long>("orderid"),
                    scada_type = x.Field<int>("scada_type"),
                    deviceid = x.Field<string>("deviceid"),
                    record_time = x.Field<DateTime>("record_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    value1 = ParseValueFloat(x["value1"].ToString()),
                    value2 = ParseValueFloat(x["value2"].ToString()),
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用药记录
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetMedicines()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getmedicines");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    orderid = x.Field<long>("orderid"),
                    take_time = x.Field<DateTime>("take_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    medicine_name = x.Field<string>("medicine_name"),
                    dosage = x.Field<string>("dosage"),
                    avatar = x.Field<string>("avatar"),
                    acceptor = x.Field<long>("acceptor"),
                    acceptor_name = x.Field<string>("acceptor_name")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 个人中心——用户信息

        /// <summary>
        /// 个人资料-修改昵称
        /// </summary>
        public JsonResult updateNickname()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@nickname", GetValue("nickname")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updateNickname"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 个人资料-账号安全-修改密码
        /// </summary>
        /// <returns>0密码修改成功-1密码修改失败</returns>
        public JsonResult updatePassword()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@phone", GetValue("phone")));
            tv.Add(new TextAndValue("@code", GetValue("code")));
            tv.Add(new TextAndValue("@password", GetValue("password")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updatePassword"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 忘记密码，修改密码
        /// </summary>
        /// <returns>0密码修改成功-1密码修改失败</returns>
        public JsonResult updatePasswordV2()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@account", GetValue("account")));
            tv.Add(new TextAndValue("@phone", GetValue("phone")));
            tv.Add(new TextAndValue("@code", GetValue("code")));
            tv.Add(new TextAndValue("@password", GetValue("password")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updatePasswordV2"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        public JsonResult updateAvatar()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@avatar", GetValue("avatar")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updateAvatar"), JsonRequestBehavior.AllowGet);

            //string retUrl = "0";
            //try
            //{
            //    retUrl = Upload();
            //    tv.Add(new TextAndValue("@avatar", retUrl));
            //    return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_updateAvatar"), JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    retUrl = ex.Message;
            //    return this.Json(new
            //    {
            //        code = 1,
            //        info = retUrl,
            //        result = new { }
            //    }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion

        #region 个人中心——我的关注

        /// <summary>
        /// 关注的用户列表
        /// </summary>
        public JsonResult uGetAttentionUsers()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getAttentionUsers");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //avatar,touid,toname
                    avatar = x.Field<string>("avatar"),
                    userid = x.Field<long>("touid"),
                    realname = x.Field<string>("toname")
                }).ToList();
                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 关注用户
        /// </summary>
        public JsonResult uAddAttentionUser()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //关注人
            tv.Add(new TextAndValue("@touid", GetValue("touid")));     //被关注人

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddAttentionUser"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取消关注用户
        /// </summary>
        public JsonResult uRemoveAttentionUser()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //关注人
            tv.Add(new TextAndValue("@touid", GetValue("touid")));     //被关注人

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_RemoveAttentionUser"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 关注话题
        /// </summary>
        public JsonResult uAddAttentionTopic()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //关注人
            tv.Add(new TextAndValue("@topic_id", GetValue("topic_id"))); //话题id

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddAttentionTopic"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 关注话题
        /// </summary>
        public JsonResult uRemoveAttentionTopic()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //关注人
            tv.Add(new TextAndValue("@topic_id", GetValue("topic_id"))); //话题id

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_RemoveAttentionTopic"), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取关注的主题列表
        /// </summary>
        public JsonResult uGetAttentionTopics()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getAttentionTopics");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //topic_id,section_id,userid,title,content,create_time,like_cnt,comment_cnt
                    //section_name,realname,avatar,gender,attentionuser
                    topic_id = x.Field<long>("topic_id"),
                    section_id = x.Field<int>("section_id"),
                    section_name = x.Field<string>("section_name"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    avatar = x.Field<string>("avatar"),
                    gender = x.Field<int>("gender"),
                    attentionuser = x.Field<int>("attentionuser"),
                    title = x.Field<string>("title"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    sources = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("topic_id") == x.Field<long>("topic_id")).Select(s => new
                    {
                        //topic_id,s.id,s.url,s.src_type,s.state
                        id = s.Field<long>("id"),
                        url = s.Field<string>("url"),
                        src_type = s.Field<int>("src_type"),
                        state = s.Field<int>("state")
                    })
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用户所有的主题信息——个人名片
        /// </summary>
        public JsonResult uGetUserTopics()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));//登陆人
            tv.Add(new TextAndValue("@touid", GetValue("touid")));//关注的用户
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getUserTopics");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //topic_id,section_id,userid,title,content,create_time,like_cnt,comment_cnt
                    //section_name,realname,avatar,gender,attentionuser
                    pcount = x.Field<int>("pcount"),
                    topic_id = x.Field<long>("topic_id"),
                    section_id = x.Field<int>("section_id"),
                    section_name = x.Field<string>("section_name"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    avatar = x.Field<string>("avatar"),
                    gender = x.Field<int>("gender"),
                    attentionuser = x.Field<int>("attentionuser"),
                    attention_topic = x.Field<int>("attention_topic"),
                    title = x.Field<string>("title"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    self_like = x.Field<int>("self_like"),
                    sources = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("topic_id") == x.Field<long>("topic_id")).Select(s => new
                    {
                        //topic_id,s.id,s.url,s.src_type,s.state
                        id = s.Field<long>("id"),
                        url = s.Field<string>("url"),
                        src_type = s.Field<int>("src_type"),
                        state = s.Field<int>("state")
                    })
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取话题详情
        /// </summary>
        public JsonResult GetTopic()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));      //登陆人
            tv.Add(new TextAndValue("@topic_id", GetValue("topic_id")));  //话题id  
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));  //获取数量  
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));      //当前索引  

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_gettopic");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    //topic_id,section_id,userid,title,content,create_time,like_cnt,comment_cnt
                    //section_name,realname,avatar,gender,attentionuser
                    topic_id = x.Field<long>("topic_id"),
                    section_id = x.Field<int>("section_id"),
                    section_name = x.Field<string>("section_name"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    avatar = x.Field<string>("avatar"),
                    gender = x.Field<int>("gender"),
                    attentionuser = x.Field<int>("attentionuser"),
                    attentiontopic = x.Field<int>("attentiontopic"),
                    title = x.Field<string>("title"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    self_like = x.Field<int>("self_like"),
                    commands = ds.Tables[2].AsEnumerable().Select(c => new
                    {
                        userid = c.Field<long>("userid"),
                        realname = c.Field<string>("realname"),
                        touid = c.Field<long>("touid"),
                        toname = c.Field<string>("toname"),
                        content = c.Field<string>("content"),
                        create_time = c.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                    }),
                    src_urls = ds.Tables[3].AsEnumerable().Select(c => new
                    {
                        url = c.Field<string>("url")
                    })

                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[1].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 健康社区

        /// <summary>
        /// 获取健康社区话题分类
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetCommunitySections()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getcommunitysections");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "获取失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                //section_id,name
                var lst = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    section_id = x.Field<int>("section_id"),
                    section_name = x.Field<string>("name")
                }).ToList();

                bmrow = new BaseModel<object>(0, "", lst);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取社区所有的主题信息
        /// </summary>
        public JsonResult uGetCommunityTopics()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@section_id", GetValue("section_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getcommunitytopics");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //topic_id,section_id,userid,title,content,create_time,like_cnt,comment_cnt
                    //section_name,realname,avatar,gender,attention
                    topic_id = x.Field<long>("topic_id"),
                    section_id = x.Field<int>("section_id"),
                    section_name = x.Field<string>("section_name"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    avatar = x.Field<string>("avatar"),
                    gender = x.Field<int>("gender"),
                    attention = x.Field<int>("attention"),
                    title = x.Field<string>("title"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    self_like = x.Field<int>("self_like"),
                    sources = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("topic_id") == x.Field<long>("topic_id")).Select(s => new
                    {
                        //topic_id,s.id,s.url,s.src_type,s.state
                        id = s.Field<long>("id"),
                        url = s.Field<string>("url"),
                        src_type = s.Field<int>("src_type"),
                        state = s.Field<int>("state")
                    })
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发布社区文章
        /// </summary>
        public JsonResult AddCommunityTopic()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@section_id", GetValue("section_id"))); //社区话题类型id
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //发布人
            tv.Add(new TextAndValue("@title", GetValue("title")));
            tv.Add(new TextAndValue("@content", GetValue("content")));
            tv.Add(new TextAndValue("@source_urls", GetValue("source_urls")));//图片urls，使用“,”分隔
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_AddCommunityTopic"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 评论话题
        /// </summary>
        public JsonResult uAddTopicComment()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@topic_id", GetValue("topic_id"))); //话题id
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //评论人
            tv.Add(new TextAndValue("@touid", GetValue("touid")));       //被评论人（touid=0为评论话题)
            tv.Add(new TextAndValue("@content", GetValue("content")));
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddTopicComment"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增话题点赞
        /// </summary>
        public JsonResult uAddTopicLike()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@topic_id", GetValue("topic_id"))); //资讯id
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //评论人
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddTopicLike"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 资讯
        /// <summary>
        /// 获取最新5条资讯（用于首页展示）
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetTop5Infos()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gettop5infos");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "获取失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                //info_id,title,info_type,author,cover,content,src_url,lk.,ict.comment_cnt
                var lst = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    info_id = x.Field<int>("info_id"),
                    title = x.Field<string>("title"),
                    info_type = x.Field<int>("info_type"),
                    author = x.Field<string>("author"),
                    cover = x.Field<string>("cover"),
                    content = x.Field<string>("content"),
                    src_url = x.Field<string>("src_url"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    has_like = x.Field<int>("has_like")
                }).ToList();

                bmrow = new BaseModel<object>(0, "", lst);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取热门资讯
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetHotInfo()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("info_type", GetValue("info_type")));  //info_type int  --0:健康资讯、1:新闻热点,2:视频 
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_gethotinfo");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "获取失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                //info_id,title,info_type,author,cover,content,src_url,lk.,ict.comment_cnt
                var info = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    info_id = x.Field<int>("info_id"),
                    title = x.Field<string>("title"),
                    info_type = x.Field<int>("info_type"),
                    author = x.Field<string>("author"),
                    cover = x.Field<string>("cover"),
                    content = x.Field<string>("content"),
                    src_url = x.Field<string>("src_url"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    has_like = x.Field<int>("has_like")
                }).FirstOrDefault();

                bmrow = new BaseModel<object>(0, "", info);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取资讯列表
        /// </summary>
        /// <returns></returns>
        public JsonResult uGetInfos()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("hot_info_id", GetValue("hot_info_id")));  //热门资讯id（排除热门资讯）
            tv.Add(new TextAndValue("info_type", GetValue("info_type")));  //info_type int  --1:健康资讯、2:新闻热点,3:视频 
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getinfos");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "获取失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                //info_id,title,info_type,author,cover,content,src_url,lk.,ict.comment_cnt
                var lst = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    info_id = x.Field<int>("info_id"),
                    title = x.Field<string>("title"),
                    info_type = x.Field<int>("info_type"),
                    author = x.Field<string>("author"),
                    cover = x.Field<string>("cover"),
                    content = x.Field<string>("content"),
                    src_url = x.Field<string>("src_url"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    has_like = x.Field<int>("has_like")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), lst), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取资讯详情
        /// </summary>
        public JsonResult uGetInfo()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@info_id", GetValue("info_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appu_getInfo");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    info_id = x.Field<int>("info_id"),
                    title = x.Field<string>("title"),
                    info_type = x.Field<int>("info_type"),
                    author = x.Field<string>("author"),
                    cover = x.Field<string>("cover"),
                    content = x.Field<string>("content"),
                    src_url = x.Field<string>("src_url"),
                    like_cnt = x.Field<int>("like_cnt"),
                    comment_cnt = x.Field<int>("comment_cnt"),
                    has_like = x.Field<int>("has_like"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    commands = ds.Tables[2].AsEnumerable().Select(c => new
                    {
                        id = c.Field<long>("id"),
                        userid = c.Field<long>("userid"),
                        realname = c.Field<string>("realname"),
                        touid = c.Field<long>("touid"),
                        toname = c.Field<string>("toname"),
                        content = c.Field<string>("content"),
                        create_time = c.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                    })

                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[1].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增资讯评论
        /// </summary>
        public JsonResult uAddInfoComment()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@info_id", GetValue("info_id"))); //资讯id
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //评论人
            tv.Add(new TextAndValue("@touid", GetValue("touid")));       //被评论人（touid=0为评论话题)
            tv.Add(new TextAndValue("@content", GetValue("content")));
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddInfoComment"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增资讯点赞
        /// </summary>
        public JsonResult uAddInfoLike()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@info_id", GetValue("info_id"))); //资讯id
            tv.Add(new TextAndValue("@userid", GetValue("userid")));     //评论人
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appu_AddInfoLike"), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 护工招聘

        /// <summary>
        /// 申请陪诊、护工、护士
        /// </summary>
        public JsonResult ApplyNurse()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid"))); //申请人
            tv.Add(new TextAndValue("@name", GetValue("realname")));     //真实姓名
            tv.Add(new TextAndValue("@avatar", GetValue("avatar")));
            tv.Add(new TextAndValue("@idcard", GetValue("idcard")));
            tv.Add(new TextAndValue("@birthdate", GetValue("birthdate")));
            tv.Add(new TextAndValue("@gender", GetValue("gender")));
            tv.Add(new TextAndValue("@idcard_1", GetValue("idcard_1")));
            tv.Add(new TextAndValue("@idcard_2", GetValue("idcard_2")));
            tv.Add(new TextAndValue("@certificate_1", GetValue("certificate_1")));
            tv.Add(new TextAndValue("@certificate_2", GetValue("certificate_2")));
            tv.Add(new TextAndValue("@job", GetValue("job"))); //1陪诊，2护工/护士

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_applynurse"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取用户申请信息
        /// </summary>
        public JsonResult getUserApply()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid"))); //申请人
            tv.Add(new TextAndValue("@job", GetValue("job"))); //1陪诊，2护工/护士

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_getuserapply");
            BaseModelEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    userid = x.Field<long>("userid"),
                    account = x.Field<string>("account"),
                    realname = x.Field<string>("name"),
                    avatar = x.Field<string>("avatar"),
                    idcard = x.Field<string>("idcard"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    gender = x.Field<int>("gender"),
                    idcard_1 = x.Field<string>("idcard_1"),
                    idcard_2 = x.Field<string>("idcard_2"),
                    certificate_1 = x.Field<string>("certificate_1"),
                    certificate_2 = x.Field<string>("certificate_2"),
                    audit_state = x.Field<int>("audit_state")  //-2 审核失败，-1 未申请  0 审核中， 1 审核通过
                }).FirstOrDefault();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// 登陆(用户端)
        /// </summary>
        /// <returns></returns>
        public JsonResult nLogin()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("account", GetValue("account")));
            tv.Add(new TextAndValue("password", GetValue("password")));
            tv.Add(new TextAndValue("@device_id", GetValue("device_id")));
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_Login");
            BaseModel<object> bmrow = null;

            if (ds == null || ds.Tables.Count <= 0)
            {
                bmrow = new BaseModel<object>(-1, "登陆失败", new object());
            }
            else if (ds.Tables.Count > 0)
            {
                string retMsg = ds.Tables[0].Rows[0][0].ToString();
                if (retMsg != "0")
                {
                    bmrow = new BaseModel<object>(1, ds.Tables[0].Rows[0][1].ToString(), new object());
                }
                else
                {
                    //用户基本信息
                    var user = ds.Tables[1].AsEnumerable().Select(x => new
                    {
                        userid = x.Field<long>("userid"),
                        account = x.Field<string>("account"),
                        realname = x.Field<string>("realname"),
                        nickname = x.Field<string>("nickname"),
                        avatar = x.Field<string>("avatar"),
                        gender = x.Field<int>("gender"),
                        birthdate = x.Field<string>("birthdate"),
                        tel = x.Field<string>("tel"),
                        idcard = x.Field<string>("idcard"),
                        age = x.Field<int>("age"),
                        star = x.Field<int>("star"),
                        area = x.Field<int>("area"),
                        area_name = x.Field<string>("area_name"),
                        jobs = x.Field<string>("jobs")

                    }).FirstOrDefault();

                    bmrow = new BaseModel<object>(0, "", user);
                }
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取病患监护列表
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetPatientMonitorings()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));       //用户端登陆人userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getPatientMonitorings");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    o_type = x.Field<int>("o_type"),
                    n_type = x.Field<int>("n_type"),
                    acceptor = x.Field<long>("acceptor"),
                    state = x.Field<int>("state"),
                    hospitalid = x.Field<int>("hospitalid"),
                    //hospital_name = x.Field<string>("hospital_name"),
                    room_num = x.Field<string>("room_num"),
                    bed_num = x.Field<string>("bed_num"),
                    in_time = x.Field<DateTime>("in_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    area = x.Field<int>("area"),
                    addr = x.Field<string>("addr"),
                    p_username = x.Field<string>("p_username"),
                    gender = x.Field<int>("gender"),
                    avatar = x.Field<string>("avatar"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    idcard = x.Field<string>("idcard"), 
                    tel = x.Field<string>("tel")

                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取病患详情
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetPatientDetail()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));  //订单id

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getPatientDetail");
            BaseReturn<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    o_type = x.Field<int>("o_type"),
                    n_type = x.Field<int>("n_type"),
                    acceptor = x.Field<long>("acceptor"),
                    state = x.Field<int>("state"),
                    hospitalid = x.Field<int>("hospitalid"),
                    hospital_name = x.Field<string>("hospital_name"),
                    room_num = x.Field<string>("room_num"),
                    bed_num = x.Field<string>("bed_num"),
                    in_time = x.Field<DateTime>("in_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    area = x.Field<int>("area"),
                    addr = x.Field<string>("addr"),
                    p_username = x.Field<string>("p_username"),
                    gender = x.Field<int>("gender"),
                    avatar = x.Field<string>("avatar"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    idcard = x.Field<string>("idcard"),
                    tel = x.Field<string>("tel"),
                    s_type_cnt = x.Field<int>("s_type_cnt")  //智能设备购买数量

                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新住院信息
        /// </summary>
        /// <returns></returns>
        public JsonResult nUpdateHospitalized()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@room_num", GetValue("room_num")));
            tv.Add(new TextAndValue("@bed_num", GetValue("bed_num")));
            tv.Add(new TextAndValue("@in_time", GetValue("in_time")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_update_hospitalized"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取诊断记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetDiagnosis()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //护工userid
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getdiagnosis");
            BaseReturn<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    diagnosis_id = x.Field<long>("diagnosis_id"),
                    orderid = x.Field<long>("orderid"),
                    postion = x.Field<string>("postion"),
                    in_time = x.Field<DateTime>("in_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    out_time = x.Field<DateTime>("out_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    descr = x.Field<string>("descr"),
                    avatar = x.Field<string>("avatar"),
                    acceptor = x.Field<long>("acceptor"),
                    acceptor_name = x.Field<string>("acceptor_name"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除诊断记录
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult nDeleteDiagnosis()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@diagnosis_id", GetValue("diagnosis_id")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_delete_diagnosis"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 新增诊断记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nAddDiagnosis()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@postion", GetValue("postion")));
            tv.Add(new TextAndValue("@in_time", GetValue("in_time")));
            tv.Add(new TextAndValue("@out_time", GetValue("out_time")));
            tv.Add(new TextAndValue("@descr", GetValue("descr")));

            
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_add_diagnosis"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取病患记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetScadas()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //护工userid
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid

            //scada_type int,  --病患采集类型（1体温,2血压,3血糖,4胎心仪,5心电图,6血氧计,7体脂称）
            tv.Add(new TextAndValue("@scada_type", GetValue("scada_type")));
            tv.Add(new TextAndValue("@bgntime", GetValue("bgntime")));
            tv.Add(new TextAndValue("@endtime", GetValue("endtime")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getscadas");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //orderid,scada_type,deviceid,record_time,value1,value2
                    orderid = x.Field<long>("orderid"),
                    scada_type = x.Field<int>("scada_type"),
                    deviceid = x.Field<string>("deviceid"),
                    record_time = x.Field<DateTime>("record_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    value1 = ParseValueFloat(x["value1"].ToString()),
                    value2 = ParseValueFloat(x["value2"].ToString()),
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 新增病患记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nAddScada()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@scada_type", GetValue("scada_type")));
            tv.Add(new TextAndValue("@deviceid", GetValue("deviceid")));
            tv.Add(new TextAndValue("@record_time", GetValue("record_time")));
            tv.Add(new TextAndValue("@value1", GetValue("value1")));
            tv.Add(new TextAndValue("@value2", GetValue("value2")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_add_scada"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取用药记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetMedicines()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //护工userid
            tv.Add(new TextAndValue("@p_userid", GetValue("p_userid")));   //病人userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getmedicines");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    omid = x.Field<long>("omid"),
                    orderid = x.Field<long>("orderid"),
                    take_time = x.Field<DateTime>("take_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    medicine_name = x.Field<string>("medicine_name"),
                    dosage = x.Field<string>("dosage"),
                    avatar = x.Field<string>("avatar"),
                    acceptor = x.Field<long>("acceptor"),
                    acceptor_name = x.Field<string>("acceptor_name")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增用药记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nAddMedicine()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@take_time", GetValue("take_time")));
            tv.Add(new TextAndValue("@medicine_name", GetValue("medicine_name")));
            tv.Add(new TextAndValue("@dosage", GetValue("dosage")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_add_medicine"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 删除用药记录
        /// </summary>
        /// <returns></returns>
        public JsonResult nDeleteMedicine()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@omid", GetValue("omid")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_delete_medicine"), JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 获取智能硬件
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetHospitalDevices()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));   //订单id
            tv.Add(new TextAndValue("@hospital_id", GetValue("hospital_id")));   //医院id

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_gethospitaldevices");
            BaseReturn<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var hosp_hardware_service = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    id = x.Field<int>("id"),
                    hospitalid = x.Field<int>("hospitalid"),
                    s_type = x.Field<int>("s_type")
                });
                var hosp_device = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    deviceid = x.Field<int>("deviceid"),
                    s_type = x.Field<int>("s_type"),
                    hospitalid = x.Field<int>("hospitalid"),
                    model_number = x.Field<string>("model_number"),
                    serial_number = x.Field<string>("serial_number")
                });
                var order_hosp_device = ds.Tables[2].AsEnumerable().Select(x => new
                {
                    id = x.Field<long>("id"),
                    orderid = x.Field<long>("orderid"),
                    deviceid = x.Field<int>("deviceid")
                });

                /*--hosp_hardware_service 医院开通的智能硬件
                select id,hospitalid,s_type,cost,unit from hosp_hardware_service where hospitalid=@hospital_id

                --hosp_device 医院拥有的智能硬件
                select deviceid,s_type,hospitalid,model_number,serial_number,state
                
                --order_hosp_device 订单购买的智能硬件
                select id,orderid,deviceid,create_time from order_hosp_device ohd where orderid=@orderid
                 * */

                return this.Json(new BaseModel<object>(0, "", new {
                    hosp_hardware_service = hosp_hardware_service,
                    hosp_device = hosp_device,
                    order_hosp_device = order_hosp_device
                }), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }


        #region 订单中心 (护工端)
        /// <summary>
        /// 获取用户订单信息
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetOrders()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));       //用户端登陆人userid
            tv.Add(new TextAndValue("@o_type", GetValue("o_type")));       //订单类型(1医院陪护,2家庭陪护,3陪诊)
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getorders");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //1、订单详情
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),    //订单编号
                    p_userid = x.Field<long>("p_userid"),    //病人id
                    userid = x.Field<long>("userid"),        //护工/护士id
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),   //订单开始时间
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),       //订单结束时间

                    o_type = x.Field<int>("o_type"),                //订单类型(1医院陪护,2家庭陪护,3陪诊)
                    n_type = x.Field<int>("n_type"),                //护理项目类型
                    acceptor = x.Field<long>("acceptor"),           //接单人id
                    state = x.Field<int>("state"),                  //订单状态
                    hospitalid = x.Field<int>("hospitalid"),        //医院id
                    hospital_name = x.Field<string>("hospital_name"),  //医院名称
                    area = x.Field<int>("area"),                     //市id
                    addr = x.Field<string>("addr"),                  //地址
                    pickup = x.Field<int>("pickup"),                 //是否接送（陪诊才有，其他为0）
                    pickup_cost = ParseValueFloat(x["pickup_cost"].ToString()),  //接送费用
                    nursing_unit_price = ParseValueFloat(x["nursing_unit_price"].ToString()),  //护理类型价格（只有医院陪护才有）
                    nursing_cnt = x.Field<int>("nursing_cnt"),            //护理次数（只有医院陪护才有）
                    cost = ParseValueFloat(x["cost"].ToString()),         //总金额
                    acceptor_name = x.Field<string>("acceptor_name"),     //接单人姓名
                    hospital_avatar = x.Field<string>("hospital_avatar"),  //医院封面
                    time_descr = x.Field<string>("time_descr"),            //服务时段说明
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss"),  //订单创建时间
                    accept_time = x.Field<DateTime>("accept_time").ToString("yyyy-MM-dd HH:mm:ss"),  //接单时间
                    refund_reason = x.Field<string>("refund_reason"),
                    refund_cost = ParseValueFloat(x["refund_cost"].ToString()),
                    //2、护理项目
                    services = ds.Tables[2].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        hospitalid = s.Field<long>("hospitalid"),  //医院id
                        s_type = s.Field<int>("s_type"),          //护理项目类型
                        cost = ParseValueFloat(s["cost"].ToString()), //金额
                        unit = s.Field<string>("unit")            //单位
                    }),
                    //3、下单人信息
                    user = ds.Tables[3].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        realname = s.Field<string>("name"),    //姓名
                        gender = s.Field<int>("gender"),       //性别
                        avatar = s.Field<string>("avatar"),    //头像
                        birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),  //出生日期
                        age = s.Field<int>("age")              //年龄
                    }).FirstOrDefault(),
                    //4、患者信息
                    patient = ds.Tables[4].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        realname = s.Field<string>("name"),   //姓名
                        gender = s.Field<int>("gender"),      //性别 
                        avatar = s.Field<string>("avatar"),   //头像
                        birthdate = s.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"), //出生日期
                        age = s.Field<int>("age"),  //年龄
                        idcard = s.Field<string>("idcard"),  //身份证号
                        tel = s.Field<string>("tel")  //手机号
                    }).FirstOrDefault(),
                    //5、备注信息
                    remarks = ds.Tables[5].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        or_type = s.Field<int>("or_type"),      //备注类型
                        content = s.Field<string>("content"),   //备注内容
                        audio_url = s.Field<string>("audio_url")//备注语音
                    }),
                    //6、订单评论
                    comment = ds.Tables[6].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Count() > 0 ?
                    ds.Tables[6].AsEnumerable().Where(w => w.Field<long>("orderid") == x.Field<long>("orderid")).Select(s => new
                    {
                        content = s.Field<string>("content"),
                        star = s.Field<int>("star")
                    }).FirstOrDefault() : new { content = "", star = 0 }
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改订单状态 
        /// </summary>
        /// <returns>0成功 -1失败</returns>
        public JsonResult nUpdateOrderState()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            tv.Add(new TextAndValue("@state", GetValue("state")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_updateorderstate"), JsonRequestBehavior.AllowGet);

        }

        //拒绝接单 
        public JsonResult nRefuseOrder()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_RefuseOrder"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取某个订单，护工/护士的评论信息  
        /// </summary>
        /// <returns></returns>
        public JsonResult GetOrderComment()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_getordercomment");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    //cmtid,userid,realname,touid,toname,avatar,birthdate,age,star,content,create_time,seniority
                    cmtid = x.Field<long>("cmtid"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    touid = x.Field<long>("touid"),
                    toname = x.Field<string>("toname"),
                    avatar = x.Field<string>("avatar"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    star = x.Field<int>("star"),
                    content = x.Field<string>("content"),
                    seniority = x.Field<int>("seniority")
                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        #endregion 

        /// <summary>
        /// 获取个人评价
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetOrderComments()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));   //护工userid
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getordercomments");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    // c.cmtid,c.userid,c.name realname,c.star,c.content,c.create_time
                    cmtid = x.Field<long>("cmtid"),
                    userid = x.Field<long>("userid"),
                    realname = x.Field<string>("realname"),
                    star = x.Field<int>("star"),
                    content = x.Field<string>("content"),
                    create_time = x.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }

            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增反馈记录
        /// </summary>
        /// <returns></returns>
        public JsonResult Feedback()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@content", GetValue("content")));
            tv.Add(new TextAndValue("@app", GetValue("app", "2")));  //1：用户端，2：护工端

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_add_feedback"), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 设置服务价格
        /// </summary>
        /// <returns></returns>
        public JsonResult SetServicePrice()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));//用户端登陆人userid
            tv.Add(new TextAndValue("@job", GetValue("job")));// 1陪诊，2护工，3护士  （废弃）
            tv.Add(new TextAndValue("services", GetValue("services")));//护理项目及金额，多个用逗号隔开   s_type|cost,s_type|cost,s_type|cost,s_type|cost
            
            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.app_set_service_price"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取服务价格
        /// </summary>
        /// <returns></returns>
        public JsonResult GetServicePrice()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("userid", GetValue("userid")));
            //护理项目及金额，多个用逗号隔开   s_type|cost,s_type|cost,s_type|cost,s_type|cost
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_get_service_price");
            BaseModel<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(s => new
                {
                    //userid,s_type,cost,unit
                    userid = s.Field<long>("userid"),  //医院id
                    s_type = s.Field<int>("s_type"),          //护理项目类型
                    cost = ParseValueFloat(s["cost"].ToString()), //金额
                    unit = s.Field<string>("unit")            //单位
                }).ToList();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取用户所在城市的医院列表(护工端)
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetHospitals()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@hospital_name", GetValue("hospital_name")));
            tv.Add(new TextAndValue("@city_id", GetValue("city_id")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_GetHospitals");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    id = x.Field<int>("id"),
                    hospital_name = x.Field<string>("name"),
                    lv = x.Field<string>("lv"),
                    avatar = x.Field<string>("avatar"),
                    province_id = x.Field<int>("province_id"),
                    province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    district_id = x.Field<int>("district_id"),
                    district = x.Field<string>("district"),
                    addr = x.Field<string>("addr"),
                    contact = x.Field<string>("contact"),
                    tel = x.Field<string>("tel"),
                    nurse_cnt = x.Field<int>("nurse_cnt")
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()), r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 获取个人资料
        /// </summary>
        public JsonResult nGetUserInfo()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid"))); //申请人
            //tv.Add(new TextAndValue("@job", GetValue("job"))); //1陪诊，2护工/护士

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getuserinfo");
            BaseModelEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    userid = x.Field<long>("userid"),
                    account = x.Field<string>("account"),
                    realname = x.Field<string>("realname"),
                    nickname = x.Field<string>("nickname"),
                    avatar = x.Field<string>("avatar"),
                    gender = x.Field<int>("gender"),
                    tel = x.Field<string>("tel"),
                    idcard = x.Field<string>("idcard"),
                    birthdate = x.Field<DateTime>("birthdate").ToString("yyyy-MM-dd HH:mm:ss"),
                    age = x.Field<int>("age"),
                    star = x.Field<int>("star"),
                    area = x.Field<int>("area"),
                    area_name = x.Field<string>("area_name"),
                    hospitalid = x.Field<int>("hospitalid"),
                    hospital_name = x.Field<string>("hospital_name"),
                    bio = x.Field<string>("bio"),
                    jobs = x.Field<string>("jobs")
                }).FirstOrDefault();

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改个人简介
        /// </summary>
        public JsonResult nUpdateUserIntroduce()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@bio", GetValue("bio")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_UpdateUserIntroduce"), JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// 修改所在医院
        ///// </summary>
        //public JsonResult nUpdateUserHospital()
        //{
        //    IList<TextAndValue> tv = new List<TextAndValue>();
        //    tv.Add(new TextAndValue("@userid", GetValue("userid")));
        //    tv.Add(new TextAndValue("@hospitalid", GetValue("hospitalid")));

        //    return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_UpdateUserHospital"), JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// 修改所在地区
        /// </summary>
        public JsonResult nUpdateUserCity()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@cityid", GetValue("cityid")));

            return this.Json(KinProxyData.Execute(tv, dbname + ".dbo.appn_UpdateUserCity"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetCitys()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@appver", GetValue("appver")));
            tv.Add(new TextAndValue("@client", GetValue("client")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_GetCitys");
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "获取失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else
            {
                //城市列表
                var citys = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    // id,province_id,province,city_id,city,district_id,district,lv,is_hot
                    //id = x.Field<int>("id"),
                    //province_id = x.Field<int>("province_id"),
                    //province = x.Field<string>("province"),
                    city_id = x.Field<int>("city_id"),
                    city = x.Field<string>("city"),
                    //district_id = x.Field<int>("district_id"),
                    //district = x.Field<string>("district"),
                    //lv = x.Field<int>("lv"),
                    //is_hot = x.Field<int>("is_hot")
                });

                return this.Json(new BaseModel<object>(0, "", citys), JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult GetRefundDetail()
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_get_refund_detail");
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "获取失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var r = ds.Tables[0].AsEnumerable().Select(x => new
                {
                    orderid = x.Field<long>("orderid"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    refund_cost = ParseValueFloat(x["refund_cost"].ToString()),
                    refund_reason = x.Field<string>("refund_reason"),
                    refund_time = x.Field<DateTime>("refund_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    details=ds.Tables[1].AsEnumerable().Select(d => new{
                        orderid = d.Field<long>("orderid"),
                        remark = d.Field<string>("remark"),
                        state = d.Field<int>("state"),
                        create_time = d.Field<DateTime>("create_time").ToString("yyyy-MM-dd HH:mm:ss")
                    })
                });

                return this.Json(new BaseModel<object>(0, "", r), JsonRequestBehavior.AllowGet);

            }

        }

        private OrderInfo GetOrderInfo(string orderid)
        {
            OrderInfo order = null;
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", orderid));
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_getorderinfo");
            if (ds != null && ds.Tables.Count > 0)
            {
                order = ds.Tables[0].AsEnumerable().Select(x => new OrderInfo
                {
                    orderid = x.Field<long>("orderid"),
                    orderno = x.Field<string>("orderno"),
                    p_userid = x.Field<long>("p_userid"),
                    userid = x.Field<long>("userid"),
                    begin_time = x.Field<DateTime>("begin_time").ToString("yyyy-MM-dd HH:mm:ss"),
                    end_time = x.Field<DateTime>("end_time").ToString("yyyy-MM-dd HH:mm:ss"),

                    o_type = x.Field<int>("o_type"),
                    n_type = x.Field<int>("n_type"),
                    acceptor = x.Field<long>("acceptor"),
                    state = x.Field<int>("state"),
                    hospitalid = x.Field<int>("hospitalid"),
                    hospital_name = x.Field<string>("hospital_name"),
                    area = x.Field<int>("area"),
                    addr = x.Field<string>("addr"),
                    pickup = x.Field<int>("pickup"),
                    pickup_cost = ParseValueFloat(x["pickup_cost"].ToString()),
                    nursing_unit_price = ParseValueFloat(x["nursing_unit_price"].ToString()),
                    nursing_cnt = x.Field<int>("nursing_cnt"),
                    cost = ParseValueFloat(x["cost"].ToString())
                }).FirstOrDefault();
            }

            return order;
        }

        /// <summary>
        /// 获取用户收入(护工端)
        /// </summary>
        /// <returns></returns>
        public JsonResult nGetIncomes()
        {
            string bgndate = GetValue("bgndate");
            string enddate = GetValue("enddate");

            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            if (!DateTime.TryParse(bgndate + "-01", out start))
            {
                start = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
            }

            if (!DateTime.TryParse(enddate + "-01", out end))
            {
                end = DateTime.Parse(DateTime.Now.AddMonths(1).ToString("yyyy-MM-01"));
            }
            else
            {
                end = end.AddMonths(1);
            }

            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@userid", GetValue("userid")));
            tv.Add(new TextAndValue("@bgndate", start.ToString("yyyy-MM-01")));
            tv.Add(new TextAndValue("@enddate", end.ToString("yyyy-MM-01")));
            tv.Add(new TextAndValue("@fetchnum", GetValue("fetchnum")));
            tv.Add(new TextAndValue("@nextid", GetValue("nextid")));

            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.appn_getincomes");
            BaseReturnEx<object> bmrow = null;
            if (ds == null || ds.Tables.Count <= 0)
            {
                return this.Json(new BaseModel<object>(1, "请求失败", new object()), JsonRequestBehavior.AllowGet);
            }
            else if (ds.Tables.Count > 0)
            {
                var royaltyrates = ds.Tables[1].AsEnumerable().Select(x => new
                {
                    //userid,hospitalid,job,royalty_rate
                    userid = x.Field<long>("userid"),
                    hospitalid = x.Field<int>("hospitalid"),
                    job = x.Field<int>("job"),  //job: 1 陪诊,2	护工,3 护士
                    royalty_rate = ParseValueFloat(x["royalty_rate"].ToString())
                }).ToList();

                var incomes = ds.Tables[2].AsEnumerable().Select(x => new
                {
                    //orderid,orderno,cost,nurse_income
                    orderno = x.Field<string>("orderno"),
                    cost = ParseValueFloat(x["cost"].ToString()),
                    nurse_income = ParseValueFloat(x["nurse_income"].ToString())
                }).ToList();

                return this.Json(new BaseModelEx<object>(0, "", int.Parse(ds.Tables[0].Rows[0]["nextid"].ToString()),
                    new {
                        royaltyrates = royaltyrates,
                        incomes = incomes
                    }), JsonRequestBehavior.AllowGet);
            }
            return this.Json(bmrow, JsonRequestBehavior.AllowGet);


        }

        private void UpdateRefundState(string out_trade_no, int state)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@out_trade_no", out_trade_no));
            tv.Add(new TextAndValue("@state", state.ToString()));

            KinProxyData.Execute(tv, dbname + ".dbo.app_UpdateRefundState");
        }

        #region 支付宝

        public JsonResult AliPay() 
        {
            //string title = GetValue("title", "");
            //string content = GetValue("content", "");
            //double amount=GetValueDouble("amount",0);
            //string out_trade_no = GetValue("out_trade_no", "");

            string title = "";
            string content = "";
            string orderid = GetValue("orderid", "");
            OrderInfo or = GetOrderInfo(orderid);
            if (or != null)
            {
                if (or.o_type == 1)
                {
                    title = "医院陪护费用";
                    content = "医院陪护费用";
                }
                else if (or.o_type == 1)
                {
                    title = "家庭陪护费用";
                    content = "家庭陪护费用";
                }
                else
                {
                    title = "陪诊费用";
                    content = "陪诊费用";
                }
                AlipayTradeAppPayResponse response = AliOrder(title, content, or.cost, or.orderno);
                if (!response.IsError)
                {
                    //修改支付方式
                    IList<TextAndValue> tv = new List<TextAndValue>();
                    tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
                    tv.Add(new TextAndValue("@paytype", "2"));  //1：微信，2：支付宝 
                    KinProxyData.Execute(tv, dbname + ".dbo.app_updatepaytype");
                    return this.Json(new
                    {
                        code = 0,
                        info = "",
                        result = new
                        {
                            trade_no = response.TradeNo ?? "",
                            out_trade_no = response.OutTradeNo ?? "",
                            body = response.Body ?? ""
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return this.Json(new
                    {
                        code = -1,
                        info = response.SubMsg ?? "",
                        result = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = "获取订单失败",
                    result = new { }
                });
            }
        }

        public AlipayTradeAppPayResponse AliOrder(string title, string content, double amount, string out_trade_no)
        {
            IAopClient client = new DefaultAopClient(AlipayConfig.GATEWAY, AlipayConfig.APP_ID, AlipayConfig.APP_PRIVATE_KEY, "json", "1.0", AlipayConfig.SIGN_TYPE, AlipayConfig.ALIPAY_PUBLIC_KEY, AlipayConfig.Input_charset, false);
            //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：alipay.trade.app.pay
            AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            request.SetNotifyUrl(AlipayConfig.ALIPAY_Notify_Url);
            //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
            AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
            model.Body = content;                    // "我是测试数据";
            model.Subject = title;                   // "App支付测试DoNet";
            model.TotalAmount = String.Format("{0:F}",amount); //amount.ToString();   // "0.01";
            model.ProductCode = "QUICK_MSECURITY_PAY";
            model.OutTradeNo = out_trade_no;         // "20170216test01";
            model.TimeoutExpress = "30m";
            request.SetBizModel(model);

            
            AlipayTradeAppPayResponse response = client.SdkExecute(request);

            //string jsonstr = FastJSON.JSON.Beautify( FastJSON.JSON.ToJSON(response));
            //log4net.LogManager.GetLogger("AliOrder").Info(jsonstr);

            ////HttpUtility.HtmlEncode是为了输出到页面时防止被浏览器将关键参数html转义，实际打印到日志以及http传输不会有这个问题
            //Response.Write(HttpUtility.HtmlEncode(response.Body));
            ////页面输出的response.Body就是orderString 可以直接给客户端请求，无需再做处理。

            //return HttpUtility.HtmlEncode(response.Body);
            return response;
        }

        /// <summary>
        /// 异步通知
        /// </summary>
        /// <returns></returns>
        public ActionResult AliNotify()
        {
            /*1、商户需要验证该通知数据中的out_trade_no是否为商户系统中创建的订单号，
             * 2、判断total_amount是否确实为该订单的实际金额（即商户订单创建时的金额），
             * 3、校验通知中的seller_id（或者seller_email) 是否为out_trade_no这笔单据的对应的操作方（有的时候，一个商户可能有多个seller_id/seller_email），
             * 4、验证app_id是否为该商户本身。
             * 上述1、2、3、4有任何一个验证不通过，则表明本次通知是异常通知，务必忽略。在上述验证通过后商户必须根据支付宝不同类型的业务通知，正确的进行不同的业务处理，
             * 并且过滤重复的通知结果数据。在支付宝的业务通知中，只有交易通知状态为TRADE_SUCCESS或TRADE_FINISHED时，支付宝才会认定为买家付款成功。
             */

            //切记alipaypublickey是支付宝的公钥，请去open.alipay.com对应应用下查看。
            //bool RSACheckV1(IDictionary<string, string> parameters, string alipaypublicKey, string charset, string signType, bool keyFromFile)

            Dictionary<string, string> dict = GetRequestPost();
            bool flag = AlipaySignature.RSACheckV1(dict, AlipayConfig.ALIPAY_PUBLIC_KEY, AlipayConfig.Input_charset, AlipayConfig.SIGN_TYPE, false);
            if (flag)
            {
                string out_trade_no = dict["out_trade_no"];
                string trade_no = dict["trade_no"];
                string trade_status = dict["trade_status"].ToUpper();
                // TODO 验签成功后
                //按照支付结果异步通知中的描述，对支付结果中的业务内容进行1\2\3\4二次校验，校验成功后在response中返回success，校验失败返回failure
                //IList<TextAndValue> tv = new List<TextAndValue>();
                //tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
                //DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.app_get_refund_detail");
                //if (ds == null || ds.Tables.Count <= 0)
                //{
                //    return this.Json(new BaseModel<object>(1, "获取失败", new object()), JsonRequestBehavior.AllowGet);
                //}
                //else
                {
                    if (trade_status == "TRADE_FINISHED")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        //注意：
                        //该种交易状态只在两种情况下出现
                        //1、开通了普通即时到账，买家付款成功后。
                        //2、开通了高级即时到账，从该笔交易成功时间算起，过了签约时的可退款时限（如：三个月以内可退款、一年以内可退款等）后。
                        log4net.LogManager.GetLogger("AliNotify").Info("Alipay notify_url out_trade_no:" + out_trade_no);

                        PaySuccess(out_trade_no, trade_no);

                    }
                    else if (trade_status == "TRADE_SUCCESS")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        //注意：
                        //该种交易状态只在一种情况下出现——开通了高级即时到账，买家付款成功后。
                        log4net.LogManager.GetLogger("AliNotify").Info("Alipay notify_url out_trade_no:" + out_trade_no);
                        PaySuccess(out_trade_no, trade_no);
                    }
                }

                return Content("success");
            }
            else
            {
                return Content("failure");

            }
        }

        private BaseModel<IList<string>> PaySuccess(string out_trade_no, string trade_no)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@out_trade_no", out_trade_no));
            tv.Add(new TextAndValue("@trade_no", trade_no));

            BaseModel<IList<string>> bm = KinProxyData.Execute(tv, dbname + ".dbo.app_PaySuccess");
            return bm;
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <returns></returns>
        public JsonResult AliQuery()
        {
            string out_trade_no = GetValue("out_trade_no","");
            string trade_no = GetValue("trade_no", "");
            IAopClient client = new DefaultAopClient(AlipayConfig.GATEWAY, AlipayConfig.APP_ID, AlipayConfig.APP_PRIVATE_KEY, "json", "1.0", AlipayConfig.SIGN_TYPE, AlipayConfig.ALIPAY_PUBLIC_KEY, AlipayConfig.Input_charset, false);
            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();//创建API对应的request类

            AlipayTradeQueryModel model = new AlipayTradeQueryModel();

            model.TradeNo = trade_no;	              //支付宝28位交易号
            model.OutTradeNo = out_trade_no;          //支付时传入的商户订单号
            request.SetBizModel(model);

            AlipayTradeQueryResponse response = client.Execute(request);//通过alipayClient调用API，获得对应的response类

            //return HttpUtility.HtmlEncode(response.Body);

            string jsonstr = FastJSON.JSON.Beautify(FastJSON.JSON.ToJSON(response));
            log4net.LogManager.GetLogger("AliQuery").Info(jsonstr);

            if (!response.IsError)
            {
                string result = response.Body;
                return this.Json(new
                {
                    code = 0,
                    info = "",
                    result = new
                    {
                        trade_no = response.TradeNo ?? "",
                        out_trade_no = response.OutTradeNo ?? "",
                        trade_status = response.TradeStatus ?? "",
                        body = response.Body ?? ""
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = response.SubMsg,
                    result = new{}
                }, JsonRequestBehavior.AllowGet);
            }

            //根据response中的结果继续业务逻辑处理
            //trade_no      支付宝28位交易号
            //out_trade_no	支付时传入的商户订单号
            //trade_status	交易当前状态
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <returns></returns>
        public JsonResult AliRefund()
        {
            string out_trade_no = GetValue("out_trade_no", "");
            string trade_no = GetValue("trade_no", "");
            string out_request_no = GetValue("out_request_no", "");  //本次退款请求流水号，部分退款时必传
            string refund_amount = GetValue("refund_amount", "");      //本次退款金额
            IAopClient client = new DefaultAopClient(AlipayConfig.GATEWAY, AlipayConfig.APP_ID, AlipayConfig.APP_PRIVATE_KEY, "json", "1.0", AlipayConfig.SIGN_TYPE, AlipayConfig.ALIPAY_PUBLIC_KEY, AlipayConfig.Input_charset, false);
            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();//创建API对应的request类

            AlipayTradeRefundModel model = new AlipayTradeRefundModel();
            model.OutTradeNo = out_trade_no;
            model.TradeNo = trade_no;
            model.OutRequestNo = out_request_no;
            model.RefundAmount = refund_amount;
            request.SetBizModel(model);

            AlipayTradeRefundResponse response = client.Execute(request);//通过alipayClient调用API，获得对应的response类
            //return HttpUtility.HtmlEncode(response.Body);
            string jsonstr = FastJSON.JSON.Beautify(FastJSON.JSON.ToJSON(response));
            log4net.LogManager.GetLogger("AliRefund").Info(jsonstr);
            if (!response.IsError)
            {
                //修改退款状态
                //UpdateRefundState(out_trade_no, 9);

                return this.Json(new
                {
                    code = 0,
                    info = "",
                    result = new
                    {
                        trade_no = response.TradeNo ?? "",
                        out_trade_no = response.OutTradeNo ?? "",
                        refund_fee = response.RefundFee ?? "0.00",
                        body = response.Body ?? ""
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = response.SubMsg,
                    result = new { }
                }, JsonRequestBehavior.AllowGet);
            }

            //refund_fee	该笔交易已退款的总金额
        }

        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组 
        /// request回来的信息组成的数组
        public Dictionary<string, string> GetRequestPost()
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }

        #endregion

        #region 微信


        private static TenPayV3Info _tenPayV3Info;

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }

        public static TenPayV3Info TenPayV3Info
        {
            get
            {
                if (_tenPayV3Info == null)
                {
                    _tenPayV3Info =
                        TenPayV3InfoCollection.Data[System.Configuration.ConfigurationManager.AppSettings["TenPayV3_MchId"]];
                }
                return _tenPayV3Info;
            }
        }

        public JsonResult WxPay()
        {
            string title = "";
            string content = "";
            string orderid = GetValue("orderid", "");
            OrderInfo or = GetOrderInfo(orderid);
            if (or != null)
            {
                if (or.o_type == 1)
                {
                    title = "医院陪护费用";
                    content = "医院陪护费用";
                }
                else if (or.o_type == 1)
                {
                    title = "家庭陪护费用";
                    content = "家庭陪护费用";
                }
                else
                {
                    title = "陪诊费用";
                    content = "陪诊费用";
                }
                string out_trade_no = or.orderno;
                string timeStamp = "";
                string nonceStr = "";
                //当前时间 yyyyMMdd
                string date = DateTime.Now.ToString("yyyyMMdd");
                if ("" == out_trade_no)
                {
                    //生成订单10位序列号，此处用时间和随机数生成，商户根据自己调整，保证唯一
                    out_trade_no = DateTime.Now.ToString("HHmmss") + TenPayV3Util.BuildRandomStr(28);
                }

                //创建支付应答对象
                RequestHandler packageReqHandler = new RequestHandler(null);
                //初始化
                packageReqHandler.Init();

                timeStamp = TenPayV3Util.GetTimestamp();
                nonceStr = TenPayV3Util.GetNoncestr();

                //设置package订单参数
                packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		           //公众账号ID
                packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		           //商户号
                packageReqHandler.SetParameter("nonce_str", nonceStr);                         //随机字符串
                packageReqHandler.SetParameter("attach", title);                               //附加数据
                packageReqHandler.SetParameter("body", content);                               //商品信息
                packageReqHandler.SetParameter("out_trade_no", out_trade_no);		               //商家订单号
                packageReqHandler.SetParameter("total_fee", int.Parse((or.cost * 100).ToString()).ToString());		   //商品金额,以分为单位(money * 100).ToString()
                packageReqHandler.SetParameter("spbill_create_ip", Request.UserHostAddress);   //用户的公网ip，不是商户服务器IP
                packageReqHandler.SetParameter("notify_url", TenPayV3Info.TenPayV3Notify);	   //接收财付通通知的URL
                packageReqHandler.SetParameter("trade_type", TenPayV3Type.APP.ToString());	   //交易类型

                string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
                packageReqHandler.SetParameter("sign", sign);	                    //签名

                string data = packageReqHandler.ParseXML();

                var result = TenPayV3.Unifiedorder(data);
                var res = XDocument.Parse(result);

                var xml = res.Element("xml");
                string return_code = res.Element("xml").Element("return_code").Value;
                string info = res.Element("xml").Element("return_msg").Value;
                string result_code = res.Element("xml").Element("result_code") == null ? "" : res.Element("xml").Element("result_code").Value;
                if (return_code.ToUpper() == "SUCCESS" && result_code.ToUpper() == "SUCCESS")
                {
                    //修改支付方式
                    IList<TextAndValue> tv = new List<TextAndValue>();
                    tv.Add(new TextAndValue("@orderid", GetValue("orderid")));
                    tv.Add(new TextAndValue("@paytype", "1"));  //1：微信，2： 支付宝
                    KinProxyData.Execute(tv, dbname + ".dbo.app_updatepaytype");

                    string prepayId = res.Element("xml").Element("prepay_id").Value;
                    //return this.Json(new
                    //{
                    //    code = 0,
                    //    info = info,
                    //    result = new
                    //    {
                    //        appid = TenPayV3Info.AppId,
                    //        mch_id = TenPayV3Info.MchId,
                    //        prepay_id = prepayId,
                    //        trade_type = TenPayV3Type.APP.ToString(),
                    //        key = TenPayV3Info.Key
                    //    }
                    //}, JsonRequestBehavior.AllowGet);

                    //设置支付参数
                    RequestHandler paySignReqHandler = new RequestHandler(null);
                    paySignReqHandler.SetParameter("appid", TenPayV3Info.AppId);		           //公众账号ID
                    paySignReqHandler.SetParameter("partnerid", TenPayV3Info.MchId);
                    paySignReqHandler.SetParameter("prepayid", prepayId);
                    paySignReqHandler.SetParameter("noncestr", nonceStr);
                    paySignReqHandler.SetParameter("timestamp", timeStamp);
                    paySignReqHandler.SetParameter("package", "Sign=WXPay");
                    string paySign = paySignReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);

                    return this.Json(new
                    {
                        code = 0,
                        info = info,
                        result = new
                        {
                            appid = TenPayV3Info.AppId,
                            partnerid = TenPayV3Info.MchId,
                            prepayid = prepayId,
                            noncestr = nonceStr,
                            timestamp = timeStamp,
                            packageValue = "Sign=WXPay",
                            trade_type = TenPayV3Type.APP.ToString(),
                            sign = paySign,
                            key = TenPayV3Info.Key
                        }
                    }, JsonRequestBehavior.AllowGet);

                    //<xml>
                    //   <return_code><![CDATA[SUCCESS]]></return_code>
                    //   <return_msg><![CDATA[OK]]></return_msg>
                    //   <appid><![CDATA[wx2421b1c4370ec43b]]></appid>
                    //   <mch_id><![CDATA[10000100]]></mch_id>
                    //   <nonce_str><![CDATA[IITRi8Iabbblz1Jc]]></nonce_str>
                    //   <sign><![CDATA[7921E432F65EB8ED0CE9755F0E86D72F]]></sign>
                    //   <result_code><![CDATA[SUCCESS]]></result_code>
                    //   <prepay_id><![CDATA[wx201411101639507cbf6ffd8b0779950874]]></prepay_id>
                    //   <trade_type><![CDATA[APP]]></trade_type>
                    //</xml> 
                }
                else
                {
                    return this.Json(new
                    {
                        code = -1,
                        info = info,
                        result = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = "获取订单信息失败",
                    result = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult WxNotify()
        {
            ResponseHandler resHandler = new ResponseHandler(null);

            string return_code = resHandler.GetParameter("return_code");
            string return_msg = resHandler.GetParameter("return_msg");

            string res = null;

            resHandler.SetKey(TenPayV3Info.Key);
            //验证请求是否从微信发过来（安全）
            if (resHandler.IsTenpaySign())
            {
                string result_code = resHandler.GetParameter("return_msg");
                if (return_code.ToString() == "SUCCESS " && result_code == "SUCCESS ")
                {
                    res = "success";

                    //正确的订单处理
                    string out_trade_no = resHandler.GetParameter("out_trade_no");
                    string trade_no = resHandler.GetParameter("transaction_id");
                    log4net.LogManager.GetLogger("WxNotify").Info("out_trade_no:" + out_trade_no);
                    PaySuccess(out_trade_no, trade_no);
                }

            }
            else
            {
                res = "wrong";

                //错误的订单处理
            }

            return Content(res);
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <returns></returns>
        public JsonResult WxQuery()
        {
            string out_trade_no = GetValue("out_trade_no", "");
            string trade_no = GetValue("trade_no", "");

            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("transaction_id", trade_no);       //填入微信订单号 
            packageReqHandler.SetParameter("out_trade_no", out_trade_no);         //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);             //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            //var result = TenPayV3.OrderQuery(data);
            //var res = XDocument.Parse(result);
            //string openid = res.Element("xml").Element("sign").Value;


            var response = TenPayV3.OrderQuery(data);

            int code = 0;
            WxPayData result = new WxPayData();
            result.FromXml(response);
            var return_code = result.GetValue("return_code");
            var info = result.GetValue("return_msg");
            var result_code = result.GetValue("result_code");
            if (return_code.ToString() == "SUCCESS " && result.IsSet("result_code") && result_code.ToString() == "SUCCESS ")
            {
                string trade_state = result.IsSet("trade_state") ? result.GetValue("trade_state").ToString() : "UNDIFINED";

                /*SUCCESS—支付成功 
                REFUND—转入退款 
                NOTPAY—未支付 
                CLOSED—已关闭 
                REVOKED—已撤销（刷卡支付） 
                USERPAYING--用户支付中 
                PAYERROR--支付失败(其他原因，如银行返回失败)*/

                if (result.IsSet("err_code_des"))
                {
                    code = -1;
                    info = result.GetValue("err_code_des").ToString();
                }
                return this.Json(new
                {
                    code = code,
                    info = info,
                    result = new {
                        trade_state = trade_state
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = info,
                    result = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 关闭订单接口
        /// </summary>
        /// <returns></returns>
        public ActionResult WxCloseOrder()
        {
            string out_trade_no = GetValue("out_trade_no", "");

            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", out_trade_no);                 //填入商家订单号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();

            //var result = TenPayV3.CloseOrder(data);
            //var res = XDocument.Parse(result);
            //string openid = res.Element("xml").Element("openid").Value;
            //return Content(openid);


            var response = TenPayV3.CloseOrder(data);

            int code = 0;
            WxPayData result = new WxPayData();
            result.FromXml(response);
            var return_code = result.GetValue("return_code");
            var info = result.GetValue("return_msg");
            var result_code = result.GetValue("result_code");
            if (return_code.ToString() == "SUCCESS " && result.IsSet("result_code") && result_code.ToString() == "SUCCESS ")
            {
                if (result.IsSet("err_code_des"))
                {
                    code = -1;
                    info = result.GetValue("err_code_des").ToString();
                }
                return this.Json(new
                {
                    code = code,
                    info = info,
                    result = new {}
                }, JsonRequestBehavior.AllowGet); 
                
                //return this.Json(new
                //{
                //    code = 0,
                //    info = "查询关闭成功",
                //    result = new
                //    {
                //        return_code = return_code,
                //        return_msg = return_msg,
                //        result_code = result_code,
                //        err_code = err_code,
                //        err_code_des = err_code_des
                //    }
                //}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = info,
                    result = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 退款申请接口
        /// </summary>
        /// <returns></returns>
        public ActionResult WxRefund()
        {
            string out_trade_no = GetValue("out_trade_no", "");
            string trade_no = GetValue("trade_no", "");
            string out_request_no = GetValue("out_request_no", "");  //本次退款请求流水号，部分退款时必传
            string total_fee = GetValue("total_fee", "0");
            string refund_amount = GetValue("refund_amount", "0");      //本次退款金额

            string nonceStr = TenPayV3Util.GetNoncestr();
            RequestHandler packageReqHandler = new RequestHandler(null);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", TenPayV3Info.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", TenPayV3Info.MchId);		  //商户号
            packageReqHandler.SetParameter("out_trade_no", out_trade_no);                 //填入商家订单号
            packageReqHandler.SetParameter("out_refund_no", out_request_no);                //填入退款订单号
            packageReqHandler.SetParameter("total_fee", total_fee);               //填入总金额
            packageReqHandler.SetParameter("refund_fee", refund_amount);               //填入退款金额
            packageReqHandler.SetParameter("op_user_id", TenPayV3Info.MchId);   //操作员Id，默认就是商户号
            packageReqHandler.SetParameter("nonce_str", nonceStr);              //随机字符串
            string sign = packageReqHandler.CreateMd5Sign("key", TenPayV3Info.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名
            //退款需要post的数据
            string data = packageReqHandler.ParseXML();

            //退款接口地址
            string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";

            //=======【证书路径设置】===================================== 
            /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）*/
            //本地或者服务器的证书位置（证书在微信支付申请成功发来的通知邮件中）
            //string cert = @"F:\apiclient_cert.p12";
            string cert = Path.Combine(Request.PhysicalApplicationPath, "cert/apiclient_cert.p12");
            //私钥（在安装证书时设置）
            string password = SSLCERT_PASSWORD;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //调用证书
            X509Certificate2 cer = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            #region 发起post请求
            HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webrequest.ClientCertificates.Add(cer);
            webrequest.Method = "post";

            byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
            webrequest.ContentLength = postdatabyte.Length;
            Stream stream;
            stream = webrequest.GetRequestStream();
            stream.Write(postdatabyte, 0, postdatabyte.Length);
            stream.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)webrequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            #endregion


            //var res = XDocument.Parse(responseContent);
            //string openid = res.Element("xml").Element("out_refund_no").Value;
            //return Content(openid);

            int code = 0;
            WxPayData result = new WxPayData();
            result.FromXml(responseContent);
            var return_code = result.GetValue("return_code");
            var info = result.GetValue("return_msg");
            var result_code = result.GetValue("result_code");
            if (return_code.ToString() == "SUCCESS " && result.IsSet("result_code") && result_code.ToString() == "SUCCESS ")
            {
                //商户退款单号 out_refund_no 
                string out_refund_no = result.IsSet("out_refund_no") ? result.GetValue("out_refund_no").ToString() : "";

                /*SYSTEMERROR 接口返回错误 系统超时 请不要更换商户退款单号，请使用相同参数再次调用API。 
                USER_ACCOUNT_ABNORMAL 退款请求失败 用户帐号注销 此状态代表退款申请失败，商户可自行处理退款。 
                NOTENOUGH 余额不足 商户可用退款余额不足 此状态代表退款申请失败，商户可根据具体的错误提示做相应的处理。 
                INVALID_TRANSACTIONID 无效transaction_id 请求参数未按指引进行填写 请求参数错误，检查原交易号是否存在或发起支付交易接口返回失败 
                PARAM_ERROR 参数错误 请求参数未按指引进行填写 请求参数错误，请重新检查再调用退款申请 
                APPID_NOT_EXIST APPID不存在 参数中缺少APPID 请检查APPID是否正确 
                MCHID_NOT_EXIST MCHID不存在 参数中缺少MCHID 请检查MCHID是否正确 
                APPID_MCHID_NOT_MATCH appid和mch_id不匹配 appid和mch_id不匹配 请确认appid和mch_id是否匹配 
                REQUIRE_POST_METHOD 请使用post方法 未使用post传递参数  请检查请求参数是否通过post方法提交 
                SIGNERROR 签名错误 参数签名结果不正确 请检查签名参数和方法是否都符合签名算法要求 
                XML_FORMAT_ERROR XML格式错误 XML格式错误 请检查XML参数格式是否正确 
                */

                if (result.IsSet("err_code_des"))
                {
                    return this.Json(new
                    {
                        code = -1,
                        info = result.GetValue("err_code_des").ToString(),
                        result = new
                        {
                            out_refund_no = out_refund_no
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //修改退款状态
                    //UpdateRefundState(out_trade_no, 9);

                    return this.Json(new
                    {
                        code = code,
                        info = info,
                        result = new
                        {
                            out_refund_no = out_refund_no
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return this.Json(new
                {
                    code = -1,
                    info = info,
                    result = new { }
                }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion

    }
}
