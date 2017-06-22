using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.zgyey.sgspay.model;

namespace WxPayAPI
{
    /// <summary>
    /// 支付结果通知回调处理类
    /// 负责接收微信支付后台发送的支付结果并对订单有效性进行验证，将验证结果反馈给微信支付后台
    /// </summary>
    public class ResultNotify:Notify
    {
        public ResultNotify(Page page):base(page)
        {
        }

        public override void ProcessNotify()
        {
            WxPayData notifyData = GetNotifyData();

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                log4net.LogManager.GetLogger("sgspay").Error("WX Pay result is error :" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());
                log4net.LogManager.GetLogger("sgspay").Error("WX Pay Order query failure :" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            //查询订单成功
            else
            {
                //更新订单状态，开通vip
                //string uid = notifyData.GetValue("openid").ToString(); 
                string out_trade_no = notifyData.GetValue("out_trade_no").ToString();
                UpdateOrderStatus("0", out_trade_no);

                WxPayData res = new WxPayData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                Log.Info(this.GetType().ToString(), "order query success : " + res.ToXml());
                log4net.LogManager.GetLogger("sgspay").Error("WX Pay Order query success :" + res.ToXml());
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
        }

        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateOrderStatus(string uid, string orderno)
        {

            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@uid", uid));
            tv.Add(new TextAndValue("@orderno", orderno));
            tv.Add(new TextAndValue("@status", "1"));
            com.zgyey.sgspay.core.DataProxy.SGS_PAYProxyData.Execute(tv, "payapp..sgs_UpdateOrderStatus");
        }
    }
}