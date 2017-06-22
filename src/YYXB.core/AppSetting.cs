using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Core
{
    public class AppSetting
    {

        public static String AppCenter
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["AppCenter"]; }
        }

        public static string CookieDomain
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["CookieDomain"]; }
        }


    }
}
