using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class VoucherService
    {

        public static string ClassName = "VoucherService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Voucher QueryVouchersByVoucherNumberByCustomer(HttpClient httpClient, string voucherNumber,
            Guid customerId)
        {

            try
            {

                var odataQuery = webApiQueryUrl + "blu_vouchers?$select=blu_amount,blu_vouchernumber&$filter=statecode eq 0 and  statuscode eq 1 and  _blu_customer_value eq " +
                             customerId + " and  blu_vouchernumber eq '" + voucherNumber + "'";
            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            foreach (var data in systemUserObject.value)
            {
                return new Voucher()
                {
                    Id = data["blu_voucherid"] == null ? Guid.Empty : Guid.Parse(data["blu_voucherid"].ToString()),
                    VoucherNumber = data["blu_vouchernumber"] == null ? "" : data["blu_vouchernumber"].ToString(),
                    Amount = data["blu_amount"] == null ? 0 : (decimal) data["blu_amount"],
                    AmountDisplayName = data["blu_amount@OData.Community.Display.V1.FormattedValue"] == null
                        ? "0"
                        : data["blu_amount@OData.Community.Display.V1.FormattedValue"].ToString()
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

        public static void SetVoucherState(OrganizationServiceProxy service, Guid voucherId)
        {

            try
            {

                var state = new SetStateRequest
                {
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(2),
                    EntityMoniker = new EntityReference("blu_voucher", voucherId)
                };

                var stateSet = (SetStateResponse)service.Execute(state);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(service, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
            }

        }

    }
}