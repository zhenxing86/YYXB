using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    /// <summary>
    /// 类名：AlipayConfig
    /// 功能：基础配置类
    /// 详细：设置帐户有关信息及返回路径
    /// 版本：3.3
    /// 日期：2012-07-05
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
    /// 
    /// 如何获取安全校验码和合作身份者ID
    /// 1.用您的签约支付宝账号登录支付宝网站(www.alipay.com)
    /// 2.点击“商家服务”(https://b.alipay.com/order/myOrder.htm)
    /// 3.点击“查询合作者身份(PID)”、“查询安全校验码(Key)”
    /// </summary>
    public class AlipayConfig
    {
        #region 字段
        private static string appid = "";
        private static string app_private_key = "";
        private static string alipay_public_key = "";
        private static string input_charset = "";
        private static string sign_type = "";
        private static string gateway = "";
        private static string alipay_notify_url = "";
        #endregion

        static AlipayConfig()
        {
            //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            //APPID即创建应用后生成
            appid = System.Configuration.ConfigurationManager.AppSettings["APP_ID"];

            //开发者应用私钥，由开发者自己生成
            app_private_key = System.Configuration.ConfigurationManager.AppSettings["APP_PRIVATE_KEY"]; 

            //支付宝公钥，由支付宝生成
            alipay_public_key = System.Configuration.ConfigurationManager.AppSettings["ALIPAY_PUBLIC_KEY"]; 

            //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

            //字符编码格式 目前支持 gbk 或 utf-8
            input_charset = "utf-8";

            //签名方式，选择项：RSA、DSA、MD5、RSA2
            sign_type = System.Configuration.ConfigurationManager.AppSettings["SIGN_TYPE"]; // "RSA2";

            alipay_notify_url = System.Configuration.ConfigurationManager.AppSettings["ALIPAY_Notify_Url"];

            gateway = System.Configuration.ConfigurationManager.AppSettings["GATEWAY"]; 
        }

        #region 属性
        /// <summary>
        /// APPID即创建应用后生成
        /// </summary>
        public static string APP_ID
        {
            get { return appid; }
            set { appid = value; }
        }

        /// <summary>
        /// 开发者应用私钥，由开发者自己生成
        /// </summary>
        public static string APP_PRIVATE_KEY
        {
            get { return app_private_key; }
            set { app_private_key = value; }
        }

        /// <summary>
        /// 支付宝公钥，由支付宝生成
        /// </summary>
        public static string ALIPAY_PUBLIC_KEY
        {
            get { return alipay_public_key; }
            set { alipay_public_key = value; }
        }

        /// <summary>
        /// 获取字符编码格式
        /// </summary>
        public static string Input_charset
        {
            get { return input_charset; }
        }

        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        public static string SIGN_TYPE
        {
            get { return sign_type; }
        }
        #endregion

        /// <summary>
        /// 支付宝网关
        /// </summary>
        public static string GATEWAY { get { return gateway; } }

        public static string ALIPAY_Notify_Url
        {
            get { return alipay_notify_url; }
            set { alipay_notify_url = value; }
        }
    }
}
