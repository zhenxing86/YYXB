using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    public class UserInfo
    {
        // id,channelid,username,usertype,devid,createtime,cellphone,email

        public int pcount { get; set; }
        public int id {get;set;} 
        public string channelid {get;set;} 
        public string username {get;set;} 
        public int usertype {get;set;} 
        public string devid {get;set;} 
        public DateTime createtime {get;set;} 
        public string cellphone {get;set;}
        public string email { get; set; }

        public string createtimestr
        {
            get
            {
                return createtime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public string usertypestr
        {
            get
            {
                return usertype > 0 ? "VIP用户" : "普通用户";
            }
        }

        public int status
        {
            get
            {
                return usertype > 0 ? 0: 1;
            }
        }


        public string password { get; set; }
    }

}
