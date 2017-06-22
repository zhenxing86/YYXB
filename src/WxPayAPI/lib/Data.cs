using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using System.Data;

namespace WxPayAPI
{
    /// <summary>
    /// 微信支付协议接口数据类，所有的API接口通信都依赖这个数据结构，
    /// 在调用接口之前先填充各个字段的值，然后进行接口通信，
    /// 这样设计的好处是可扩展性强，用户可随意对协议进行更改而不用重新设计数据结构，
    /// 还可以随意组合出不同的协议数据包，不用为每个协议设计一个数据包结构
    /// </summary>
    public class WxPayData
    {
        public WxPayData()
        {

        }

        //采用排序的Dictionary的好处是方便对数据包进行签名，不用再签名之前再做一次排序
        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();

        /**
        * 设置某个字段的值
        * @param key 字段名
         * @param value 字段值
        */
        public void SetValue(string key, object value)
        {
            m_values[key] = value;
        }

        /**
        * 根据字段名获取某个字段的值
        * @param key 字段名
         * @return key对应的字段值
        */
        public object GetValue(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o;
        }

        /**
         * 判断某个字段是否已设置
         * @param key 字段名
         * @return 若字段key已被设置，则返回true，否则返回false
         */
        public bool IsSet(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            if (null != o)
                return true;
            else
                return false;
        }

        /**
        * @将Dictionary转成xml
        * @return 经转换得到的xml串
        * @throws WxPayException
        **/
        public string ToXml()
        {
            //数据为空时不能转化为xml格式
            if (0 == m_values.Count)
            {
                Log.Error(this.GetType().ToString(), "WxPayData数据为空!");
                throw new WxPayException("WxPayData数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    Log.Error(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new WxPayException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    Log.Error(this.GetType().ToString(), "WxPayData字段数据类型错误!");
                    throw new WxPayException("WxPayData字段数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }

        /**
       * @将Dictionary转成xml
       * @return 经转换得到的xml串
       * @throws WxPayException
       **/
        public string ToXmlString()
        {
            //数据为空时不能转化为xml格式
            if (0 == m_values.Count)
            {
                Log.Error(this.GetType().ToString(), "WxPayData数据为空!");
                throw new WxPayException("WxPayData数据为空!");
            }

            string xml = "&lt;xml&gt;</br>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    Log.Error(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new WxPayException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "&lt;" + pair.Key + "&gt;" + pair.Value + "&lt;/" + pair.Key + "&gt;</br>";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "&lt;" + pair.Key + "&gt;" + "&lt;![CDATA[" + pair.Value + "]]>&lt;/" + pair.Key + "&gt;</br>";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    Log.Error(this.GetType().ToString(), "WxPayData字段数据类型错误!");
                    throw new WxPayException("WxPayData字段数据类型错误!");
                }
            }
            xml += "&lt;/xml&gt;";
            return xml;
        }


        /**
        * @将xml转为WxPayData对象并返回对象内部的数据
        * @param string 待转换的xml串
        * @return 经转换得到的Dictionary
        * @throws WxPayException
        */
        public SortedDictionary<string, object> FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                Log.Error(this.GetType().ToString(), "将空的xml串转换为WxPayData不合法!");
                throw new WxPayException("将空的xml串转换为WxPayData不合法!");
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }
            try
            {
                CheckSign();//验证签名,不通过会抛异常
            }
            catch(WxPayException ex)
            {
                throw new WxPayException(ex.Message);
            }

            return m_values;
        }

        /**
        * @Dictionary格式转化成url参数格式
        * @ return url格式串, 该串不包含sign字段值
        */
        public string ToUrl()
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    Log.Error(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new WxPayException("WxPayData内部含有值为null的字段!");
                }

                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }


        /**
        * @Dictionary格式化成Json
         * @return json串数据
        */
        public string ToJson()
        {
            string jsonStr = JsonMapper.ToJson(m_values);
            return jsonStr;
        }

        /**
        * @values格式化成能在Web页面上显示的结果（因为web页面上不能直接输出xml格式的字符串）
        */
        public string ToPrintStr()
        {
            string str = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    Log.Error(this.GetType().ToString(), "WxPayData内部含有值为null的字段!");
                    throw new WxPayException("WxPayData内部含有值为null的字段!");
                }

                str += string.Format("{0}={1}<br>", pair.Key, pair.Value.ToString());
            }
            Log.Debug(this.GetType().ToString(), "Print in Web Page : " + str);
            return str;
        }

        /**
        * @生成签名，详见签名生成算法
        * @return 签名, sign字段不参加签名
        */
        public string MakeSign()
        {
            //转url格式
            string str = ToUrl();
            //在string后加入API KEY
            str += "&key=" + WxPayConfig.KEY;
            //MD5加密
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToUpper();
        }

        /**
        * 
        * 检测签名是否正确
        * 正确返回true，错误抛异常
        */
        public bool CheckSign()
        {
            ////如果没有设置签名，则跳过检测
            //if (!IsSet("sign"))
            //{
            //    return true;
            //}
            //如果设置了签名但是签名为空，则抛异常
            //else 
            if(GetValue("sign") == null || GetValue("sign").ToString() == "")
            {
                Log.Error(this.GetType().ToString(), "WxPayData签名存在但不合法!");
                throw new WxPayException("WxPayData签名存在但不合法!");
            }

            //获取接收到的签名
            string return_sign = GetValue("sign").ToString();

            //在本地计算新的签名
            string cal_sign = MakeSign();

            if (cal_sign == return_sign)
            {
                return true;
            }

            Log.Error(this.GetType().ToString(), "WxPayData签名验证错误!");
            throw new WxPayException("WxPayData签名验证错误!");
        }

        /**
        * @获取Dictionary
        */
        public SortedDictionary<string, object> GetValues()
        {
            return m_values;
        }

        //public BaseModel<WxPaySuccess> ToJsonResult<WxPaySuccess>()
        //{
            
        //}

    }


    public class WxPaySuccess
    {
        //        <xml>
        //<appid><![CDATA[wxade1b7d6d4152501]]></appid>
        //<crttime><![CDATA[2015-08-10 16:17:24]]></crttime>
        //<err_code_des><![CDATA[OK]]></err_code_des>
        //<fee_des><![CDATA[家长学校VIP套餐一年]]></fee_des>
        //<fee_money>120</fee_money>
        //<fee_name><![CDATA[家长学校VIP套餐一年]]></fee_name>
        //<key><![CDATA[474692041593198103chuangmengjzxx]]></key>
        //<mch_id><![CDATA[1238968202]]></mch_id>
        //<nonce_str><![CDATA[f3a73a80977241d499ab297465d16e80]]></nonce_str>
        //<out_trade_no><![CDATA[123896820220150810161719182]]></out_trade_no>
        //<paytype><![CDATA[2]]></paytype>
        //<prepay_id><![CDATA[wx20150810161727b1dfd1f1c80427258430]]></prepay_id>
        //<result_code><![CDATA[SUCCESS]]></result_code>
        //<return_code><![CDATA[SUCCESS]]></return_code>
        //<return_msg><![CDATA[OK]]></return_msg>
        //<sign><![CDATA[9665092E4083B026F19ADCDDCB98F82F]]></sign>
        //</xml>
        private string _appid;

        public string appid
        {
            get { return _appid; }
            set { _appid = value; }
        }

        private object _crttime;

        public object crttime
        {
            get { return _crttime; }
            set { _crttime = value; }
        }

        private string _err_code_des;

        public string err_code_des
        {
            get { return _err_code_des; }
            set { _err_code_des = value; }
        }

        private string _fee_des;

        public string fee_des
        {
            get { return _fee_des; }
            set { _fee_des = value; }
        }

        private object _fee_money;

        public object fee_money
        {
            get { return _fee_money; }
            set { _fee_money = value; }
        }

        private string _fee_name;

        public string fee_name
        {
            get { return _fee_name; }
            set { _fee_name = value; }
        }

        private string _key;

        public string key
        {
            get { return _key; }
            set { _key = value; }
        }

        private string _mch_id;

        public string mch_id
        {
            get { return _mch_id; }
            set { _mch_id = value; }
        }

        private string _nonce_str;

        public string nonce_str
        {
            get { return _nonce_str; }
            set { _nonce_str = value; }
        }

        private string _out_trade_no;

        public string out_trade_no
        {
            get { return _out_trade_no; }
            set { _out_trade_no = value; }
        }

        private object _paytype;

        public object paytype
        {
            get { return _paytype; }
            set { _paytype = value; }
        }
        private object _prepay_id;

        public object prepay_id
        {
            get { return _prepay_id; }
            set { _prepay_id = value; }
        }

        private string _result_code;

        public string result_code
        {
            get { return _result_code; }
            set { _result_code = value; }
        }
        private string _return_msg;

        public string return_msg
        {
            get { return _return_msg; }
            set { _return_msg = value; }
        }
        private string _return_code;

        public string return_code
        {
            get { return _return_code; }
            set { _return_code = value; }
        }
        //private string _sign;

        //public string sign
        //{
        //    get { return _sign; }
        //    set { _sign = value; }
        //}
        
    }

    public class WxPayError
    {

        public string return_code { get; set; }

        public string return_msg { get; set; }
    }
}