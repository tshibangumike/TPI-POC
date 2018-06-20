using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class CustomerService
    {

        public static string ClassName = "CustomerService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Guid CreateAccount(IOrganizationService service, Customer customer)
        {

            try
            {

                var entity = new Entity("account")
                {
                    ["name"] = customer.Name,
                    ["emailaddress1"] = customer.EmailAddress,
                    ["telephone1"] = customer.ContactNumber,
                    ["blu_portalregisteredas"] = (int)PortalRegisteredAs.PortalUser,
                };

                return service.Create(entity);

            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }

        }

        //---------------------------------

        public static Guid CreateContact(HttpClient httpClient, Customer customer)
        {

            try
            {

                var contactJObject = new JObject
                {
                    ["firstname"] = customer.Firstname,
                    ["lastname"] = customer.Lastname,
                    ["emailaddress1"] = customer.EmailAddress,
                    ["mobilephone"] = customer.ContactNumber,
                    ["blu_portalregisteredas"] = (int)PortalRegisteredAs.PortalUser,
                };

                if (customer.PriceListId != Guid.Empty)
                {
                    contactJObject["defaultpricelevelid@odata.bind"] = "/pricelevels(" + customer.PriceListId + ")";
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "contacts")
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
                        Name = "Create Contact Error",
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

        public static Guid CreateAccount(HttpClient httpClient, Customer customer)
        {

            try
            {

                var accountJObject = new JObject
                {
                    ["name"] = customer.Name,
                    ["emailaddress1"] = customer.EmailAddress,
                    ["telephone1"] = customer.ContactNumber,
                    ["blu_portalregisteredas"] = (int)PortalRegisteredAs.PortalUser,
                };

                if (customer.PriceListId != Guid.Empty)
                {
                    accountJObject["defaultpricelevelid@odata.bind"] = "/pricelevels(" + customer.PriceListId + ")";
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "accounts")
                {
                    Content = new StringContent(accountJObject.ToString(), Encoding.UTF8, "application/json")
                };
                var response = httpClient.SendAsync(request);
                if (!response.Result.IsSuccessStatusCode)
                {
                    var error = response.Result.Content.ReadAsStringAsync();
                    LogService.LogMessage(httpClient, new Log()
                    {
                        Level = (int)LogType.Error,
                        Name = "Create Account Error",
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

        public static Customer QueryContact(HttpClient httpClient, Guid contactId)
        {

            try
            {

                var odataQuery =
                webApiQueryUrl + "contacts?$select=contactid,fullname,emailaddress1,firstname,lastname,mobilephone,blu_hasanaccount&$filter=statecode eq 0 and contactid eq " + contactId + "";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;

                foreach (var data in systemUserObject.value)
                    return new Customer()
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
                            : data["lastname"].ToString(),
                        EmailAddress = data["emailaddress1"] == null
                            ? ""
                            : data["emailaddress1"].ToString(),
                        ContactNumber = data["mobilephone"] == null
                            ? ""
                            : data["mobilephone"].ToString(),
                        HasAccount = data["blu_hasanaccount"] == null
                        ? false
                        : (bool)data["blu_hasanaccount"],
                        CustomerType = 2
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

        public static Customer QueryAccount(HttpClient httpClient, Guid accountId)
        {

            try
            {

                var odataQuery =
                webApiQueryUrl + "accounts?$" +
                "select=accountid,name,emailaddress1,blu_hasanaccount,telephone1&$filter=statecode eq 0 and accountid eq " + accountId + "";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;

                foreach (var data in systemUserObject.value)
                    return new Customer()
                    {
                        Id = data["accountid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["accountid"].ToString()),
                        Name = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        EmailAddress = data["emailaddress1"] == null
                            ? ""
                            : data["emailaddress1"].ToString(),
                        ContactNumber = data["telephone1"] == null
                            ? ""
                            : data["telephone1"].ToString(),
                        HasAccount = data["blu_hasanaccount"] == null
                        ? false
                        : (bool)data["blu_hasanaccount"],
                        CustomerType = 1
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

        public static bool UpdateCustomer(HttpClient httpClient, Customer customer)
        {

            try
            {

                var customerJObject = new JObject();
                var entityName = string.Empty;
                if (customer.CustomerType == (int)CustomerType.Account)
                {
                    entityName = "accounts";
                    customerJObject["name"] = customer.Name;
                    customerJObject["emailaddress1"] = customer.EmailAddress;
                    customerJObject["telephone1"] = customer.ContactNumber;
                }
                else if (customer.CustomerType == (int)CustomerType.Account)
                {
                    entityName = "contacts";
                    customerJObject["firstname"] = customer.Firstname;
                    customerJObject["lastname"] = customer.Lastname;
                    customerJObject["emailaddress1"] = customer.EmailAddress;
                    customerJObject["mobilephone"] = customer.ContactNumber;
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                    webApiQueryUrl + entityName + "(" + customer.Id + ")")
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
