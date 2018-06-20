using Microsoft.Xrm.Sdk.Client;
using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;

namespace Eppei.Crm
{
    public class CrmConnect
    {
        public static OrganizationServiceProxy Service
        {
            get
            {
                string crmUrl = string.Format(ConfigurationManager.AppSettings["OrganisationServiceUrl"]);
                Uri organizationUri = new Uri(crmUrl);
                var clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = ConfigurationManager.AppSettings["CrmUsername"];
                clientCredentials.UserName.Password = ConfigurationManager.AppSettings["CrmPassword"];
                if (!string.IsNullOrEmpty(crmUrl) && crmUrl.Contains("https"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                    { return true; };
                }
                OrganizationServiceProxy organizationServiceProxy = new OrganizationServiceProxy(organizationUri, null, clientCredentials, null);
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(ProxyTypesAssemblyAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    organizationServiceProxy.EnableProxyTypes(Assembly.GetExecutingAssembly());
                else
                    organizationServiceProxy.EnableProxyTypes();
                organizationServiceProxy.Timeout = new TimeSpan(0, 10, 0);
                return organizationServiceProxy;
            }
        }

        public static OrganizationServiceProxy GetService()
        {
            var uri = new Uri(string.Format(ConfigurationManager.AppSettings["OrganisationServiceUrl"]));
            var clientCredentials = new ClientCredentials();
            clientCredentials.UserName.UserName = ConfigurationManager.AppSettings["CrmUsername"];
            clientCredentials.UserName.Password = ConfigurationManager.AppSettings["CrmPassword"];
            var service = new OrganizationServiceProxy(uri, null, clientCredentials, null);
            service.EnableProxyTypes();
            return service;
        }

    }
}
