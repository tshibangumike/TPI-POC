using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class OrderService
    {

        public static string ClassName = "OrderService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Order QueryOrder(HttpClient httpClient, Guid orderId)
        {

            try
            {

                var odataQuery =
                    webApiQueryUrl + "salesorders(" + orderId + ")?$select=_customerid_value,_opportunityid_value,ordernumber,salesorderid,totalamount,statecode";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse =
                    JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic data = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                var order = new Order()
                {
                    Id = Guid.Parse(data["salesorderid"].ToString()),
                    Name = data["name"],
                    TotalAmount = (decimal)data["totalamount"],
                    TotalAmountDisplayName = data["totalamount@OData.Community.Display.V1.FormattedValue"],
                    OrderNumber = data["ordernumber"],
                    CustomerId = Guid.Parse(data["_customerid_value"].ToString()),
                    StateCode = data["statecode"]
                };
                return order;

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

        public static Order QueryOrderByOrderNumber(HttpClient httpClient, string orderNumber)
        {

            try
            {

                var odataQuery =
                    webApiQueryUrl + "salesorders?$select=_customerid_value,blu_currentusersessiondetail,_opportunityid_value,ordernumber,salesorderid,totalamount,statecode&$filter=ordernumber eq '" + orderNumber + "'";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count != 1) return null;

                foreach (var data in systemUserObject.value)
                {
                    var order = new Order()
                    {
                        Id = Guid.Parse(data["salesorderid"].ToString()),
                        Name = data["name"],
                        TotalAmount = (decimal)data["totalamount"],
                        TotalAmountDisplayName = data["totalamount@OData.Community.Display.V1.FormattedValue"],
                        OrderNumber = data["ordernumber"],
                        CustomerId = Guid.Parse(data["_customerid_value"].ToString()),
                        StateCode = data["statecode"],
                        CurrentUserSessionDetail = data["blu_currentusersessiondetail"]
                    };
                    return order;
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

        public static Order QueryOrderByOpportunity(HttpClient httpClient, Guid opportunityId)
        {

            try
            {
                var odataQuery =
                    webApiQueryUrl + "salesorders?$select=_opportunityid_value,salesorderid,_customerid_value,ordernumber,totalamount&$filter=_opportunityid_value eq " + opportunityId;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count != 1) return null;

                foreach (var data in systemUserObject.value)
                {
                    var order = new Order()
                    {
                        Id = Guid.Parse(data["salesorderid"].ToString()),
                        TotalAmount = (decimal)data["totalamount"],
                        TotalAmountDisplayName = data["totalamount@OData.Community.Display.V1.FormattedValue"],
                        OrderNumber = data["ordernumber"],
                        CustomerId = Guid.Parse(data["_customerid_value"].ToString())
                    };
                    return order;
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

        public static Guid ConvertOrderToInvoice(OrganizationServiceProxy service, Guid orderId)
        {

            try
            {

                var convertOrderRequest =
                    new ConvertSalesOrderToInvoiceRequest()
                    {
                        SalesOrderId = orderId,
                        ColumnSet = new ColumnSet(true)
                    };
                var convertOrderResponse =
                    (ConvertSalesOrderToInvoiceResponse)service.Execute(convertOrderRequest);

                var invoiceId = convertOrderResponse.Entity.Id;

                return invoiceId;

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
                return Guid.Empty;
            }

        }

        public static void SetOrderState(OrganizationServiceProxy service, Guid orderId)
        {

            try
            {

                var state = new SetStateRequest
                {
                    State = new OptionSetValue(4),
                    Status = new OptionSetValue(100003),
                    EntityMoniker = new EntityReference("salesorder", orderId)
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

        public static bool UpdateOrder(HttpClient httpClient, Order order)
        {

            try
            {

                var orderJObject = new JObject();

                if (order.PropertyId != Guid.Empty)
                {
                    orderJObject["blu_RegardingPropertyId@odata.bind"] = "/blu_inspections(" + order.PropertyId + ")";
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                    webApiQueryUrl + "salesorders(" + order.Id + ")")
                {
                    Content = new StringContent(orderJObject.ToString(), Encoding.UTF8, "application/json")
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

        public static Guid CreateOrderProduct(HttpClient httpClient, OrderProduct orderProduct)
        {

            try
            {

                var orderProductJObject = new JObject()
                {
                    ["salesorderid@odata.bind"] = "/salesorders(" + orderProduct.OrderId + ")",
                    ["productid@odata.bind"] = "/products(" + orderProduct.ProductId + ")",
                    ["uomid@odata.bind"] = "/uoms(" + orderProduct.UomId + ")",
                    ["quantity"] = 1,
                };

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "salesorderdetails")
                {
                    Content = new StringContent(orderProductJObject.ToString(), Encoding.UTF8, "application/json")
                };
                var response = httpClient.SendAsync(request);
                if (!response.Result.IsSuccessStatusCode)
                {
                    var error = response.Result.Content.ReadAsStringAsync();
                    LogService.LogMessage(httpClient, new Log()
                    {
                        Level = (int)LogType.Error,
                        Name = "Create Opportunity Product Error",
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
