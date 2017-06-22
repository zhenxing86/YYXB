using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace YYXB.Core
{
    public class SessionManager
    {
        private static object lockHelper = new object();
        private static SessionManager instance = null;
        private static HttpRequestBase httpRequest = null;

        private SessionManager()
        {

        }

        public Int32 UserID
        {
            get
            {
                if (httpRequest.Cookies["AtsCMSUserCookie"] != null && httpRequest.Cookies["AtsCMSUserCookie"].Values["UserID"] != null)
                {
                    return Convert.ToInt32(httpRequest.Cookies["AtsCMSUserCookie"].Values["UserID"]);
                }
                else
                {
                    return 0;
                }
            }
        }

        public static SessionManager GetSessionInstance(HttpRequestBase request1)
        {
            httpRequest = request1;
            if (instance == null)
            {
                lock (lockHelper)
                {
                    if (instance == null)
                    {

                        instance = new SessionManager();
                    }
                }
            }
            return instance;
        }
    }
}
