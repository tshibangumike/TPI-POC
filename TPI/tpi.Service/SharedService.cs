using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using tpi.Model;
using System.Collections.Generic;

namespace tpi.Service
{
    public class SharedService
    {
        public static string UrlEncode(string fetchXml)
        {
            var encodedUrl = fetchXml.Replace("<", "%3C").Replace(" ", "%20").Replace("=", "%3D").Replace("=", "%3D").Replace("\"", "%22").Replace(">", "%3E");
            return encodedUrl;
        }

        public static HttpCookie GetAuthenticationTicket(CurrentUser currentUser)
        {
            // Create the authentication ticket with custom user data.
            var serializer = new JavaScriptSerializer();
            var userData = serializer.Serialize(currentUser);
            var ticket = new FormsAuthenticationTicket(1,
                currentUser.PortalUser.CustomerId.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(45),
                true,
                userData,
                FormsAuthentication.FormsCookiePath);
            // Encrypt the ticket.
            var encTicket = FormsAuthentication.Encrypt(ticket);
            var httpCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            if (ticket.IsPersistent) httpCookie.Expires = ticket.Expiration;
            return httpCookie;
        }

        public static DateTime RetrieveLocalTimeFromUtcTime(DateTime utcTime, int? timeZoneCode, IOrganizationService service)
        {

            if (!timeZoneCode.HasValue)
                return DateTime.Now;

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;

        }

        public static Guid CreateEntity(HttpClient httpClient, JObject jObject, string entityName)
        {

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://bluleaderdemo2dev.crm6.dynamics.com/api/data/v8.2/" + entityName)
            {
                Content = new StringContent(jObject.ToString(), Encoding.UTF8, "application/json")
            };
            var response = httpClient.SendAsync(request);
            if (!response.Result.IsSuccessStatusCode) return Guid.Empty;
            var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
            if (recordUrl == null) return Guid.Empty;
            var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
            var recordId = Guid.Parse(splitRetrievedData[1]);
            return recordId;
        }

        public static DateTime ConvertCrmDateToUserDate(IOrganizationService service, Guid userGuid, DateTime inputDate)
        {
            //replace userid with id of user
            Entity userSettings = service.Retrieve("usersettings", userGuid, new ColumnSet("timezonecode"));
            //timezonecode for UTC is 85
            int timeZoneCode = 85;

            //retrieving timezonecode from usersetting
            if ((userSettings != null) && (userSettings["timezonecode"] != null))
            {
                timeZoneCode = (int)userSettings["timezonecode"];
            }
            //retrieving standard name
            var qe = new QueryExpression("timezonedefinition");
            qe.ColumnSet = new ColumnSet("standardname");
            qe.Criteria.AddCondition("timezonecode", ConditionOperator.Equal, timeZoneCode);
            EntityCollection TimeZoneDef = service.RetrieveMultiple(qe);

            TimeZoneInfo userTimeZone = null;
            if (TimeZoneDef.Entities.Count == 1)
            {
                String timezonename = TimeZoneDef.Entities[0]["standardname"].ToString();
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezonename);
            }
            //converting date from UTC to user time zone
            DateTime cstTime =  TimeZoneInfo.ConvertTime(inputDate, userTimeZone, TimeZoneInfo.Local);

            return cstTime;
        }

        public static string GenerateCoupon(int length)
        {
            Random random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }

        public static TimeZoneInfo GetUserTimeZone(IOrganizationService service, Guid userGuid)
        {
            //replace userid with id of user
            Entity userSettings = service.Retrieve("usersettings", userGuid, new ColumnSet("timezonecode"));
            //timezonecode for UTC is 85
            int timeZoneCode = 85;

            //retrieving timezonecode from usersetting
            if ((userSettings != null) && (userSettings["timezonecode"] != null))
            {
                timeZoneCode = (int)userSettings["timezonecode"];
            }
            //retrieving standard name
            var qe = new QueryExpression("timezonedefinition");
            qe.ColumnSet = new ColumnSet("standardname");
            qe.Criteria.AddCondition("timezonecode", ConditionOperator.Equal, timeZoneCode);
            EntityCollection TimeZoneDef = service.RetrieveMultiple(qe);

            TimeZoneInfo userTimeZone = null;
            if (TimeZoneDef.Entities.Count == 1)
            {
                String timezonename = TimeZoneDef.Entities[0]["standardname"].ToString();
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezonename);
            }
            return userTimeZone;
        }

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }

    }
}
