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
    public class PriceListService
    {

        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];
        public static string ClassName = "PriceListService";

        public static List<PriceList> QueryPriceListByPortalUserRole(HttpClient httpClient, int portalUserRole)
        {

            try
            {

                var priceLists = new List<PriceList>();
                var odataQuery =
                    webApiQueryUrl + "pricelevels?$" +
                    "select=blu_portaluserrole,name,_blu_questioncategoryid_value,blu_reportpriority&$filter=statecode eq 0 and  blu_portaluserrole eq " +
                    portalUserRole;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {
                    var priceLevel = new PriceList()
                    {
                        Id = data["pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevelid"].ToString()),
                        Name = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        QuestionSetupId = data["_blu_questioncategoryid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_blu_questioncategoryid_value"].ToString()),
                    };
                    priceLists.Add(priceLevel);
                }

                return priceLists;

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

        public static List<PriceList> QueryPriceListByCustomer(HttpClient httpClient, Guid customerId)
        {

            try
            {

                var customerType = 0;
                var contact = CustomerService.QueryContact(httpClient, customerId);
                if (contact != null && contact.CustomerType != 0) customerType = contact.CustomerType;

                var account = CustomerService.QueryAccount(httpClient, customerId);
                if (account != null && account.CustomerType != 0) customerType = account.CustomerType;

                var priceLists = new List<PriceList>();
                var fetchXml = string.Empty;
                fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
                fetchXml += "  <entity name='pricelevel'>";
                fetchXml += "    <attribute name='name' />";
                fetchXml += "    <attribute name='pricelevelid' />";
                fetchXml += "    <attribute name='blu_questioncategoryid' />";
                fetchXml += "    <attribute name='blu_portaluserrole' />";
                fetchXml += "    <filter type='and'>";
                fetchXml += "      <condition attribute='statecode' operator='eq' value='0' />";
                fetchXml += "    </filter>";
                if (customerType == (int)CustomerType.Account)
                {
                    fetchXml +=
                        "    <link-entity name='account' from='defaultpricelevelid' to='pricelevelid' link-type='inner' alias='am'>";
                    fetchXml += "      <filter type='and'>";
                    fetchXml +=
                        "        <condition attribute='accountid' operator='eq' value='" + customerId + "' />";
                    fetchXml += "      </filter>";
                    fetchXml += "    </link-entity>";
                }
                else if (customerType == (int)CustomerType.Contact)
                {
                    fetchXml +=
                        "    <link-entity name='contact' from='defaultpricelevelid' to='pricelevelid' link-type='inner' alias='an'>";
                    fetchXml += "      <filter type='and'>";
                    fetchXml +=
                        "        <condition attribute='contactid' operator='eq' value='" + customerId + "' />";
                    fetchXml += "      </filter>";
                    fetchXml += "    </link-entity>";
                }

                fetchXml += "  </entity>";
                fetchXml += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXml);
                var odataQuery = webApiQueryUrl + "pricelevels?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {

                    var priceLevel = new PriceList()
                    {
                        Id = data["pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevelid"].ToString()),
                        Name = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        QuestionSetupId = data["_blu_questioncategoryid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_blu_questioncategoryid_value"].ToString()),
                    };
                    priceLists.Add(priceLevel);
                }

                return priceLists;

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

        public static PriceList QueryPriceList(HttpClient httpClient, Guid priceListId)
        {

            try
            {

                var fetchXml = string.Empty;
                fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXml += "  <entity name='pricelevel'>";
                fetchXml += "    <attribute name='name' />";
                fetchXml += "    <attribute name='blu_portaluserrole' />";
                fetchXml += "    <attribute name='pricelevelid' />";
                fetchXml += "    <attribute name='blu_paymentmethodtype' />";
                fetchXml += "    <attribute name='blu_reportpriority' />";
                fetchXml += "    <attribute name='blu_questioncategoryid' />";
                fetchXml += "    <filter type='and'>";
                fetchXml += "      <condition attribute='pricelevelid' operator='eq' value='" + priceListId + "' />";
                fetchXml += "    </filter>";
                fetchXml += "  </entity>";
                fetchXml += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXml);
                var odataQuery = webApiQueryUrl + "pricelevels?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {

                    return new PriceList()
                    {
                        Id = data["pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevelid"].ToString()),
                        Name = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        QuestionSetupId = data["_blu_questioncategoryid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_blu_questioncategoryid_value"].ToString()),
                        PortalUserRole = data["blu_portaluserrole"] == null
                            ? 0
                            : (int)data["blu_portaluserrole"],
                        ReportPriority = data["blu_reportpriority"] == null
                            ? 0
                            : (int)data["blu_reportpriority"],
                        PaymentMethods = data["blu_paymentmethodtype"] == null
                            ? new string[0]
                            : data["blu_paymentmethodtype"].ToString().Split(',')

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

        public static PriceList QueryDefaultPriceList(HttpClient httpClient)
        {

            try
            {

                var odataQuery =
                    webApiQueryUrl + "pricelevels?$" +
                    "select=blu_portaluserrole,name,_blu_questioncategoryid_value,blu_reportpriority&$filter=statecode eq 0 and  name eq 'Retail'";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {
                    return new PriceList()
                    {
                        Id = data["pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevelid"].ToString()),
                        Name = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        QuestionSetupId = data["_blu_questioncategoryid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_blu_questioncategoryid_value"].ToString()),
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

    }
}
