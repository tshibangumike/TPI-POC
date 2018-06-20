using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using tpi.Model;

namespace tpi.Service
{
    public class LogService
    {
        public static void LogMessage(HttpClient httpClient, Log log)
        {
            var logObject = new JObject()
            {
                ["blu_name"] = log.Name,
                ["blu_message"] = log.Message,
                ["blu_level"] = log.Level,
                ["blu_functionname"] = log.FunctionName,
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://bluleaderdemo2dev.crm6.dynamics.com/api/data/v8.2/blu_logs")
            {
                Content = new StringContent(logObject.ToString(), Encoding.UTF8, "application/json")
            };
            var response = httpClient.SendAsync(request);
            if (!response.Result.IsSuccessStatusCode)
            {
                var error = response.Result.Content.ReadAsStringAsync();
                return;
            }
            var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
            if (recordUrl == null) return;
            var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
            var recordId = Guid.Parse(splitRetrievedData[1]);
            return;
        }

        public static void LogMessage(OrganizationServiceProxy service, Log log)
        {

            var logEntity = new Entity("blu_log");
            logEntity["blu_name"] = log.Name;
            logEntity["blu_message"] = log.Message;
            logEntity["blu_level"] = (int)log.Level;
            logEntity["blu_functionname"] = log.FunctionName;

            service.Create(logEntity);

        }

    }
}
