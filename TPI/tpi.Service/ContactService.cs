using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class ContactService
    {

        public static string ClassName = "CustomerService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Guid CreateContact(OrganizationServiceProxy service, Contact contact)
        {

            var entity = new Entity("contact")
            {
                Id = contact.Id,
                ["firstname"] = contact.Firstname,
                ["lastname"] = contact.Lastname,
                ["emailaddress1"] = contact.EmailAddress,
                ["mobilephone"] = contact.ContactNumber,
                ["blu_portalregisteredas"] = new OptionSetValue(858890000)
            };
            return service.Create(entity);

        }

        public static List<Customer> QueryContacts(HttpClient httpClient)
        {

            var contacts = new List<Customer>();

            try
            {

                var odataQuery =
                webApiQueryUrl + "contacts?$select=contactid,fullname,emailaddress1,firstname,lastname,mobilephone,blu_hasanaccount&$filter=statecode eq 0";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;

                foreach (var data in systemUserObject.value)
                {
                    contacts.Add(new Customer()
                    {
                        Id = data["contactid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["contactid"].ToString()),
                        Name = data["fullname"] == null
                            ? ""
                            : data["fullname"].ToString(),
                        Firstname = data["firstname"] == null
                            ? ""
                            : data["firstname"].ToString(),
                        Lastname = data["lastname"] == null
                            ? ""
                            : data["lastname"].ToString()
                    });
                }

                return contacts;

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
