using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Findparts.ActionFilters
{
    public class RequireEmailConfirmed : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated && !filterContext.HttpContext.User.Identity.IsVerified())
            {
                if ((filterContext.RouteData.Values["controller"] as string) == "Account" && (filterContext.RouteData.Values["action"] as string) == "VerifyEmail")
                {

                } else if (filterContext.HttpContext.Request.HttpMethod == "GET")
                {
                    var routeDic = new RouteValueDictionary();
                    routeDic.Add("controller", "Account");
                    routeDic.Add("action", "VerifyEmail");
                    filterContext.Result = new RedirectToRouteResult(routeDic);
                }
                
            }
            base.OnActionExecuting(filterContext);
        }
    }
}