using Microsoft.Xrm.Sdk.Client;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace tpi.CrmConnector
{
    public class CrmConnect
    {

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

        public static HttpClient GetHttpClient()
        {

            var resource = ConfigurationManager.AppSettings["Resource"];
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var authority = ConfigurationManager.AppSettings["AuthorityUrl"];

            var userName = ConfigurationManager.AppSettings["CrmUsername"];
            var password = ConfigurationManager.AppSettings["CrmPassword"];

            var authContext = new AuthenticationContext(authority, false);
            var credentials = new UserCredential(userName, password);
            var authResult = authContext.AcquireToken(resource, clientId, credentials);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(resource),
                Timeout = new TimeSpan(0, 2, 0)
            };
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\"");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                authResult.AccessToken);
            return httpClient;
        }

    }
}
