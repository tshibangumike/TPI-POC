using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using tpi.Model;
using Microsoft.Crm.Sdk.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Configuration;

namespace tpi.Service
{
    public class OpportunityService
    {

        public static string ClassName = "OpportunityService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Opportunity QueryOpportunity(HttpClient httpClient, Guid opportunityId)
        {

            try
            {

                var odataQuery = webApiQueryUrl + "opportunities?$select=_pricelevelid_value,statecode,totalamount,discountamount&$filter=opportunityid eq " + opportunityId + "";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse =
                    JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                foreach (var data in systemUserObject.value)
                    return new Opportunity()
                    {
                        Id = Guid.Parse(data["opportunityid"].ToString()),
                        TotalAmount = (decimal)data["totalamount"],
                        TotalAmountDisplayName = data["totalamount@OData.Community.Display.V1.FormattedValue"],
                        PriceListId = Guid.Parse(data["_pricelevelid_value"].ToString()),
                        StateCode = (int)data["statecode"],
                        DiscountAmount = data["discountamount"] == null ? -1 : (decimal)data["discountamount"],
                        DiscountAmountDisplayName = data["discountamount@OData.Community.Display.V1.FormattedValue"] == null
                            ? ""
                            : data["discountamount@OData.Community.Display.V1.FormattedValue"],
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

        public static List<Opportunity> QueryOpportunitiesByPriceListsByCustomerIdByInspectionId(HttpClient httpClient, List<PriceList> priceLists, Guid customerId, Guid inspectionId)
        {

            try
            {

                var priceListIds = string.Empty;
                foreach (var priceList in priceLists)
                {
                    priceListIds += "<value>" + priceList.Id + "</value>";
                }

                var xmlQuery = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='opportunity'>
                                <attribute name='opportunityid' />
                                <attribute name='pricelevelid' />
                                <attribute name='customerid' />
                                <attribute name='totalamount' />
                                <attribute name='blu_opportunitytype' />
                                <filter type='and'>
                                  <condition attribute='statuscode' operator='eq' value='" + (int)OpportunityStatusCode.IncompleteChart + @"' />
                                  <condition attribute='customerid' operator='eq' value='" + customerId + @"' />
                                  <condition attribute='pricelevelid' operator='in'>
                                    " + priceListIds + @"
                                  </condition>
                                </filter>
                                <link-entity name='product' from='productid' to='productid' link-type='inner' alias='ah'>
                                  <link-entity name='blu_inspectiondetail' from='blu_productid' to='productid' link-type='outer' alias='ai'>
                                    <filter type='and'>
                                      <condition attribute='blu_inspectionid' operator='eq' value='" + inspectionId + @"' />
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";

                var encodedQuery = SharedService.UrlEncode(xmlQuery);

                var odataQuery = webApiQueryUrl + "productpricelevels?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse =
                    JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                var opportunities = new List<Opportunity>();
                foreach (var data in systemUserObject.value)
                {

                    var opportunity = new Opportunity()
                    {
                        Id = Guid.Parse(data["opportunityid"].ToString()),
                        Name = data["name"],
                        CustomerId = Guid.Parse(data["_customerid_value"].ToString()),
                        TotalAmount = (decimal)data["totalamount"],
                        TotalAmountDisplayName = data["totalamount@OData.Community.Display.V1.FormattedValue"],
                        PriceListId = Guid.Parse(data["_pricelevelid_value"].ToString()),
                        OpportunityType = (int)data["blu_opportunitytype"]
                    };
                    opportunities.Add(opportunity);

                }
                return opportunities;

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

        public static Guid CreateOpporutnity(HttpClient httpClient, Opportunity opportunity)
        {

            try
            {

                var opportunityJObject = new JObject()
                {
                    ["name"] = "Online Shopping",
                    ["blu_opportunitytype"] = (int)OpportunityType.OnlineShopping,
                    ["statuscode"] = (int)OpportunityStatusCode.IncompleteChart,
                    ["blu_createdfromportal"] = true,
                    ["pricelevelid@odata.bind"] = "/pricelevels(" + opportunity.PriceListId + ")"
                };

                if (opportunity.PropertyId != Guid.Empty)
                {
                    opportunityJObject["blu_RegardingPropertyId@odata.bind"] =
                        "/blu_inspections(" + opportunity.PropertyId + ")";
                }

                if (opportunity.ParentAccountId != Guid.Empty)
                {
                    opportunityJObject["customerid_account@odata.bind"] =
                        "/accounts(" + opportunity.CustomerId + ")";
                }

                if (opportunity.ParentContactId != Guid.Empty)
                {
                    opportunityJObject["customerid_contact@odata.bind"] =
                        "/contacts(" + opportunity.CustomerId + ")";
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "opportunities")
                {
                    Content = new StringContent(opportunityJObject.ToString(), Encoding.UTF8, "application/json")
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

        public static bool UpdateOpportunity(HttpClient httpClient, Opportunity opportunity)
        {

            try
            {

                var opportunityJObject = new JObject();

                if (opportunity.DiscountAmount > 0)
                {
                    if (opportunity.DiscountAmount != 0) opportunityJObject["discountamount"] = opportunity.DiscountAmount;
                }
                if (opportunity.VoucherId != Guid.Empty)
                {
                    opportunityJObject["blu_VoucherId@odata.bind"] = "/blu_vouchers(" + opportunity.VoucherId + ")";
                }
                if(opportunity.PropertyId != Guid.Empty)
                {
                    opportunityJObject["blu_RegardingPropertyId@odata.bind"] = "/blu_inspections(" + opportunity.PropertyId + ")";
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                    webApiQueryUrl + "opportunities(" + opportunity.Id + ")")
                {
                    Content = new StringContent(opportunityJObject.ToString(), Encoding.UTF8, "application/json")
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

        public static void SetOpportunityState(OrganizationServiceProxy service, Guid opportunityId)
        {

            try
            {

                var request = new WinOpportunityRequest();
                var opportunityClose = new Entity("opportunityclose");
                opportunityClose.Attributes.Add("opportunityid", new EntityReference("opportunity", opportunityId));
                request.OpportunityClose = opportunityClose;
                var oStatus = new OptionSetValue { Value = (int)OpportunityStatusCode.Ordered };
                request.Status = oStatus;
                var resp = (WinOpportunityResponse)service.Execute(request);

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

        public static Guid CreateOpportunityProduct(HttpClient httpClient, OpportunityProduct opportunityProduct)
        {

            try
            {

                var opportunityProductJObject = new JObject()
                {
                    ["opportunityid@odata.bind"] = "/opportunities(" + opportunityProduct.OpportunityId + ")",
                    ["productid@odata.bind"] = "/products(" + opportunityProduct.ProductId + ")",
                    ["uomid@odata.bind"] = "/uoms(" + opportunityProduct.UomId + ")",
                    ["quantity"] = 1,
                    ["ispriceoverridden"] = opportunityProduct.IsPriceOverriden,
                    ["blu_productcategory"] = opportunityProduct.ProductCategory,
                    ["blu_sellableto"] = opportunityProduct.SellableTo,
                    ["blu_onbackorder"] = opportunityProduct.OnBackOrder,
                    ["blu_freereport"] = opportunityProduct.FreeReport
                };

                if (opportunityProduct.ReportPriority != 0)
                {
                    opportunityProductJObject["blu_reportpriority"] = opportunityProduct.ReportPriority;
                }

                if (opportunityProduct.IsPriceOverriden)
                {
                    opportunityProductJObject["priceperunit"] = opportunityProduct.Amount;
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "opportunityproducts")
                {
                    Content = new StringContent(opportunityProductJObject.ToString(), Encoding.UTF8, "application/json")
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

        public static Guid ConvertOpportunityToSalesOrder(OrganizationServiceProxy service, Guid opportunityId)
        {

            try
            {

                var generateSalesOrderFromOpportunityRequest = new GenerateSalesOrderFromOpportunityRequest()
                {
                    ColumnSet = new ColumnSet(true),
                    OpportunityId = opportunityId
                };

                var generateSalesOrderFromOpportunityResponse =
                    (GenerateSalesOrderFromOpportunityResponse)service
                        .Execute(generateSalesOrderFromOpportunityRequest);

                if (generateSalesOrderFromOpportunityResponse?.Entity == null ||
                    generateSalesOrderFromOpportunityResponse.Entity.Id == Guid.Empty)
                {
                    return Guid.Empty;
                }

                var salesOrderId = generateSalesOrderFromOpportunityResponse.Entity.Id;

                return salesOrderId;

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

        public static List<CartItem> QueryOpportunityProductsByOpportunityId(HttpClient httpClient,
            Guid opportunityId)
        {

            try
            {

                var cartItems = new List<CartItem>();

                var fetchXml = string.Empty;
                fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXml += "  <entity name='opportunityproduct'>";
                fetchXml += "    <attribute name='productid' />";
                fetchXml += "    <attribute name='blu_productcategory' />";
                fetchXml += "    <attribute name='opportunityproductid' />";
                fetchXml += "    <attribute name='opportunityid' />";
                fetchXml += "    <attribute name='baseamount' />";
                fetchXml += "    <attribute name='blu_reportpriority' />";
                fetchXml += "    <attribute name='blu_freereport' />";
                fetchXml += "    <attribute name='blu_onbackorder' />";
                fetchXml += "    <attribute name='blu_reportisreleasedto' />";
                fetchXml += "    <attribute name='blu_reportisresellable' />";
                fetchXml += "    <attribute name='blu_sellableto' />";
                fetchXml += "    <attribute name='blu_stratareport' />";
                fetchXml += "    <filter type='and'>";
                fetchXml += "      <condition attribute='opportunityid' operator='eq' value='" + opportunityId + "' />";
                fetchXml += "      <condition attribute='producttypecode' operator='in'>";
                fetchXml += "        <value>2</value>";
                fetchXml += "        <value>1</value>";
                fetchXml += "      </condition>";
                fetchXml += "    </filter>";
                fetchXml += "    <link-entity name='product' from='productid' to='productid' visible='false' link-type='outer' alias='product'>";
                fetchXml += "      <attribute name='blu_requiredinspectorskills' />";
                fetchXml += "      <attribute name='blu_appointmentduration' />";
                fetchXml += "    </link-entity>";
                fetchXml += "  </entity>";
                fetchXml += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXml);

                var odataQuery = webApiQueryUrl + "opportunityproducts?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse =
                    JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;

                var opportunityProducts = new List<OpportunityProduct>();

                foreach (var data in systemUserObject.value)
                {

                    var cartItem = new CartItem()
                    {
                        OpportunityId = data["_opportunityid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_opportunityid_value"].ToString()),
                        ProductId = data["_productid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_productid_value"].ToString()),
                        ProductName = data["_productid_value@OData.Community.Display.V1.FormattedValue"] == null
                                ? ""
                                : data["_productid_value@OData.Community.Display.V1.FormattedValue"],
                        AmountText = data["baseamount@OData.Community.Display.V1.FormattedValue"] == null
                                ? ""
                                : data["baseamount@OData.Community.Display.V1.FormattedValue"],
                        ProductSkills = data["product_x002e_blu_requiredinspectorskills"] == null
                                    ? new string[0]
                                    : data["product_x002e_blu_requiredinspectorskills"].ToString().Split(','),
                        ProductCategory = data["blu_productcategory"] == null ? null : data["blu_productcategory"],
                        AppointmentDuration = data["product_x002e_blu_appointmentduration"] == null
                                    ? 0
                                    : (int)data["product_x002e_blu_appointmentduration"],
                        ReportPriority = data["blu_reportpriority"] == null
                            ? 0
                            : (int)data["blu_reportpriority"],
                        FreeReport = data["blu_freereport"] == null
                            ? false
                            : (bool)data["blu_freereport"],
                        OnBackOrder = data["blu_onbackorder"] == null
                            ? false
                            : (bool)data["blu_onbackorder"],
                        ReportIsReleasedTo = data["blu_reportisreleasedto"] == null
                            ? 0
                            : (int)data["blu_reportisreleasedto"],
                        ReportIsSellableTo = data["blu_reportisresellable"] == null
                            ? 0
                            : (int)data["blu_reportisresellable"],
                        SellableTo = data["blu_sellableto"] == null
                            ? 0
                            : (int)data["blu_sellableto"],
                        IsStrataReport = data["blu_stratareport"] == null
                            ? false
                            : (bool)data["blu_stratareport"],
                    };
                    cartItems.Add(cartItem);
                }

                return cartItems;

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
