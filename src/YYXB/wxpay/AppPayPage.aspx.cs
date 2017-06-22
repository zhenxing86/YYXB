using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using com.zgyey.sgspay.model;

namespace WxPayAPI
{
    public partial class AppPayPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BaseModel<Object> resultData = new BaseModel<Object>();
            string uid = GetReqValue("uid");
            string feeid = GetReqValue("feeid");
            string from = GetReqValue("from");
            string key = GetReqValue("key");

            if (uid == "" || feeid == "" || from == "")
            {
                //检查openid和product_id是否返回  uid是否返回
                WxPayError err = new WxPayError();
                err.return_code = "FAIL";
                err.return_msg = "统一下单失败";
                resultData.code = 1;
                resultData.info = "统一下单失败";
                resultData.result = err;

                Log.Info(this.GetType().ToString(), "The data WeChat post is error : " + LitJson.JsonMapper.ToJson(err));
            }
            else
            {
                string paytype = "2";
                AppPay appPay = new AppPay();
                resultData = appPay.ProcessOrder(uid, feeid, from,paytype, key);
            }
            Response.ContentType = "application/json";
            Response.Write( LitJson.JsonMapper.ToJson(resultData));
        }

        protected string GetReqValue(string name)
        {
            string value = "";
            value = Request.QueryString[name] != null ? Request.QueryString[name] : "";
            if (value == "")
            {
                value = Request.Form[name] != null ? Request.Form[name] : "";
            }
            return value;
        }
    }
}