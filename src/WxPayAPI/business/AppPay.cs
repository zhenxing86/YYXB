using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.zgyey.sgspay.model;
using System.Data;
using System.Linq;
using com.zgyey.sgspay.common;
namespace WxPayAPI
{
    /// <summary>
    /// 扫码支付模式一回调处理类
    /// 接收微信支付后台发送的扫码结果，调用统一下单接口并将下单结果返回给微信支付后台
    /// </summary>
    public class AppPay
    {
        public BaseModel<Object> ProcessOrder(string uid, string feeid, string from,string paytype,string key)
        {
            //调统一下单接口，获得下单结果
            WxPayData unifiedOrderResult = new WxPayData();
            BaseModel<Object> resultData = new BaseModel<Object>();
            try
            {
                AlipayOrder fee = GetFee(uid,feeid,from,paytype,key);
                unifiedOrderResult = UnifiedOrder(fee);

                //若下单失败，则立即返回结果给微信支付后台
                if (!unifiedOrderResult.IsSet("appid") || !unifiedOrderResult.IsSet("mch_id") || !unifiedOrderResult.IsSet("prepay_id"))
                {
                    WxPayError err  = new WxPayError();
                    err.return_code = "FAIL";
                    err.return_msg = "统一下单失败";
                    resultData.code = 1;
                    resultData.info = "统一下单失败";
                    resultData.result = err;

                    return resultData;
                }

                //统一下单成功,则返回成功结果给微信支付后台
                //WxPayData data = new WxPayData();
                //data.SetValue("return_code", "SUCCESS");
                //data.SetValue("return_msg", "OK");
                //data.SetValue("appid", WxPayConfig.APPID);
                //data.SetValue("mch_id", WxPayConfig.MCHID);
                //data.SetValue("key", WxPayConfig.KEY);//key
                //data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());
                //data.SetValue("prepay_id", unifiedOrderResult.GetValue("prepay_id"));
                //data.SetValue("result_code", "SUCCESS");
                //data.SetValue("err_code_des", "OK");
                //int amount = 0;
                //int.TryParse(fee.amount.ToString(), out amount);
                //data.SetValue("out_trade_no", fee.orderNo);
                //data.SetValue("fee_money", amount);
                //data.SetValue("fee_name", fee.feeName);
                //data.SetValue("fee_des", fee.fee_des);
                //data.SetValue("paytype", fee.paytype);
                //data.SetValue("crttime", fee.crttime);
                
                //data.SetValue("sign", data.MakeSign());

                int amount = 0;
                if (com.zgyey.sgspay.common.AppSetting.Contains(com.zgyey.sgspay.common.AppSetting.DemoFeeids, feeid))
                {
                    amount = 1;
                }
                else
                {
                    if (!int.TryParse(fee.amount.ToString(), out amount))
                    {
                        amount = 100;
                    }
                }


                object prepay_id  = unifiedOrderResult.GetValue("prepay_id");
                WxPaySuccess wp = new WxPaySuccess()
                {
                    return_code = "SUCCESS",
                    return_msg = "OK",
                    appid = WxPayConfig.APPID,
                    mch_id = WxPayConfig.MCHID,
                    key = WxPayConfig.KEY,//key
                    nonce_str = WxPayApi.GenerateNonceStr(),
                    prepay_id = prepay_id,
                    result_code = "SUCCESS",
                    err_code_des = "OK",
                    out_trade_no = fee.orderNo.ToString(),
                    fee_money = amount,
                    fee_name = fee.feeName.ToString(),
                    fee_des = fee.fee_des.ToString(),
                    paytype = fee.paytype,
                    crttime = fee.crttime
                };
                
                resultData.code = 0;
                resultData.info = "统一下单成功";
                resultData.result = wp;

                Log.Info(this.GetType().ToString(), "UnifiedOrder success , send data to WeChat : " + LitJson.JsonMapper.ToJson(wp));
                return resultData;
            }
            catch (Exception ex)//若在调统一下单接口时抛异常，立即返回结果给微信支付后台
            {
                WxPayError err = new WxPayError();
                err.return_code = "FAIL";
                err.return_msg = "统一下单失败";
                resultData.code = 1;
                resultData.info = "统一下单失败";
                resultData.result = err;
                Log.Error(this.GetType().ToString(), "UnifiedOrder failure : " + LitJson.JsonMapper.ToJson(err));
                return resultData;
            }

        }

        private AlipayOrder GetFee(string uid, string feeid, string from ,string paytype, string key)
        {
            AlipayOrder fee = null;
            DataMsg m = new DataMsg();
            BaseModel<DataSet> bs = null;
             //创建订单
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@uid", uid));
            tv.Add(new TextAndValue("@feeid", feeid));
            tv.Add(new TextAndValue("@from", from));
            isFrom(tv, key);
            tv.Add(new TextAndValue("@paytype", paytype));//1：支付宝，2：微信支付
            string out_trade_no = WxPayApi.GenerateOutTradeNo(from); //请与贵网站订单系统中的唯一订单号匹配
            tv.Add(new TextAndValue("@orderno", out_trade_no));
            bs = com.zgyey.sgspay.core.DataProxy.SGS_PAYProxyData.GetList(tv, "payapp..sgs_CreateOrder", "tb1", "tb2");
            
            DataSet ds = bs.result;
            m.code = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            m.info = ds.Tables[0].Rows[0][1].ToString();
            if (m.code == 0)
            {
                fee = (
                    from x in ds.Tables[1].AsEnumerable()
                    select new AlipayOrder()
                    {
                        feeid = x["fee_id"],
                        orderNo = x["order_no"],
                        feeName = x["fee_name"],
                        fee_des = x["fee_des"],
                        amount = x["fee_money"],
                        crttime = DateTime.Parse(x["crttime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss"),
                        paytype = paytype
                    }
                   ).ToList()[0];
            }

            return fee;
        }

        private WxPayData UnifiedOrder(AlipayOrder fee)
        {
            //统一下单 
            WxPayData req = new WxPayData();
            if (fee != null)
            {
                int total_fee = 1;
                int amount = 1;
                if (int.TryParse(fee.amount.ToString(), out total_fee) && total_fee > 0)
                {
                    if (com.zgyey.sgspay.common.AppSetting.Contains(com.zgyey.sgspay.common.AppSetting.DemoFeeids, fee.feeid.ToString()))
                    {
                        amount = 1;
                    }
                    else
                    {
                        amount = total_fee * 100;
                    }
                    req.SetValue("body", fee.feeName); 
                    req.SetValue("attach", fee.fee_des);
                    req.SetValue("out_trade_no", fee.orderNo);
                    req.SetValue("total_fee", amount);
                }
            }
            req.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            req.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            req.SetValue("goods_tag", "SGS");
            req.SetValue("trade_type", "APP");

            WxPayData result = WxPayApi.UnifiedOrder(req);
            return result;
        }

        protected Boolean isFrom(IList<TextAndValue> tv,string key)
        {
            Boolean bl = false;
            String timestamp = DateTime.Now.ToString("yyyy-MM-ddHH", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string pp = "";
            for (int i = 0; i < tv.Count; i++)
            {
                pp += tv[i].Value;
            }

            string newkey = pp + "zgyey_235&*9)!";
            pp += timestamp;

            string px = key;

            string newps = GetDigestStr(newkey);

            if (key == GetDigestStr(pp))
            {
                bl = true;
            }

            if (key == GetDigestStr(newkey))
            {
                bl = true;
            }

            if (System.Configuration.ConfigurationManager.AppSettings["Release"] == "0" && System.Configuration.ConfigurationManager.AppSettings["key"] == key)
            {
                bl = true;
            }

            if (!bl)
            {
                tv.Clear();
                tv.Add(new TextAndValue("error", "error"));
                tv.Add(new TextAndValue("javakey", px));
                tv.Add(new TextAndValue("netkeynewno", newkey));
                tv.Add(new TextAndValue("netkeynew", newps));

            }

            return bl;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetDigestStr(string info)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(32);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(System.Text.Encoding.GetEncoding("utf-8").GetBytes(info));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        public static string GetDigestList(IList<TextAndValue> tv)
        {
            string pp = "";
            for (int i = 0; i < tv.Count; i++)
            {
                pp += tv[i].Value;
            }
            string newkey = pp + "zgyey_235&*9)!";

            return GetDigestStr(newkey);
        }
    }
}