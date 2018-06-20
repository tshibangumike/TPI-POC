using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using tpi.Service;

namespace tpi.Model
{
    public class CartItemService
    {

        public static string EntityPluralName = "products";
        public static string ClassName = "CartItemService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static List<CartItem> QueryCartItemByInspection(HttpClient httpClient, Guid inspectionId, List<PriceList> priceLists)
        {

            try
            {

                var cartItems = new List<CartItem>();

                if (priceLists == null || priceLists.Count == 0)
                {
                    priceLists = PriceListService.QueryPriceListByPortalUserRole(httpClient, (int)PortalUserrole.Consumer);
                }

                var fetchXml = string.Empty;
                fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
                fetchXml += "  <entity name='product'>";
                fetchXml += "    <attribute name='name' />";
                fetchXml += "    <attribute name='productid' />";
                fetchXml += "    <attribute name='defaultuomid' />";
                fetchXml += "    <attribute name='blu_buyerpays' />";
                fetchXml += "    <attribute name='parentproductid' />";
                fetchXml += "    <attribute name='blu_appointmentduration' />";
                fetchXml += "    <attribute name='blu_requiredinspectorskills' />";
                fetchXml += "    <attribute name='blu_sellableto' />";
                fetchXml += "    <attribute name='blu_reportisreleasedto' />";
                fetchXml += "    <attribute name='blu_finalbuyerpays' />";
                fetchXml += "    <attribute name='blu_freereport' />";
                fetchXml += "    <attribute name='blu_reportisreleasedto' />";
                fetchXml += "    <filter type='and'>";
                fetchXml += "          <condition attribute='blu_reportisresellable' operator='eq' value='1' />";
                if (priceLists != null && priceLists.Count > 0)
                {
                    
                    fetchXml += "      <condition attribute='pricelevelid' operator='in'>";
                    foreach (var priceList in priceLists)
                    {
                        fetchXml += "        <value>" + priceList.Id + "</value>";
                    }
                    fetchXml += "      </condition>";
                }
                fetchXml += "    </filter>";
                fetchXml += "    <link-entity name='blu_inspectiondetail' from='blu_productid' to='productid' link-type='inner' alias='ae'>";
                fetchXml += "      <filter type='and'>";
                fetchXml += "        <condition attribute='statecode' operator='eq' value='0' />";
                fetchXml += "      </filter>";
                fetchXml += "      <link-entity name='blu_inspection' from='blu_inspectionid' to='blu_inspectionid' link-type='inner' alias='af'>";
                fetchXml += "        <filter type='and'>";
                fetchXml += "          <condition attribute='statecode' operator='eq' value='0' />";
                fetchXml += "          <condition attribute='blu_inspectionid' operator='eq' value='" + inspectionId + "' />";
                fetchXml += "        </filter>";
                fetchXml += "      </link-entity>";
                fetchXml += "    </link-entity>";
                fetchXml += "    <link-entity name='product' from='productid' to='blu_stockingproduct' link-type='inner' alias='ag'>";
                fetchXml += "      <link-entity name='blu_productinventory' from='blu_reporttype' to='productid' link-type='inner' alias='ah'>";
                fetchXml += "        <filter type='and'>";
                fetchXml += "          <condition attribute='blu_property' operator='eq' value='" + inspectionId + "' />";
                fetchXml += "        </filter>";
                fetchXml += "      </link-entity>";
                fetchXml += "    </link-entity>";
                fetchXml += "  </entity>";
                fetchXml += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXml);

                var odataQuery = webApiQueryUrl + EntityPluralName + "?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;

                foreach (var data in systemUserObject.value)
                {
                    var cartItem = new CartItem()
                    {

                        ProductId = data["productid"] == null ? Guid.Empty : Guid.Parse(data["productid"].ToString()),
                        ProductName = data["name"] == null
                            ? ""
                            : data["name"].ToString(),
                        ParentProductName = data["_parentproductid_value@OData.Community.Display.V1.FormattedValue"] ?? "",
                        Amount =
                            data["blu_buyerpays"] == null
                                ? 0
                                : (decimal)data["blu_buyerpays"],
                        AmountText =
                            data["blu_buyerpays@OData.Community.Display.V1.FormattedValue"] ?? "",
                        UomId = data["_defaultuomid_value"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["_defaultuomid_value"].ToString()),
                        InspectionDetailStateCode = data["ae_x002e_statecode"] == null
                            ? 0
                            : (int)data["ae_x002e_statecode"],
                        InspectionDetailStatusCode = data["ae_x002e_statuscode"] == null
                            ? 0
                            : (int)data["ae_x002e_statuscode"],
                        ProductSkills = data["blu_requiredinspectorskills"] == null
                            ? new string[0]
                            : data["blu_requiredinspectorskills"].ToString().Split(','),
                        AppointmentDuration = data["blu_appointmentduration"] == null ? 0 : (int)data["blu_appointmentduration"],
                        IsPriceOverriden = true,
                        ProductCategory = true,
                        SellableTo = data["blu_sellableto"] == null
                            ? 0
                            : (int)data["blu_sellableto"],
                        BuyerPays =
                            data["blu_buyerpays"] == null
                                ? 0
                                : (decimal)data["blu_buyerpays"],
                        BuyerPaysText =
                            data["blu_buyerpays@OData.Community.Display.V1.FormattedValue"] ?? "",
                        FinalBuyerPays =
                            data["blu_finalbuyerpays"] == null
                                ? 0
                                : (decimal)data["blu_finalbuyerpays"],
                        FinalBuyerPaysText =
                            data["blu_finalbuyerpays@OData.Community.Display.V1.FormattedValue"] ?? "",
                        ReportIsReleasedTo = data["blu_reportisreleasedto"] == null
                            ? 0
                            : (int)data["blu_reportisreleasedto"],
                        ReportIsReleasedToText = data["blu_reportisreleasedto@OData.Community.Display.V1.FormattedValue"] == null
                            ? ""
                            : data["blu_reportisreleasedto@OData.Community.Display.V1.FormattedValue"],
                        Conditions = data["blu_conditions"] == null
                            ? ""
                            : data["blu_conditions"],
                        FreeReport = data["blu_freereport"] != null && (bool)data["blu_freereport"],
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

        public static List<CartItem> QueryPriceListItemsByPriceLists(HttpClient httpClient, List<PriceList> priceLists)
        {

            try
            {

                var cartItems = new List<CartItem>();

                if (priceLists == null || priceLists.Count == 0)
                {
                    priceLists = PriceListService.QueryPriceListByPortalUserRole(httpClient, (int)PortalUserrole.Consumer);
                }

                var fetchXml = string.Empty;
                fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXml += "  <entity name='productpricelevel'>";
                fetchXml += "    <attribute name='amount' />";
                fetchXml += "    <attribute name='uomid' />";
                if (priceLists != null && priceLists.Count > 0)
                {
                    fetchXml += "    <filter type='and'>";
                    fetchXml += "      <condition attribute='pricelevelid' operator='in'>";
                    foreach (var priceList in priceLists)
                    {
                        fetchXml += "        <value>" + priceList.Id + "</value>";
                    }

                    fetchXml += "      </condition>";
                    fetchXml += "    </filter>";
                }
                else
                {
                    fetchXml += "    <filter type='and'>";
                    fetchXml += "      <condition attribute='pricelevelid' operator='in'>";

                    var consumerPriceLists = PriceListService.QueryPriceListByPortalUserRole(httpClient, (int)PortalUserrole.Consumer);
                    foreach (var priceList in consumerPriceLists)
                    {
                        fetchXml += "        <value>" + priceList.Id + "</value>";
                    }

                    fetchXml += "      </condition>";
                    fetchXml += "    </filter>";
                }

                fetchXml +=
                    "    <link-entity name='pricelevel' from='pricelevelid' to='pricelevelid' visible='false' link-type='outer' alias='pricelevel'>";
                fetchXml += "      <attribute name='pricelevelid' />";
                fetchXml += "      <attribute name='name' />";
                fetchXml += "      <attribute name='blu_reportpriority' />";
                fetchXml += "    </link-entity>";
                fetchXml +=
                    "    <link-entity name='product' from='productid' to='productid' link-type='outer' alias='product'>";
                fetchXml += "      <attribute name='productid' />";
                fetchXml += "      <attribute name='name' />";
                fetchXml += "      <attribute name='blu_stratareport' />";
                fetchXml += "      <attribute name='parentproductid' />";
                fetchXml += "      <attribute name='defaultuomid' />";
                fetchXml += "      <attribute name='blu_appointmentduration' />";
                fetchXml += "      <attribute name='blu_sellableto' />";
                fetchXml += "      <attribute name='blu_finalbuyerpays' />";
                fetchXml += "      <attribute name='blu_buyerpays' />";
                fetchXml += "      <attribute name='blu_reportisreleasedto' />";
                fetchXml += "      <attribute name='blu_conditions' />";
                fetchXml += "      <attribute name='blu_freereport' />";
                fetchXml += "      <attribute name='blu_reportisreleasedto' />";
                fetchXml += "      <filter type='and'>";
                fetchXml += "        <condition attribute='statuscode' operator='eq' value='1' />";
                fetchXml += "        <condition attribute='statecode' operator='eq' value='0' />";
                fetchXml += "        <condition attribute='blu_reportisresellable' operator='eq' value='1' />";
                fetchXml += "      </filter>";
                fetchXml += "    </link-entity>";
                fetchXml += "  </entity>";
                fetchXml += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXml);

                var odataQuery = webApiQueryUrl + "productpricelevels?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                if (systemUserObject == null || systemUserObject.value == null) return null;
                if (systemUserObject.value.Count == 0) return null;

                foreach (var data in systemUserObject.value)
                {

                    var cartItem = new CartItem()
                    {

                        ProductId = data["product_x002e_productid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["product_x002e_productid"].ToString()),

                        ProductName = data["product_x002e_name"] ?? "",
                        ParentProductId = data["product_x002e_parentproductid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["product_x002e_parentproductid"].ToString()),
                        ParentProductName = data["product_x002e_parentproductid@OData.Community.Display.V1.FormattedValue"] ?? "",
                        IsStrataReport = data["_blu_stratareport_value"] != null && (bool)data["_blu_stratareport_value"],
                        AppointmentDuration = data["product_x002e_blu_appointmentduration"] == null ? 0 : (int)data["product_x002e_blu_appointmentduration"],
                        PriceListId = data["pricelevel_x002e_pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevel_x002e_pricelevelid"].ToString()),
                        PriceListItemId = data["pricelevel_x002e_pricelevelid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["pricelevel_x002e_pricelevelid"].ToString()),
                        ReportPriority = data["pricelevel_x002e_blu_reportpriority"] == null
                            ? 0
                            : (int)data["pricelevel_x002e_blu_reportpriority"],
                        UomId = data["product_x002e_defaultuomid"] == null ? Guid.Empty : Guid.Parse(data["product_x002e_defaultuomid"].ToString()),
                        Amount = data["amount"] == null
                            ? 0
                            : (decimal)data["amount"],
                        AmountText = data["amount@OData.Community.Display.V1.FormattedValue"] ?? "",
                        IsPriceOverriden = false,
                        ProductCategory = false,
                        SellableTo = data["product_x002e_blu_sellableto"] == null
                            ? 0
                            : (int)data["product_x002e_blu_sellableto"],
                        FinalBuyerPays =
                            data["product_x002e_blu_finalbuyerpays"] == null
                                ? 0
                                : (decimal)data["product_x002e_blu_finalbuyerpays"],
                        FinalBuyerPaysText =
                            data["product_x002e_blu_finalbuyerpays@OData.Community.Display.V1.FormattedValue"] ?? "",
                        BuyerPays =
                            data["product_x002e_blu_buyerpays"] == null
                                ? 0
                                : (decimal)data["product_x002e_blu_buyerpays"],
                        BuyerPaysText =
                            data["product_x002e_blu_buyerpays@OData.Community.Display.V1.FormattedValue"] ?? "",
                        ReportIsReleasedTo = data["product_x002e_blu_reportisreleasedto"] == null
                            ? 0
                            : (int)data["product_x002e_blu_reportisreleasedto"],
                        ReportIsReleasedToText = data["product_x002e_blu_reportisreleasedto@OData.Community.Display.V1.FormattedValue"] == null
                            ? ""
                            : data["product_x002e_blu_reportisreleasedto@OData.Community.Display.V1.FormattedValue"],
                        Conditions = data["product_x002e_blu_conditions"] == null
                            ? ""
                            : data["product_x002e_blu_conditions"],
                        FreeReport = data["product_x002e_blu_freereport"] != null && (bool)data["product_x002e_blu_freereport"]
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
