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
    public class ProductService
    {

        public static string ClassName = "ProductService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static List<Product> QueryProductsByInspection(HttpClient httpClient, Guid inspectionId)
        {

            var products = new List<Product>();

            var today = DateTime.Today;

            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
            fetchXml += "  <entity name='product'>";
            fetchXml += "    <attribute name='name' />";
            fetchXml += "    <attribute name='productid' />";
            fetchXml += "    <attribute name='parentproductid' />";
            fetchXml += "    <attribute name='price' />";
            fetchXml += "    <attribute name='defaultuomid' />";
            fetchXml += "    <attribute name='blu_buyerpays' />";
            fetchXml += "    <attribute name='statecode' />";
            fetchXml += "    <attribute name='blu_appointmentduration' />";
            fetchXml += "    <attribute name='blu_requiredinspectorskills' />";
            fetchXml +=
                "    <link-entity name='blu_inspectiondetail' from='blu_productid' to='productid' link-type='inner' alias='ae'>";
            fetchXml += "      <filter type='and'>";
            fetchXml += "        <condition attribute='blu_reportvalidfrom' operator='on-or-before' value='" + today +
                        "' />";
            fetchXml += "        <condition attribute='blu_reportvalidto' operator='on-or-after' value='" + today +
                        "' />";
            fetchXml += "      </filter>";
            fetchXml +=
                "      <link-entity name='blu_inspection' from='blu_inspectionid' to='blu_inspectionid' link-type='inner' alias='af'>";
            fetchXml += "        <filter type='and'>";
            fetchXml += "          <condition attribute='statecode' operator='eq' value='0' />";
            fetchXml += "          <condition attribute='blu_inspectionid' operator='eq' value='" + inspectionId +
                        "' />";
            fetchXml += "        </filter>";
            fetchXml += "      </link-entity>";
            fetchXml += "    </link-entity>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "products?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;
            if (systemUserObject.value.Count == 0) return null;

            foreach (var data in systemUserObject.value)
            {

                var parentProduct = new Product()
                {
                    Id = data["_parentproductid_value"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["_parentproductid_value"].ToString()),
                    Name = data["_parentproductid_value@OData.Community.Display.V1.FormattedValue"] == null
                        ? ""
                        : data["_parentproductid_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                    IsParentProduct = true
                };

                var product = new Product()
                {
                    Id = data["productid"] == null ? Guid.Empty : Guid.Parse(data["productid"].ToString()),
                    Name = data["name"] == null
                        ? ""
                        : data["name"].ToString(),
                    BuyerPays = data["blu_buyerpays"] == null
                        ? 0
                        : (decimal) data["blu_buyerpays"],
                    BuyerPaysDisplayName =
                        data["blu_buyerpays@OData.Community.Display.V1.FormattedValue"] == null
                            ? ""
                            : data["blu_buyerpays@OData.Community.Display.V1.FormattedValue"].ToString(),
                    UomId = data["_defaultuomid_value"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["_defaultuomid_value"].ToString()),
                    Status = data["statecode"] == null
                        ? 0
                        : (int) data["statecode"],
                    InspectorSkills = data["blu_requiredinspectorskills"] == null
                        ? new List<string>()
                        : data["blu_requiredinspectorskills"].ToString().Split(','),
                    AppointmentDuration = data["blu_appointmentduration"] == null? 0 : (int)data["blu_appointmentduration"],
                    ParentProductId = parentProduct.Id,
                    ParentProduct = parentProduct,
                };

                products.Add(product);

            }

            return products;

        }

        public static List<Product> QueryProductsByPriceLists(HttpClient httpClient,
            List<PriceList> priceLists)
        {

            var products = new List<Product>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='product'>";
            fetchXml += "    <attribute name='name' />";
            fetchXml += "    <attribute name='productid' />";
            fetchXml += "    <attribute name='productnumber' />";
            fetchXml += "    <attribute name='description' />";
            fetchXml += "    <attribute name='statecode' />";
            fetchXml += "    <attribute name='productstructure' />";
            fetchXml += "    <attribute name='parentproductid' />";
            fetchXml += "    <attribute name='pricelevelid' />";
            fetchXml += "    <attribute name='blu_stratareport' />";
            fetchXml += "    <filter type='and'>";
            if (priceLists != null && priceLists.Count > 0)
            {
                fetchXml += "      <condition attribute='pricelevelid' operator='in'>";
                foreach (var priceList in priceLists)
                {
                    fetchXml += "        <value>" + priceList.Id + "</value>";
                }

                fetchXml += "      </condition>";
            }

            fetchXml += "      <condition attribute='statecode' operator='eq' value='0' />";
            fetchXml += "      <condition attribute='statuscode' operator='eq' value='1' />";
            fetchXml += "    </filter>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";


            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "products?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;
            if (systemUserObject.value.Count == 0) return null;

            foreach (var data in systemUserObject.value)
            {

                var priceList = new PriceList()
                {
                    Id = data["_pricelevelid_value"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["_pricelevelid_value"].ToString()),
                    Name = data["_pricelevelid_value@OData.Community.Display.V1.FormattedValue"] == null
                        ? ""
                        : data["_pricelevelid_value@OData.Community.Display.V1.FormattedValue"].ToString()
                };

                var parentProduct = new Product()
                {
                    Id = data["_parentproductid_value"] == null
                        ? Guid.Empty
                        : Guid.Parse(data["_parentproductid_value"].ToString()),
                    Name = data["_parentproductid_value@OData.Community.Display.V1.FormattedValuee"] == null
                        ? ""
                        : data["_parentproductid_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                    IsParentProduct = true
                };

                var product = new Product()
                {
                    Id = data["productid"] == null ? Guid.Empty : Guid.Parse(data["productid"].ToString()),
                    Name = data["name"] == null
                        ? ""
                        : data["name"].ToString(),
                    StrataReport = data["blu_stratareport"] != null && (bool) data["blu_stratareport"],
                    ParentProductId = parentProduct.Id,
                    ParentProduct = parentProduct,
                    DefaultPriceListId = priceList.Id,
                    DefaultPriceList = priceList
                };

                products.Add(product);

            }

            return products;

        }

        public static Product QueryProduct(HttpClient httpClient, Guid productId)
        {

            try
            {

                var odataQuery =
                    webApiQueryUrl + "products(" + productId + ")?$select=productid,_blu_stockingproduct_value,name,_defaultuomid_value";

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse =
                    JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic data = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                var product = new Product()
                {
                    Id = Guid.Parse(data["productid"].ToString()),
                    Name = data["name"] == null ? "" : data["name"].ToString(),
                    StockingProductId = data["_blu_stockingproduct_value"] == null ? Guid.Empty : Guid.Parse(data["_blu_stockingproduct_value"].ToString()),
                    UomId = data["_defaultuomid_value"] == null ? Guid.Empty : Guid.Parse(data["_defaultuomid_value"].ToString()),
                };
                return product;

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
