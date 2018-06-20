using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class SystemUserService
    {

        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];
        public static string ClassName = "SystemUserService";

        public static List<SystemUser> QuerySystemUsers(HttpClient httpClient)
        {
            var systemUsers = new List<SystemUser>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='systemuser'>";
            fetchXml += "    <attribute name='fullname' />";
            fetchXml += "    <attribute name='systemuserid' />";
            fetchXml += "    <attribute name='blu_inspectorskills' />";
            fetchXml += "    <order attribute='fullname' descending='false' />";
            fetchXml += "    <filter type='and'>";
            fetchXml += "      <condition attribute='blu_isaninspector' operator='eq' value='1' />";
            fetchXml += "      <condition attribute='blu_inspectorskills' operator='not-null' />";
            fetchXml += "    </filter>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "systemusers?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return systemUsers;
            if (systemUserObject.value.Count == 0) return systemUsers;

            foreach (var data in systemUserObject.value)
            {
                systemUsers.Add(
                    new SystemUser
                    {
                        Id = data["systemuserid"] == null ? Guid.Empty : Guid.Parse(data["systemuserid"].ToString()),
                        Fullname = data["fullname"] == null ? "" : data["fullname"].ToString(),
                        InspectorSkills = data["blu_inspectorskills"] == null
                            ? new List<string>()
                            : data["blu_inspectorskills"].ToString().Split(',')
                    });
            }

            return systemUsers;
        }

        public static List<SystemUser> QuerySystemUsersBySkills(HttpClient httpClient, string[] skills)
        {
            var systemUsers = new List<SystemUser>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='systemuser'>";
            fetchXml += "    <attribute name='fullname' />";
            fetchXml += "    <attribute name='systemuserid' />";
            fetchXml += "    <attribute name='blu_inspectorskills' />";
            fetchXml += "    <order attribute='fullname' descending='false' />";
            fetchXml += "    <filter type='and'>";
            fetchXml += "      <condition attribute='blu_isaninspector' operator='eq' value='1' />";
            if (skills != null && skills.Length > 0)
            {
                fetchXml += "      <condition attribute='blu_inspectorskills' operator='contain-values'>";
                foreach (var skill in skills)
                {
                    fetchXml += "      <value>" + skill + "</value>";
                }
                fetchXml += "      </condition>";
            }
            fetchXml += "    </filter>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "systemusers?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return systemUsers;
            if (systemUserObject.value.Count == 0) return systemUsers;

            foreach (var data in systemUserObject.value)
            {
                systemUsers.Add(
                    new SystemUser
                    {
                        Id = data["systemuserid"] == null ? Guid.Empty : Guid.Parse(data["systemuserid"].ToString()),
                        Fullname = data["fullname"] == null ? "" : data["fullname"].ToString(),
                        InspectorSkills = data["blu_inspectorskills"] == null
                            ? new string[0]
                            : data["blu_inspectorskills"].ToString().Split(',')
                    });

            }

            return systemUsers;
        }

        public static SystemUser QuerySystemUser(HttpClient httpClient, string systemUserDomainName)
        {

            try
            {
                var odataQuery =
                    webApiQueryUrl + "systemusers?$select=fullname,systemuserid&$filter=domainname eq '"+ systemUserDomainName + "'";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());

                foreach (var data in systemUserObject.value)
                    return new SystemUser()
                    {
                        Id = Guid.Parse(data["systemuserid"].ToString()),
                        Fullname = data["fullname@OData.Community.Display.V1.FormattedValue"]
                    };
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
    }
}
