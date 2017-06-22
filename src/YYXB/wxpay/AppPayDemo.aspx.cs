using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using com.zgyey.sgspay.model;
using System.Data;
using System.Linq;
using System.IO;
using System.Net;

namespace WxPayAPI
{
    public partial class AppPayDemo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                #region test
                //string attach = "APP支付测试";
                //string body = "APP支付测试";
                //string mch_id = WxPayConfig.MCHID;
                //string nonce_str = "1add1a30ac87aa2db72f57a2375d8fec";
                //string notify_url = "http://www.baidu.com/";
                //string openid = GetReqValue("openid");
                //string out_trade_no = "1415659990";
                //string spbill_create_ip = "14.23.150.211";
                //string total_fee = "1";
                //string trade_type = "APP"; 

                //WxPayData inputObj = new WxPayData();
                //inputObj.SetValue("appid", WxPayConfig.APPID);//公众账号ID
                //inputObj.SetValue("attach", attach);
                //inputObj.SetValue("body", body);
                //inputObj.SetValue("mch_id", mch_id);
                //inputObj.SetValue("nonce_str", nonce_str);
                //inputObj.SetValue("notify_url", notify_url);
                //inputObj.SetValue("openid", openid);
                //inputObj.SetValue("out_trade_no", out_trade_no);
                //inputObj.SetValue("spbill_create_ip", spbill_create_ip);
                //inputObj.SetValue("total_fee", total_fee);
                //inputObj.SetValue("trade_type", trade_type);

                ////签名
                //inputObj.SetValue("sign", inputObj.MakeSign());
                //string xml = inputObj.ToXml();
                //string tourl = inputObj.ToUrl();

                //string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
                //Log.Debug("WxPayApi", "UnfiedOrder request : " + xml);
                //string response = HttpService.Post(xml, url,false, 6);
                //Log.Debug("WxPayApi", "UnfiedOrder response : " + response);

                //WxPayData resultData = new WxPayData();
                //resultData.FromXml(response);

                #endregion

                WxPayData resultData = new WxPayData();
                string openid = GetReqValue("openid");
                string product_id = GetReqValue("product_id");
                string out_trade_no = GetReqValue("out_trade_no");
                string sign = GetReqValue("sign");
                 
                if (openid == "" || product_id == "" || out_trade_no == "")
                {
                    //检查openid和product_id是否返回  uid是否返回
                    resultData = new WxPayData();
                    resultData.SetValue("return_code", "FAIL");
                    resultData.SetValue("return_msg", "APP上传参数格式不正确");
                    Log.Info(this.GetType().ToString(), "The data WeChat post is error : " + resultData.ToXml());
                }
                else
                {
                    resultData = ProcessOrder(openid, product_id, out_trade_no);
                }
                Response.Write("<span style='color:#00CD00;font-size:20px'>" + resultData.ToPrintStr() + "</span>");
            }
            catch (WxPayException ex)
            {
                Response.Write("<span style='color:#FF0000;font-size:20px'>" + ex.ToString() + "</span>");
            }
            catch (Exception ex)
            {
                Response.Write("<span style='color:#FF0000;font-size:20px'>" + ex.ToString() + "</span>");
            }
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

        public WxPayData ProcessOrder(string openid, string product_id, string out_trade_no)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@uid", openid));
            tv.Add(new TextAndValue("@feeid", product_id));
            tv.Add(new TextAndValue("@paytype", "2"));//1：支付宝，2：微信支付
            tv.Add(new TextAndValue("@orderno", out_trade_no));

            BaseModel<DataSet> bs = com.zgyey.sgspay.core.DataProxy.SGS_PAYProxyData.GetList(tv, "fmcapp..jzxx_CreateOrder", "tb1", "tb2");
            DataSet ds = bs.result;
            AlipayOrder fee = new AlipayOrder();
            if (int.Parse(ds.Tables[0].Rows[0][0].ToString()) == 0)
            {
                fee = (
                    from x in ds.Tables[1].AsEnumerable()
                    select new AlipayOrder()
                    {
                        orderNo = x["order_no"],
                        feeName = x["fee_name"],
                        fee_des = x["fee_des"],
                        amount = x["fee_money"]
                    }
                   ).ToList()[0];
            }

            //调统一下单接口，获得下单结果
            WxPayData unifiedOrderResult = new WxPayData();
            try
            {
                unifiedOrderResult = UnifiedOrder(fee);
            }
            catch (Exception ex)//若在调统一下单接口时抛异常，立即返回结果给微信支付后台
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "统一下单失败");
                Log.Error(this.GetType().ToString(), "UnifiedOrder failure : " + res.ToXml());
                return res;
            }

            //若下单失败，则立即返回结果给微信支付后台
            if (!unifiedOrderResult.IsSet("appid") || !unifiedOrderResult.IsSet("mch_id") || !unifiedOrderResult.IsSet("prepay_id"))
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "统一下单失败");
                Log.Error(this.GetType().ToString(), "UnifiedOrder failure : " + res.ToXml());
                return res;
            }

            //统一下单成功,则返回成功结果给微信支付后台
            WxPayData data = new WxPayData();
            data.SetValue("return_code", "SUCCESS");
            data.SetValue("return_msg", "OK");
            data.SetValue("appid", WxPayConfig.APPID);
            data.SetValue("mch_id", WxPayConfig.MCHID);
            data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());
            data.SetValue("prepay_id", unifiedOrderResult.GetValue("prepay_id"));
            data.SetValue("result_code", "SUCCESS");
            data.SetValue("err_code_des", "OK");
            data.SetValue("sign", data.MakeSign());

            Log.Info(this.GetType().ToString(), "UnifiedOrder success , send data to WeChat : " + data.ToXml());
            return data;
        }

        private WxPayData UnifiedOrder(AlipayOrder fee)
        {
            //统一下单 
            WxPayData req = new WxPayData();
            if (fee != null && fee.orderNo != null && fee.orderNo.ToString() != "")
            {
                int total_fee = 0;
                if(int.TryParse(fee.amount.ToString(), out total_fee))
                {
                    req.SetValue("body", fee.feeName);
                    req.SetValue("attach", fee.fee_des);
                    req.SetValue("out_trade_no", fee.orderNo);
                    req.SetValue("total_fee", total_fee);
                }
            }
            req.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            req.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            req.SetValue("goods_tag", "jzxx");
            req.SetValue("trade_type", "APP");

            WxPayData result = WxPayApi.UnifiedOrder(req);
            return result;
        }
    }
}