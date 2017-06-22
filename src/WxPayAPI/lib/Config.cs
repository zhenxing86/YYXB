using System;
using System.Collections.Generic;
using System.Web;

namespace WxPayAPI
{
    /**
    * 	配置账号信息
    */
    public class WxPayConfig
    {
        //=======【基本信息设置】=====================================
        /* 微信公众号信息配置
        * APPID：绑定支付的APPID（必须配置）
        * MCHID：商户号（必须配置）
        * KEY：商户支付密钥，参考开户邮件设置（必须配置）
        * APPSECRET：公众帐号secert（仅JSAPI支付的时候需要配置）
        */
        //public const string APPID = "wx2428e34e0e7dc6ef";
        //public const string MCHID = "1233410002";
        //public const string KEY = "e10adc3849ba56abbe56e056f20f883e";
        //public const string APPSECRET = "51c56b886b5be869567dd389b3e5d1d6";

        //中幼
        //public const string APPID = "wxdcc245ed8aee4963";
        //public const string MCHID = "1243905402";
        //public const string KEY = "Z0G4YE61Y006013al2o2n2g20FuD8L1F";
        //public const string APPSECRET = "b41396af7b8104a78eec05eda1f026d1";

        ////爱童书  程东
        //public const string APPID = "wx2743c9580cccf9db";
        //public const string MCHID = "1308178701";
        //public const string KEY = "09ecaf53bbe945cdf5fd3ef7e695dfa5";
        //public const string APPSECRET = "d4624c36b6795d1d99dcf0547af5443d";

        //林昊殷
        public const string APPID = "wx9fb4f7dddc9ce7bc";
        public const string MCHID = "1311030201";
        public const string KEY = "hpwpybzbzrlcfu7ipvlq83ijh7x5432e";
        public const string APPSECRET = "ad3266cbd473d69dbf598680d9ee3c77";

        //=======【证书路径设置】===================================== 
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
        */
        public const string SSLCERT_PATH = "cert/apiclient_cert.p12";
        public const string SSLCERT_PASSWORD = "1233410002";



        //=======【支付结果通知url】===================================== 
        /* 支付结果通知回调url，用于商户接收支付结果
        */
        //public const string NOTIFY_URL = "http://paysdk.weixin.qq.com/example/ResultNotifyPage.aspx";
        public const string NOTIFY_URL = "";  //在配置文件里配置



        //=======【商户系统后台机器IP】===================================== 
        /* 此参数可手动配置也可在程序中自动获取
        */
        public const string IP = "8.8.8.8";


        //=======【代理服务器设置】===================================
        /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
        */
        //public const string PROXY_URL = "http://10.152.18.220:8080";
        public const string PROXY_URL = "0.0.0.0:0";

        //=======【上报信息配置】===================================
        /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
        */
        public const int REPORT_LEVENL = 2;

        //=======【日志级别】===================================
        /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
        */
        public const int LOG_LEVENL = 2;


    }

}