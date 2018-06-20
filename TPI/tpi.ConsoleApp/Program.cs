using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using eWAY.Rapid;
using eWAY.Rapid.Enums;
using eWAY.Rapid.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Crm.Sdk.Samples.HelperCode;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.CrmConnector;
using tpi.Model;
using tpi.Service;
using Customer = tpi.Model.Customer;
using Payment = tpi.Model.Payment;

namespace tpi.ConsoleApp
{
    internal class Program
    {
        // username and password
        private static string _userName = "";
        private static string _password = "";
        // Crm Url
        private static string _resource = "";
        // Application's Client Id
        private static string _clientId = "";
        // Redirct Uri specified during registration of application
        private static string _redirectUri = "";
        // Authroiztion Endpoint
        private static string _authority = "";

        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        private static AuthenticationResult _authResult;

        private static void Main(string[] args)
        {

            var service = CrmConnect.GetService();

            var entity = new Entity
            {
                ["name"] = "Test Account",
                ["emailaddress1"] = "test@property18.co.za",
                ["telephone1"] = "0113452255",
            };

            service.Create(entity);


            Console.WriteLine("Done");
            Console.Read();

        }

        public static PortalUser QueryPortalUserByUsernameByPassword(IOrganizationService service, string username,
             string password)
        {
            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='blu_portaluser'>";
            fetchXml += "    <attribute name='blu_portaluserid' />";
            fetchXml += "    <attribute name='blu_name' />";
            fetchXml += "    <attribute name='blu_username' />";
            fetchXml += "    <attribute name='blu_portaluserrole' />";
            fetchXml += "    <attribute name='blu_customer' />";
            fetchXml += "    <order attribute='blu_name' descending='false' />";
            fetchXml += "    <filter type='and'>";
            fetchXml += "      <condition attribute='blu_username' operator='eq' value='" + username + "' />";
            fetchXml += "      <condition attribute='blu_password' operator='eq' value='" + password + "' />";
            fetchXml += "    </filter>";
            fetchXml +=
                "    <link-entity name='account' from='accountid' to='blu_customer' visible='false' link-type='outer' alias='account'>";
            fetchXml += "      <attribute name='blu_hasanaccount' />";
            fetchXml += "    </link-entity>";
            fetchXml +=
                "    <link-entity name='contact' from='contactid' to='blu_customer' visible='false' link-type='outer' alias='contact'>";
            fetchXml += "      <attribute name='blu_hasanaccount' />";
            fetchXml += "    </link-entity>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var portalUsers = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (portalUsers.Entities.Count == 0) return null;

            foreach (var entity in portalUsers.Entities)
            {
                var portalUser = new PortalUser()
                {
                    Id = (Guid)entity["blu_portaluserid"],
                    Name = (string)entity["blu_name"]
                };
                return portalUser;
            }

            return null;
        }

    }
}