using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace YYXB.Filter
{
    public class ExceptionFilter : FilterAttribute, IExceptionFilter
    {
        void IExceptionFilter.OnException(ExceptionContext filterContext)
        {
            //获取异常信息
            String errorMessages = filterContext.Exception.Message;
            object o = filterContext.RouteData.Route;
            //保存到viewdata
            filterContext.Controller.ViewData["ErrorMessage"] = filterContext.Exception.Message;

            var controllername = filterContext.RouteData.Values["controller"];
            var action = filterContext.RouteData.Values["action"];
            //呈现到view
            log4net.LogManager.GetLogger("ExceptionFilter").Error("Controller:" + controllername + "Action:" + action, filterContext.Exception);

            filterContext.Result = new ViewResult()
            {
                ViewName = "Error",
                ViewData = filterContext.Controller.ViewData,
            };
            //将此处理请求交给异常捕捉请求
            filterContext.ExceptionHandled = true;

        }
    }
}
