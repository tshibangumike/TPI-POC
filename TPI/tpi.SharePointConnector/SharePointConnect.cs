using Microsoft.SharePoint.Client;
using System.Configuration;

namespace tpi.SharePointConnector
{
    public class SharePointConnect
    {

        public static ClientContext GetClientContext()  
        {
                var clientContext = new ClientContext(ConfigurationManager.AppSettings["SharePointSiteUrl"])
                {
                    RequestTimeout = 1000000,
                    Credentials = Utils.GetO365Credentials(ConfigurationManager.AppSettings["CrmUsername"], ConfigurationManager.AppSettings["CrmPassword"])
                };
                var web = clientContext.Web;
                return clientContext;
        }

    }

}
