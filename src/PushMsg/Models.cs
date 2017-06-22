using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreedForm
{
    public class MsgBase
    {
        public long msg_id { get; set; }
        public string device_id { get; set; }
        public string ext { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public int action { get; set; }

        public long msgid { get; set; }
    }

    public class Msg
    {
        public long msg_id { get; set; }
        public string ext { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string action { get; set; }

        public IList<string> users { get; set; }
    }

    //public class User
    //{
    //    public User() 
    //    { 
    //    }
    //    public User(string device_id) { this._device_id = device_id; }

    //    private string _device_id = "";
    //    public string device_id { get; set; }
    //}

    public class Thread_State
    {
        public int thread_id { get; set; }
        public int total_count { get; set; }
        public int current_count { get; set; }
    }

    public class Thread_Param
    {
        public int thread_id { get; set; }
        public IList<Msg> msgLst { get; set; }
    }


    public class tbOrder
    {
        public long orderid { get; set; }

        public string orderno { get; set; }

        public float cost { get; set; }

        public int paytype { get; set; }
    }

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

}
