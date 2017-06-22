using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Web;
using System.IO;

namespace YYXB
{
    public class JsonNetResult : JsonResult
    {
        public JsonSerializerSettings Settings { get; private set; }

        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                //这句是解决问题的关键,也就是json.net官方给出的解决配置选项.                 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("JSON GET is not allowed");
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json";//string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;
            if (this.ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (this.Data == null)
                return;
            var scriptSerializer = JsonSerializer.Create(this.Settings);
            using (var sw = new StringWriter())
            {
                scriptSerializer.Serialize(sw, this.Data);
                response.Write(sw.ToString());
            }
        }
    }
}
