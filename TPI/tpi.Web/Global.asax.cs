using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using tpi.Model;

namespace tpi.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {

            var httpCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (httpCookie == null) return;
            var authTicket = FormsAuthentication.Decrypt(httpCookie.Value);
            if (authTicket == null) return;
            var currentUser = JsonConvert.DeserializeObject<CurrentUser>(authTicket.UserData);
            //HttpContext.Current.User = currentUser;

        }
    }
}
