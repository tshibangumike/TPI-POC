using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class PortalUserService
    {

        public static string ClassName = "PortalUserService";
        public static string WebApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Guid CreatePortalUser(IOrganizationService service, PortalUser portalUser)
        {
            try
            {
                var entity = new Entity("blu_portaluser")
                {
                    ["blu_name"] = portalUser.Name,
                    ["blu_username"] = portalUser.Username,
                    ["blu_password"] = portalUser.Password
                };
                return service.Create(entity);
            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }
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

        //-----------------------------------------------------------------------------------------------

        public static Guid CreatePortalUser(HttpClient httpClient, PortalUser portalUser, int type)
        {

            try
            {

                var contactJObject = new JObject
                {
                    ["blu_name"] = portalUser.Name,
                    ["blu_username"] = portalUser.Username,
                    ["blu_password"] = portalUser.Password,
                    ["blu_portaluserrole"] = portalUser.PortalUserRole,
                };
                switch (type)
                {
                    case (int)CustomerType.Account:
                        contactJObject["blu_Customer_account@odata.bind"] = "/accounts(" + portalUser.CustomerId + ")";
                        break;
                    case (int)CustomerType.Contact:
                        contactJObject["blu_Customer_contact@odata.bind"] = "/contacts(" + portalUser.CustomerId + ")";
                        break;
                }

                switch (portalUser.PortalUserRole)
                {
                    case (int)PortalUserrole.Consumer:
                        contactJObject["statuscode"] = "1";
                        break;
                    case (int)PortalUserrole.Agent:
                    case (int)PortalUserrole.Vendor:
                        contactJObject["statuscode"] = (int)PortalUserStatusCode.Active;
                        break;
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    WebApiQueryUrl + "blu_portalusers")
                {
                    Content = new StringContent(contactJObject.ToString(), Encoding.UTF8, "application/json")
                };
                var response = httpClient.SendAsync(request);
                if (!response.Result.IsSuccessStatusCode)
                {
                    var error = response.Result.Content.ReadAsStringAsync();
                    LogService.LogMessage(httpClient, new Log()
                    {
                        Level = (int)LogType.Error,
                        Name = "Query Error",
                        FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                        Message = error.Result.ToString()
                    });
                    return Guid.Empty;
                }
                var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
                if (recordUrl == null) return Guid.Empty;
                var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
                var recordId = Guid.Parse(splitRetrievedData[1]);
                return recordId;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Guid.Empty;
            }
        }

        public static PortalUser QueryPortalUserByUsernameByPassword(HttpClient httpClient, string username,
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
            fetchXml += "    <link-entity name='account' from='accountid' to='blu_customer' visible='false' link-type='outer' alias='account'>";
            fetchXml += "      <attribute name='blu_hasanaccount' />";
            fetchXml += "    </link-entity>";
            fetchXml += "    <link-entity name='contact' from='contactid' to='blu_customer' visible='false' link-type='outer' alias='contact'>";
            fetchXml += "      <attribute name='blu_hasanaccount' />";
            fetchXml += "    </link-entity>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = WebApiQueryUrl + "blu_portalusers?fetchXml=" + encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;
            if (systemUserObject.value.Count == 0) return null;

            foreach (var data in systemUserObject.value)
            {

                var customer = new Customer()
                {
                    Id = data["_blu_customer_value"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["_blu_customer_value"].ToString()),
                    Name = data["_blu_customer_value@OData.Community.Display.V1.FormattedValue"] == null
                            ? ""
                            : data["_blu_customer_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                    HasAccount = data["account_x002e_blu_hasanaccount"] == null
                    ? (data["contact_x002e_blu_hasanaccount"] == null ? false : (bool)data["contact_x002e_blu_hasanaccount"])
                    : (bool)data["account_x002e_blu_hasanaccount"],
                    CustomerType = data["_blu_customer_value@Microsoft.Dynamics.CRM.lookuplogicalname"] == null ? 0 : (data["_blu_customer_value@Microsoft.Dynamics.CRM.lookuplogicalname"].ToString() == "account" ? 1 : 2),
                    CustomerTypeText = data["_blu_customer_value@Microsoft.Dynamics.CRM.lookuplogicalname"] == null ? "" : data["_blu_customer_value@Microsoft.Dynamics.CRM.lookuplogicalname"].ToString()
                };
                var portalUser = new PortalUser()
                {
                    Id = data["blu_portaluserid"] == null
                    ? Guid.Empty
                    : Guid.Parse(data["blu_portaluserid"].ToString()),
                    Name = data["blu_name"] == null ? "" : data["blu_name"].ToString(),
                    Username = data["blu_username"] == null ? "" : data["blu_username"].ToString(),
                    CustomerId = customer.Id,
                    PortalUserRole = data["blu_portaluserrole"] == null ? 0 : (int)data["blu_portaluserrole"],
                    Customer = customer,
                    PriceLists = new List<PriceList>()
                };
                return portalUser;

            }

            return null;

        }

        public static PortalUser QueryPortalUserByUsername(HttpClient httpClient, string username)
        {

            try
            {

                var odataQuery =
                    WebApiQueryUrl + "blu_portalusers?$select=_blu_customer_value,blu_name,blu_portaluserrole&$filter=statecode eq 0 and statuscode eq 1 and blu_username eq '" + username + "'";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;
                foreach (var data in systemUserObject.value)
                {
                    return new PortalUser()
                    {
                        Id = data["blu_portaluserid"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["blu_portaluserid"].ToString()),
                        Name = data["blu_name"] == null ? "" : data["blu_name"].ToString(),
                        PriceLists = new List<PriceList>()
                    };
                }
                return null;
            }

            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return null;
            }

        }

        public static bool UpdatePassword(HttpClient httpClient, PortalUser  portalUser)
        {

            try
            {
                var customerJObject = new JObject
                {
                    ["blu_password"] = portalUser.Password
                };

                var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                     WebApiQueryUrl + "blu_portalusers(" + portalUser.Id + ")")
                {
                    Content = new StringContent(customerJObject.ToString(), Encoding.UTF8, "application/json")
                };

                var response = httpClient.SendAsync(request);
                if (response.Result.IsSuccessStatusCode) return true;
                var error = response.Result.Content.ReadAsStringAsync();
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = "Update Error",
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = error.Result.ToString()
                });
                return false;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return false;
            }
        }

    }
}
