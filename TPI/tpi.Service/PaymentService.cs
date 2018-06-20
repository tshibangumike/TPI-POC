using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class PaymentService
    {

        public static string ClassName = "PaymentService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static bool HasPaymentBeenMade(HttpClient httpClient, string paymentReference, string orderNumber)
        {

            var odataQuery = webApiQueryUrl + "blu_payments?$select=blu_paymentreference&$filter=blu_paymentreference eq '" + paymentReference + "' or  blu_name eq '" + orderNumber + "'";

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse =
                JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return true;
            if (systemUserObject.value.Count > 0) return true;
            return false;
        }

        public static Guid CreatePayment(OrganizationServiceProxy service, Payment payment)
        {

            var entity = new Entity("blu_payment")
            {
                ["blu_name"] = payment.Name,
                ["blu_orderid"] = new EntityReference("salesorder", payment.OrderId),
                ["blu_paymentmethodid"] = new EntityReference("blu_paymentmethod", payment.PaymentMethodId),
                ["blu_paymentdate"] = payment.PaymentDate,
                ["blu_paymentamount"] = new Money(payment.Amount),
                ["blu_paymentreference"] = payment.Reference,
                ["statecode"] = new OptionSetValue(1),
                ["statuscode"] = new OptionSetValue(2)
            };

            return service.Create(entity);

        }

        public static Guid CreatePayment(HttpClient httpClient, Payment payment)
        {
            try
            {
                var paymentObject = new JObject()
                {
                    ["blu_name"] = payment.Name,
                    ["blu_paymentamount"] = payment.Amount,
                    ["blu_paymentdate"] = payment.PaymentDate,
                    ["blu_paymentreference"] = payment.Reference,
                    ["blu_OrderId@odata.bind"] = "/salesorders(" + payment.OrderId + ")",
                    ["blu_PaymentMethodId@odata.bind"] = "/blu_paymentmethods(" + payment.PaymentMethodId + ")",
                    ["statecode"] = payment.StateCode,
                    ["statuscode"] = payment.StatusCode,
                };

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "blu_payments")
                {
                    Content = new StringContent(paymentObject.ToString(), Encoding.UTF8, "application/json")
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

    }
}
