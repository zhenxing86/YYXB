using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using System.Data;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;

using YYXB.Model;
using YYXB.Core.DataProxy;

namespace YYXB.Controllers
{
    public class HomeController : BaseControllers.BaseController
    {

        public ActionResult Index(Int32 id)
        {
            return View();
        }

        //public JsonResult ExecJson(Int32 id)
        //{
        //    string jsonstr = GetValue("txtjson");

        //    Json json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json>(jsonstr);
        //    foreach (Content content in json.data.content)
        //    {
        //        AddContent(content, 1);
        //    }
        //    return this.Json(1, JsonRequestBehavior.AllowGet);
        //}


    }

}
