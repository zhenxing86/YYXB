using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YYXB.Model
{
    public class BaseModel<T>
    {
        public BaseModel()
        {
        }
        public BaseModel(int _code, string _info, T _result)
        {
            code = _code;
            info = _info;
            result = _result;
        }

        public int code { get; set; }
        public string info { get; set; }
        public T result { get; set; }
    }

    public class BaseModelEx<T>
    {
        public BaseModelEx()
        {
        }
        public BaseModelEx(int _code, string _info, int _nextid, T _result)
        {
            code = _code;
            info = _info;
            nextid = _nextid;
            result = _result;
        }

        public int code { get; set; }
        public string info { get; set; }
        public int nextid { get; set; }
        public T result { get; set; }
    }

    public class EmptyModel
    {
        public EmptyModel()
        {
        }
    }

    #region
    public class BaseReturn<T>
    {
        public BaseReturn()
        {
        }
        public BaseReturn(string _code, string _msg, T _data)
        {
            code = _code;
            msg = _msg;
            data = _data;
        }

        public string code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
    }

    public class BaseReturnEx<T>
    {
        public BaseReturnEx()
        {
        }
        public BaseReturnEx(string _code, string _msg, int _nextid, T _data)
        {
            code = _code;
            msg = _msg;
            data = _data;
            nextid = _nextid;
        }

        public string code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
        public int nextid { get; set; }
    }

    #endregion

}
