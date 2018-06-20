using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class InspectionService
    {

        public static string EntityPluralName = "blu_inspections";
        public static string ClassName = "InspectionService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static List<Property> QueryInspectionByAddress(HttpClient httpClient, string searchText)
        {

            var inspections = new List<Property>();

            try
            {

                var odataQuery = webApiQueryUrl + "blu_inspections?" +
                                 "$select=blu_address&$filter=statecode eq 0 and contains(blu_address, '" + searchText + "')&$orderby=blu_address asc";
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                    inspections.Add(new Property()
                    {
                        Id = data.blu_inspectionid,
                        InspectionAddress = data.blu_address
                    });
                return inspections;

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

        public static List<Property> QueryInspectionByAddressParts(HttpClient httpClient, string searchText, string unitNumber,
            string streetNumber, string streetAddress, string subLocality, string suburb, string city, string state,
            string country, string postalCode)
        {

            var inspections = new List<Property>();
            var odataQuery = string.Empty;

            try
            {

                if (string.IsNullOrEmpty(streetAddress)
                    && string.IsNullOrEmpty(suburb) 
                    && string.IsNullOrEmpty(country))
                {
                    odataQuery = webApiQueryUrl + "blu_inspections" +
                                    "?$select=blu_address,blu_shipto_city,blu_shipto_country,blu_shipto_postalcode,blu_shipto_stateorprovince,blu_shipto_streetaddress,blu_shipto_streetnumber,blu_shipto_sub_locality,blu_shipto_suburb,blu_shipto_unitnumber" +
                                    "&$filter=contains(blu_address, '" + searchText + "')" +
                                    "&$orderby=blu_shipto_country asc,blu_shipto_stateorprovince asc,blu_shipto_city asc,blu_shipto_suburb asc,blu_shipto_sub_locality asc,blu_shipto_streetaddress asc,blu_shipto_streetnumber asc";
                }
                else if (!string.IsNullOrEmpty(streetNumber) && !string.IsNullOrEmpty(streetAddress) && !string.IsNullOrEmpty(suburb) && !string.IsNullOrEmpty(country))
                {
                    odataQuery = webApiQueryUrl + "blu_inspections" +
                                    "?$select=blu_address,blu_shipto_city,blu_shipto_country,blu_shipto_postalcode,blu_shipto_stateorprovince,blu_shipto_streetaddress,blu_shipto_streetnumber,blu_shipto_sub_locality,blu_shipto_suburb,blu_shipto_unitnumber" +
                                    "&$filter=(contains(blu_shipto_streetnumber, '" + streetNumber + "') and contains(blu_shipto_streetaddress, '" + streetAddress + "') and contains(blu_shipto_suburb, '" + suburb + "') and  blu_shipto_country eq '" + country + "')" +
                                    "&$orderby=blu_shipto_country asc,blu_shipto_stateorprovince asc,blu_shipto_city asc,blu_shipto_suburb asc,blu_shipto_sub_locality asc,blu_shipto_streetaddress asc,blu_shipto_streetnumber asc";
                }
                else
                {
                    odataQuery = webApiQueryUrl + "blu_inspections" +
                                     "?$select=blu_address,blu_shipto_city,blu_shipto_country,blu_shipto_postalcode,blu_shipto_stateorprovince,blu_shipto_streetaddress,blu_shipto_streetnumber,blu_shipto_sub_locality,blu_shipto_suburb,blu_shipto_unitnumber" +
                                     "&$filter=(contains(blu_shipto_streetaddress, '"+ streetAddress + "') and contains(blu_shipto_suburb, '"+ suburb+"') and  blu_shipto_country eq '"+ country + "')" +
                                     "&$orderby=blu_shipto_country asc,blu_shipto_stateorprovince asc,blu_shipto_city asc,blu_shipto_suburb asc,blu_shipto_sub_locality asc,blu_shipto_streetaddress asc,blu_shipto_streetnumber asc";
                }

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                    inspections.Add(new Property()
                    {
                        Id = data.blu_inspectionid,
                        InspectionAddress = data.blu_address,
                        UnitNumber = data.blu_shipto_unitnumber,
                        StreetNumber = data.blu_shipto_streetnumber,
                        StreetAddress = data.blu_shipto_streetaddress,
                        SubLocality = data.blu_shipto_sub_locality,
                        Suburb = data.blu_shipto_suburb,
                        City = data.blu_shipto_city,
                        State = data.blu_shipto_stateorprovince,
                        Country = data.blu_shipto_country,
                        PostalCode = data.blu_shipto_postalcode
                    });
                return inspections;

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

        public static Property QueryInspection(HttpClient httpClient, Guid inspectionId)
        {

            try
            {
                var odataQuery = webApiQueryUrl + "blu_inspections?" +
                                 "$select=blu_inspectionaddress&$filter=statecode eq 0 and  blu_inspectionid eq " + inspectionId;
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                    return new Property()
                    {
                        Id = data["blu_inspectionid"] == null ? Guid.Empty : Guid.Parse(data["blu_inspectionid"].ToString()),
                        InspectionAddress = data["blu_inspectionaddress"] == null ? "" : data["blu_inspectionaddress"].ToString()
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

        public static List<ProductInventorySale> QueryCustomerOrders(HttpClient httpClient, Guid customerId)
        {

            var productInventorySales = new List<ProductInventorySale>();

            try
            {

                //var fetchXML = string.Empty;
                //fetchXML += "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>";
                //fetchXML += "<entity name='blu_inspectiondetail'>";
                //fetchXML += "<attribute name='blu_inspectiondetailid'/>";
                //fetchXML += "<attribute name='blu_name'/>";
                //fetchXML += "<attribute name='statuscode'/>";
                //fetchXML += "<attribute name='blu_inspectionid'/>";
                //fetchXML += "<attribute name='blu_productid'/>";
                //fetchXML += "<order descending='false' attribute='blu_inspectionid'/>";
                //fetchXML += "<filter type='and'>";
                //fetchXML += "<condition attribute='blu_buyer' value='" + customerId + "' operator='eq'/>";
                //fetchXML += "</filter>";
                //fetchXML += "<link-entity name='blu_productinventory' alias='productinventory' link-type='outer' to='blu_productinventory' from='blu_productinventoryid'>";
                //fetchXML += "<attribute name='blu_downloadurl'/>";
                //fetchXML += "</link-entity>";
                //fetchXML += "</entity>";
                //fetchXML += "</fetch>";

                //var encodedQuery = SharedService.UrlEncode(fetchXML);

                var odataQuery = webApiQueryUrl + "blu_productinventorysales?$select=_blu_customer_value,blu_downloadurl,_blu_inspection_value,_blu_product_value,_blu_property_value&$filter=_blu_customer_value eq " + customerId;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {
                    //var inspectionDetail = new Inspection()
                    //{
                    //    Id = data["blu_productinventorysaleid"] == null ? Guid.Empty : Guid.Parse(data["blu_productinventorysaleid"].ToString()),
                    //    Name = data["blu_name"] == null ? "" : data["blu_name"].ToString(),
                    //    ReportUrl = data["productinventory_x002e_blu_downloadurl"] == null ? "" : data["productinventory_x002e_blu_downloadurl"].ToString(),
                    //    StatusCodeText = data["statuscode@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["statuscode@OData.Community.Display.V1.FormattedValue"].ToString(),
                    //    ProductName = data["_blu_productid_value@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["_blu_productid_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                    //    InspectionText = data["_blu_inspectionid_value@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["_blu_inspectionid_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                    //};
                    var productInventorySale = new ProductInventorySale()
                    {
                        Id = data["blu_productinventorysaleid"] == null ? Guid.Empty : Guid.Parse(data["blu_productinventorysaleid"].ToString()),
                        DownloadUrl = data["blu_downloadurl"] == null ? "" : data["blu_downloadurl"].ToString(),
                        ProductId = data["_blu_product_value"] == null ? Guid.Empty : Guid.Parse(data["_blu_product_value"].ToString()),
                        ProductName = data["_blu_product_value@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["_blu_product_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                        PropertyId = data["_blu_property_value"] == null ? Guid.Empty : Guid.Parse(data["_blu_property_value"].ToString()),
                        PropertyName = data["_blu_property_value@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["_blu_property_value@OData.Community.Display.V1.FormattedValue"].ToString(),
                        InspectionId = data["_blu_inspection_value"] == null ? Guid.Empty : Guid.Parse(data["_blu_inspection_value"].ToString()),
                        InspectionName = data["_blu_inspection_value@OData.Community.Display.V1.FormattedValue"] == null ? "" : data["_blu_inspection_value@OData.Community.Display.V1.FormattedValue"].ToString()
                    };
                    productInventorySales.Add(productInventorySale);

                }
                return productInventorySales;
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

        public static List<Inspection> QueryInspectionDetailsByStateCodeByInspection(HttpClient httpClient, int statecode, Guid inspectionId)
        {

            try
            {

                var inspectionDetailss = new List<Inspection>();
                var odataQuery = webApiQueryUrl + "blu_inspectiondetails?$" +
                                 "select=statecode,_blu_productid_value&$filter=_blu_inspectionid_value eq " +
                                 inspectionId + " and statecode eq " + statecode;
                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                    inspectionDetailss.Add(new Inspection()
                    {
                        Id = Guid.Parse(data["blu_inspectiondetailid"].ToString()),
                        StateCode = data["statecode"] == null ? -1 : (int)data["statecode"],
                        ProductId = data["_blu_productid_value"] == null ?
                            Guid.Empty :
                            Guid.Parse(data["_blu_productid_value"].ToString())
                    });
                return inspectionDetailss;

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

        public static Guid CreateProperty(HttpClient httpClient, Property inspection)
        {

            try
            {

                var inspectionObject = new JObject()
                {

                    ["blu_name"] = inspection.Name.Length > 450 ? inspection.Name.Substring(0, 450) : inspection.Name,
                    ["blu_address"] = inspection.Name.Length > 450 ? inspection.Name.Substring(0, 450) : inspection.Name,
                    ["blu_shipto_unitnumber"] = inspection.UnitNumber,
                    ["blu_shipto_streetnumber"] = inspection.StreetNumber,
                    ["blu_shipto_streetaddress"] = inspection.StreetAddress,
                    ["blu_shipto_sub_locality"] = inspection.SubLocality,
                    ["blu_shipto_suburb"] = inspection.Suburb,
                    ["blu_shipto_city"] = inspection.City,
                    ["blu_shipto_stateorprovince"] = inspection.State,
                    ["blu_shipto_country"] = inspection.Country,
                    ["blu_shipto_postalcode"] = inspection.PostalCode,
                    ["blu_createdfromportal"] = true
                };

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "" + EntityPluralName)
                {
                    Content = new StringContent(inspectionObject.ToString(), Encoding.UTF8, "application/json")
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

        public static Guid CreateInspection(HttpClient httpClient, Inspection inspectionDetail)
        {

            try
            {
                var newInspectionDetailObject = new JObject()
                {
                    ["blu_name"] = inspectionDetail.Name.Length > 450 ? inspectionDetail.Name.Substring(0, 450) : inspectionDetail.Name,
                    //["blu_productcategory"] = inspectionDetail.ProductCategory,
                    ["blu_createdfromportal"] = true,
                    ["blu_freereport"] = inspectionDetail.FreeReport,
                    ["blu_onbackorder"] = inspectionDetail.OnBackOrder,
                    ["blu_stratareport"] = inspectionDetail.StrataReport,
                    ["blu_appointmentstarttime"] = inspectionDetail.AppointmentStartTime,
                    ["statuscode"] = inspectionDetail.StatusCode
                };

                if(inspectionDetail.OwnerId != Guid.Empty)
                {
                    newInspectionDetailObject["ownerid@odata.bind"] = "/systemusers(" + inspectionDetail.OwnerId + ")";
                }
                if(inspectionDetail.OrderId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_OrderId@odata.bind"] = "/salesorders(" + inspectionDetail.OrderId + ")";
                }
                if(inspectionDetail.ProductId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_ProductId@odata.bind"] = "/products(" + inspectionDetail.ProductId + ")";
                }
                if(inspectionDetail.PropertyId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_InspectionId@odata.bind"] = "/blu_inspections(" + inspectionDetail.PropertyId + ")";
                }
                if(inspectionDetail.AppointmentId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_AppointmentId@odata.bind"] = "/appointments(" + inspectionDetail.AppointmentId + ")";
                }
                if(inspectionDetail.ReportIsReleasedTo != 0)
                {
                    newInspectionDetailObject["blu_reportisreleasedto"] = inspectionDetail.ReportIsReleasedTo;
                }
                if (inspectionDetail.ReportIsReleasedTo != 0)
                {
                    newInspectionDetailObject["blu_reportisresellable"] = inspectionDetail.ReportIsReleasedTo;
                }
                if (inspectionDetail.ReportIsReleasedTo != 0)
                {
                    newInspectionDetailObject["blu_sellableto"] = inspectionDetail.ReportIsReleasedTo;
                }
                if (inspectionDetail.ReportPriority == (int)ReportPriority.Urgent)
                {
                    newInspectionDetailObject["blu_reportpriority"] = (int)ReportPriority.Urgent;
                }
                if (inspectionDetail.ContactId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_Buyer_contact@odata.bind"] = "/contacts(" + inspectionDetail.ContactId + ")";
                }
                else if (inspectionDetail.AccountId != Guid.Empty)
                {
                    newInspectionDetailObject["blu_Buyer_account@odata.bind"] = "/accounts(" + inspectionDetail.AccountId + ")";
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + "blu_inspectiondetails")
                {
                    Content = new StringContent(newInspectionDetailObject.ToString(), Encoding.UTF8, "application/json")
                };
                var response = httpClient.SendAsync(request);
                if (!response.Result.IsSuccessStatusCode)
                {
                    var error = response.Result.Content.ReadAsStringAsync();
                    LogService.LogMessage(httpClient, new Log()
                    {
                        Level = (int)LogType.Error,
                        Name = "Create Inspection Error",
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
