using Microsoft.Xrm.Sdk.Client;
using System.Configuration;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using eWAY.Rapid;
using Newtonsoft.Json;
using tpi.CrmConnector;
using tpi.Model;

namespace tpi.Web.Controllers
{
    public class BaseController : Controller
    {

        public CurrentUser GetCurrentUser()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return null;
            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            if (authTicket == null) return null;
            var currentUser = JsonConvert.DeserializeObject<CurrentUser>(authTicket.UserData);
            return currentUser;
        }

        public OrganizationServiceProxy GetService()
        {
            return CrmConnect.GetService(); 
        }

        public HttpClient GetHttpClient()
        {
            return CrmConnect.GetHttpClient();
        }

        public IRapidClient GeteWayClient()
        {
            var apiKey = ConfigurationManager.AppSettings["apiKey"];
            var password = ConfigurationManager.AppSettings["password"];
            var rapidEndpoint = ConfigurationManager.AppSettings["rapidEndpoint"];
            return RapidClientFactory.NewRapidClient(apiKey, password, rapidEndpoint);
        }

    }
}