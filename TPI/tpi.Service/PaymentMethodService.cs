using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class PaymentMethodService
    {

        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static List<PaymentMethod> QueryPaymentMethods(HttpClient httpClient)
        {

            var odataQuery =
                webApiQueryUrl + "blu_paymentmethods?$select=blu_name,blu_paymentmethodtype&$filter=statecode eq 0";

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse =
                JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;

            var paymentMethods = new List<PaymentMethod>();

            foreach (var data in systemUserObject.value)
            {

                var opportunity = new PaymentMethod()
                {
                    Id = Guid.Parse(data["blu_paymentmethodid"].ToString()),
                    Name = data["blu_name"],
                    Type = data["blu_paymentmethodtype"] == null ? 0 : (int)data["blu_paymentmethodtype"]
                };

                paymentMethods.Add(opportunity);

            }

            return paymentMethods;

        }

        public static List<PaymentMethod> QueryPaymentMethodsByCategory(HttpClient httpClient, string[] paymentMethodCategories)
        {



            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='blu_paymentmethod'>";
            fetchXml += "    <attribute name='blu_name' />";
            fetchXml += "    <attribute name='blu_paymentmethodid' />";
            fetchXml += "    <filter type='and'>";
            if (paymentMethodCategories != null && paymentMethodCategories.Length > 0)
            {
                fetchXml += "      <condition attribute='blu_paymentmethodtype' operator='in'>";
                foreach (var paymentMethodCategory in paymentMethodCategories)
                {
                    fetchXml += "        <value>" + paymentMethodCategory + "</value>";
                }
                fetchXml += "      </condition>";
            }
            fetchXml += "    </filter>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "blu_paymentmethods?fetchXml=" +
                                encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse =
                JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;

            var paymentMethods = new List<PaymentMethod>();

            foreach (var data in systemUserObject.value)
            {

                var opportunity = new PaymentMethod()
                {
                    Id = Guid.Parse(data["blu_paymentmethodid"].ToString()),
                    Name = data["blu_name"],
                };

                paymentMethods.Add(opportunity);

            }

            return paymentMethods;

        }

    }
}
