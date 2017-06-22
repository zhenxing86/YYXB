using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using cn.jpush.api;
using cn.jpush.api.common;
using cn.jpush.api.common.resp;
using cn.jpush.api.push.mode;
using YYXB.Core.DataProxy;
using YYXB.Model;

using log4net;
using log4net.Config;

namespace ThreedForm
{
    public enum ORDER_STATE
    {
        //待付款 0
        //已付款 1
        //待服务 2
        //服务中 3
        //待评价 4 
        //已完成/已评价 5

        //已取消        6       -1
        //申请退款中    7       -2
        //退款中        8       -3
        //退款成功      9       -4
        //已拒单       10       -5
        //过期未接单   11       -6
        //退款失败     12       -7

        UNPAID = 0,
        PAID = 1,
        PENDING_SERVICE = 2,
        SERVICEING = 3,
        PENDING_EVALUATION = 4,
        FINISHED = 5,
        CANNEL = 6,
        APPLICATION_OF_REFUND = 7,
        REFUNDINT = 8,
        REFUND_SUCCESS = 9,
        REFUSED = 10,
        EXPIRE = 11,
        REFUND_FAIL = 12
    }

    public partial class Form1 : Form
    {
        protected string dbname = System.Configuration.ConfigurationManager.AppSettings["db"];
        protected string gateway = System.Configuration.ConfigurationManager.AppSettings["gateway"];
        protected string app_key = System.Configuration.ConfigurationManager.AppSettings["app_key"];
        protected string master_secret = System.Configuration.ConfigurationManager.AppSettings["master_secret"];

        int THREAD_COUNT = 1;
        Thread mainThreed = null;
        IList<Thread> threadLst = null;
        IList<Thread_State> threadStateLst = null;
        bool mainThreedRunning = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            MainThreadStart();
        }

        public void MainThreadStart()
        {
            mainThreedRunning = true;
            if (mainThreed == null)
            {
                mainThreed = new Thread(new ParameterizedThreadStart(DoWork));
                mainThreed.IsBackground = true;
                mainThreed.Start();
            }
        }

        private DateTime lastExecTime = DateTime.Now.AddHours(-2);
        private DateTime lastApplyExecTime = DateTime.Now.AddHours(-2); 
        private void DoWork(object obj)
        {
            //WriteLog("1111111111111", "Sms-Log");
            //return;

            //主要的处理流程
            MainPushSms();

            while (mainThreedRunning)
            {
                Thread.Sleep(3000);
                if (threadLst.Count(e => e.ThreadState == ThreadState.Stopped) == threadLst.Count())
                {
                    MainPushSms();
                }

                //每30秒检查一次 获取同意退款的订单，自动退款 （需要在服务里退款）
                if ((DateTime.Now - lastApplyExecTime).TotalSeconds >= 30)
                {
                    IList<tbOrder> orderLst = Get_RefundApply_OrderList();
                    Refund(orderLst,1);
                    lastApplyExecTime = DateTime.Now;
                }


                //每小时检查一次 过了预约时间，护工未接单，自动退款 （需要在服务里退款）
                if ((DateTime.Now - lastExecTime).TotalHours >= 1)
                {
                    IList<tbOrder> orderLst = Get_OverTime_OrderList();
                    Refund(orderLst,0);
                    lastExecTime = DateTime.Now;
                }



            }
            mainThreed = null;

        }

        private void Refund(IList<tbOrder> orderLst,int refund_type)
        {
            string remark = "订单过期自动退款";
            if (refund_type == 1)
            {
                remark = "系统后台同意退款";
            }
            if (orderLst == null || orderLst.Count == 0)
                return;

            foreach (tbOrder o in orderLst)
            {
                try
                {
                    //调用退款接口
                    string url = "";
                    string FormalUrl = "{0}/main/{1}?out_trade_no={2}&trade_no=&out_request_no=&refund_amount={3}";
                    if (o.paytype == 1) //支付宝
                    {
                        url = string.Format(FormalUrl, gateway, "AliRefund", o.orderno, o.cost);
                        string res = GetResponseString(url);


                        BaseModel<object> result = LitJson.JsonMapper.ToObject<BaseModel<object>>(res);
                        if (result != null && result.code == 0)
                        {
                            RefundResult(o.orderid, remark, (int)ORDER_STATE.REFUND_SUCCESS);
                        }
                        else
                        {
                            RefundResult(o.orderid, remark, (int)ORDER_STATE.REFUND_FAIL);
                        }
                        //code = 0,
                        //info = "",
                        //result = new
                        //{
                        //    trade_no = response.TradeNo ?? "",
                        //    out_trade_no = response.OutTradeNo ?? "",
                        //    refund_fee = response.RefundFee??"0.00",
                        //    body = response.Body ?? ""
                        //}
                    }
                    else //微信
                    {
                        FormalUrl = "{0}/main/{1}?out_trade_no={2}&trade_no=&out_request_no=&total_fee={3}&refund_amount={4}";
                        url = string.Format(FormalUrl, gateway, "WxRefund", o.orderno, o.cost, o.cost);
                        string res = GetResponseString(url);

                        BaseModel<object> result = LitJson.JsonMapper.ToObject<BaseModel<object>>(res);
                        if (result != null && result.code == 0)
                        {
                            RefundResult(o.orderid, remark, (int)ORDER_STATE.REFUND_SUCCESS);
                        }
                        else
                        {
                            RefundResult(o.orderid, remark, (int)ORDER_STATE.REFUND_FAIL);
                        }

                        //code = code,
                        //info = info,
                        //result = new
                        //{
                        //    out_refund_no = out_refund_no
                        //}

                    }
                }
                catch (Exception e)
                {
                    string msg = string.Format("[DoWork] orderid={0},err={1}", o.orderid, e.Message);
                    WriteLog(msg, "Order-Log");
                    service_log_add(o.orderid, "Order", msg);
                }
            }
        }



        private void RefundResult(long orderid,string remark, int state)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@orderid", orderid.ToString()));
            tv.Add(new TextAndValue("@remark", remark));
            tv.Add(new TextAndValue("@state", state.ToString()));
            
            KinProxyData.Execute(tv, dbname + ".dbo.s_RefundResult");
        }


        private void service_log_add(long key_id, string title,string content)
        {
            IList<TextAndValue> tv = new List<TextAndValue>();
            tv.Add(new TextAndValue("@key_id", key_id.ToString()));
            tv.Add(new TextAndValue("@title", title));
            tv.Add(new TextAndValue("@content", content));

            KinProxyData.Execute(tv, dbname + ".dbo.service_log_add");
        }

 
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        private IList<tbOrder> Get_OverTime_OrderList()
        {
            IList<tbOrder> lst = new List<tbOrder>();

            IList<TextAndValue> tv = new List<TextAndValue>();
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.s_overtime_order_getlist");

            lst = ds.Tables[0].AsEnumerable().Select(x => new tbOrder
            {
                //orderid,orderno,cost,paytype
                orderid = x.Field<long>("orderid"),
                orderno = x.Field<string>("orderno"),
                cost = ParseValueFloat(x["cost"].ToString(),0),
                paytype = x.Field<int>("paytype")
            }).ToList();

            return lst;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        private IList<tbOrder> Get_RefundApply_OrderList()
        {
            IList<tbOrder> lst = new List<tbOrder>();

            IList<TextAndValue> tv = new List<TextAndValue>();
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.s_refundapply_order_getlist");

            lst = ds.Tables[0].AsEnumerable().Select(x => new tbOrder
            {
                //orderid,orderno,cost,paytype
                orderid = x.Field<long>("orderid"),
                orderno = x.Field<string>("orderno"),
                cost = ParseValueFloat(x["cost"].ToString(), 0),
                paytype = x.Field<int>("paytype")
            }).ToList();

            return lst;
        }


        /// <summary>
        /// 业务处理
        /// </summary>
        private void MainPushSms()
        {
            try
            {
                threadLst = new List<Thread>();
                threadStateLst = new List<Thread_State>();
                IList<MsgBase> msgLst = GetMsg();
                IList<Msg> numList = GetNumList(msgLst, 20);
                if (numList != null && numList.Count > 0)
                {
                    int total_count = numList.Count;
                    int thread_count = THREAD_COUNT;
                    int take_count = total_count / thread_count + (total_count % thread_count > 0 ? 1 : 0);
                    for (int x = 0; x < thread_count; x++)
                    {
                        Thread_Param para = new Thread_Param();
                        para.thread_id = x;
                        para.msgLst = numList.Skip(x * take_count).Take(take_count).ToList<Msg>();

                        //处理数据
                        Thread t = new Thread(new ParameterizedThreadStart(PushSms));
                        t.IsBackground = true;
                        t.Start(para);
                        threadLst.Add(t);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("[MainPushSms] err={0}", e.Message);
                WriteLog( msg, "Sms-Log");
                service_log_add(0, "Sms", msg);
            }
        }

        public IList<Msg> GetNumList(IList<MsgBase> msgLst, int groupNum)
        {
            IList<Msg> list = new List<Msg>();
            if (msgLst == null || msgLst.Count == 0)
                return list;

            string content = msgLst[0].content;
            string title = msgLst[0].title;
            string ext = msgLst[0].ext;
            string action = msgLst[0].action == null ? "1" : msgLst[0].action.ToString();
            int num = 0;
            int num2 = 0;
            int count = msgLst.Count;
            while (num2 < count)
            {
                Msg item = new Msg();
                IList<string> users = new List<string>();
                for (int i = 0; (i < groupNum) && (num2 < count); i++)
                {
                    if ((title != msgLst[num2].title) || (content != msgLst[num2].content) || (ext != msgLst[num2].ext))
                    {
                        item.content = content;
                        item.title = title;
                        item.ext = ext;
                        item.users = users;
                        item.action = action;
                        list.Add(item);
                        ext = msgLst[num2].ext;
                        title = msgLst[num2].title;
                        content = msgLst[num2].content;
                        i = -1;
                        num = 0;
                        users = new List<string>();
                    }
                    else
                    {
                        users.Add(msgLst[num2].device_id);
                        num2++;
                        num++;
                    }
                }
                if ((((num % groupNum) == 0) || (num == (count - 1))) || (num == count))
                {
                    item.content = content;
                    item.title = title;
                    item.ext = ext;
                    item.users = users;
                    item.action = action;
                    list.Add(item);
                }
            }
            return list;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="obj"></param>
        private void PushSms(object obj)
        {
            JPushClient client = new JPushClient(app_key, master_secret);
            Thread_Param para = obj as Thread_Param;
            foreach (Msg msg in para.msgLst)
            {
                try
                {
                    PushPayload payload = PushObject_ios_audienceMore_messageWithExtras(msg);
                    client.SendPush(payload);

                    //var result = client.SendPush(payload);

                    ////由于统计数据并非非是即时的,所以等待一小段时间再执行下面的获取结果方法
                    //System.Threading.Thread.Sleep(10000);
                    ////如需查询上次推送结果执行下面的代码
                    //var apiResult = client.getReceivedApi(result.msg_id.ToString());
                    //var apiResultv3 = client.getReceivedApi_v3(result.msg_id.ToString());
                    ////如需查询某个messageid的推送结果执行下面的代码
                    //var queryResultWithV2 = client.getReceivedApi("1739302794");
                    //var querResultWithV3 = client.getReceivedApi_v3("1739302794");

                }
                catch (APIRequestException e)
                {
                    //Console.WriteLine("Error response from JPush server. Should review and fix it. ");
                    //Console.WriteLine("HTTP Status: " + e.Status);
                    //Console.WriteLine("Error Code: " + e.ErrorCode);
                    //Console.WriteLine("Error Message: " + e.ErrorMessage);

                    
                    string err = string.Format("[PushSms] Error response from JPush server. HTTP Status:{0},Error Code:{1},Error Message:{2}", e.Status, e.ErrorCode, e.Message);
                    WriteLog(err, "Sms-Log");
                    service_log_add(msg.msg_id, "Sms", err);

                }
                catch (APIConnectionException e)
                {
                    //Console.WriteLine(e.Message);
                    string err = string.Format("[PushSms] Error Message:{0}", e.Message);
                    WriteLog(err, "Sms-Log");
                    service_log_add(msg.msg_id, "Sms", err);
                }
            }

        }


        public PushPayload PushObject_ios_audienceMore_messageWithExtras(Msg msg)
        {

            var pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            //pushPayload.audience = Audience.s_tag("tag1", "tag2");
            pushPayload.audience = Audience.s_registrationId(msg.users.ToArray());
            pushPayload.message = cn.jpush.api.push.mode.Message.content(msg.content).setContentType(msg.action).setTitle(msg.title).AddExtras("from", "JPush");
            return pushPayload;

        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        private IList<MsgBase> GetMsg()
        {
            IList<MsgBase> lst = new List<MsgBase>();

            IList<TextAndValue> tv = new List<TextAndValue>();
            DataSet ds = KinProxyData.GetDataSet(tv, dbname + ".dbo.push_message_send");

            lst = ds.Tables[0].AsEnumerable().Select(x => new MsgBase
            {
                //msgid,device_id,title,content,action,ext
                msgid = x.Field<long>("msgid"),
                device_id = x.Field<string>("device_id"),
                title = x.Field<string>("title"),
                content = x.Field<string>("content"),
                action = x.Field<int>("action"),
                ext = ""//x.Field<string>("ext")
            }).ToList();

            return lst;
        }



        public void MainThreadStop()
        {
            mainThreedRunning = false;
        }

        public Form1()
        {
            InitializeComponent();
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

        public byte[] GetResponseBytes(string url)
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

        public string GetResponseString(string url)
        {
            byte[] byteresult = GetResponseBytes(url);
            return Encoding.UTF8.GetString(byteresult);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="txt">日志内容</param>
        /// <param name="prx">文件名前缀</param>
        public static void WriteLog(string txt, string prx)
        {
            LogHelper.WriteLogMax6(false, "[{0}]: {1}", prx, txt);
        }

    }
}
