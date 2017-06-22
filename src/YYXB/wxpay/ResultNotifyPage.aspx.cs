﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace WxPayAPI
{
    public partial class ResultNotifyPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            log4net.LogManager.GetLogger("sgspay").Error("WxPay ResultNotifyPage.");
            ResultNotify resultNotify = new ResultNotify(this);
            resultNotify.ProcessNotify();
        }       
    }
}