using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using tpi.Model;

namespace tpi.Service
{
    public class QuestionSetupService
    {

        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static List<InspectionPortalQA> QueryQuestionSetupByQuestionSetup(HttpClient httpClient, Guid productId)
        {

            var questionSetups = new List<InspectionPortalQA>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "    <entity name='blu_blu_questionsetup_product' >";
            fetchXml += "        <attribute name='blu_questionsetupid' />";
            fetchXml += "        <attribute name='productid' />";
            fetchXml += "        <link-entity name='product' from='productid' to='productid' linke-type='inner' >";
            fetchXml += "            <attribute name='name' />";
            fetchXml += "            <attribute name='parentproductid' />";
            fetchXml += "            <filter>";
            fetchXml += "               <condition attribute='blu_requiredfor' operator='eq' value='" + productId + "' />";
            fetchXml += "            <filter>";
            fetchXml += "        </link-entity>";
            fetchXml += "        <link-entity name='blu_questionsetup' from='blu_questionsetupid' to='blu_questionsetupid' linke-type='inner' >";
            fetchXml += "           <attribute name='blu_questionsetupid' />";
            fetchXml += "           <attribute name='blu_name' />";
            fetchXml += "           <attribute name='blu_number' />";
            fetchXml += "           <attribute name='blu_type' />";
            fetchXml += "           <attribute name='blu_answerdatatype' />";
            fetchXml += "           <attribute name='blu_optionvalues' />";
            fetchXml += "           <attribute name='blu_uniquename' />";
            
            fetchXml += "        </link-entity>";
            fetchXml += "    </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "blu_blu_questionsetup_productset?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;
            if (systemUserObject.value.Count == 0) return null;

            foreach (var data in systemUserObject.value)
            {

                var questionSetup = new InspectionPortalQA()
                {

                    Id = data["blu_questionsetupid"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetupid"].ToString()),
                    AnswerDataType = data["blu_questionsetup2_x002e_blu_answerdatatype"] == null ?
                                    0 : (int)data["blu_questionsetup2_x002e_blu_answerdatatype"],
                    AnswerDataTypeText = data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"] == null ?
                    "" : data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"].ToString(),
                    Name = data["blu_questionsetup2_x002e_blu_name"] == null ? "" : data["blu_questionsetup2_x002e_blu_name"].ToString(),
                    UniqueQuestionName = data["blu_questionsetup2_x002e_blu_uniquename"] == null ? "" : data["blu_questionsetup2_x002e_blu_uniquename"].ToString(),
                    Number = data["blu_questionsetup2_x002e_blu_number"] == null ?
                                    -99 : (int)data["blu_questionsetup2_x002e_blu_number"],
                    OptionSetValues = data["blu_questionsetup2_x002e_blu_optionvalues"] == null
                        ? new string[0]
                        : data["blu_questionsetup2_x002e_blu_optionvalues"].ToString().Split(','),
                    Type = data["blu_questionsetup2_x002e_blu_type"] == null ?
                                    0 : (int)data["blu_questionsetup2_x002e_blu_type"],

                    //Id = data["blu_questionsetup2_x002e_blu_questionsetupid"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetup2_x002e_blu_questionsetupid"].ToString()),

                    //AnswerDataType = data["blu_questionsetup2_x002e_blu_answerdatatype"] == null ?
                    //                0 : (int)data["blu_questionsetup2_x002e_blu_answerdatatype"],

                    //AnswerDataTypeText = data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"] == null ?
                    //"" : data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"].ToString(),

                    //Name = data["blu_questionsetup2_x002e_blu_name"] == null ? "" : data["blu_questionsetup2_x002e_blu_name"].ToString(),
                    //UniqueQuestionName = data["blu_questionsetup2_x002e_blu_uniquename"] == null ? "" : data["blu_questionsetup2_x002e_blu_uniquename"].ToString(),
                    //Number = data["blu_questionsetup2_x002e_blu_number"] == null ?
                    //                -99 : (int)data["blu_questionsetup2_x002e_blu_number"],
                    //OptionSetValues = data["blu_questionsetup2_x002e_blu_optionvalues"] == null
                    //    ? new string[0]
                    //    : data["blu_questionsetup2_x002e_blu_optionvalues"].ToString().Split(','),
                    //Type = data["blu_questionsetup2_x002e_blu_type"] == null ?
                    //                0 : (int)data["blu_questionsetup2_x002e_blu_type"],
                };
                questionSetup.UniqueName = Guid.NewGuid().ToString().Replace("-", "");
                questionSetups.Add(questionSetup);

            }

            return questionSetups;

        }

        public static List<InspectionPortalQA> QueryQuestionSetupByQuestionSetup(HttpClient httpClient, List<Guid> productIds, Guid priceListId)
        {

            var stockingProductIds = new List<Guid>();

            foreach (var productId in productIds)
            {
                var product = ProductService.QueryProduct(httpClient, productId);
                if (product.StockingProductId != Guid.Empty && !stockingProductIds.Contains(product.StockingProductId))
                    stockingProductIds.Add(product.StockingProductId);
            }

            var questionSetups = new List<InspectionPortalQA>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "    <entity name='blu_blu_questionsetup_product' >";
            fetchXml += "        <attribute name='blu_questionsetupid' />";
            fetchXml += "        <attribute name='productid' />";
            fetchXml += "        <link-entity name='product' from='productid' to='productid' linke-type='inner' >";
            fetchXml += "            <attribute name='name' />";
            fetchXml += "            <attribute name='parentproductid' />";
            fetchXml += "            <filter>";
            if (stockingProductIds != null && stockingProductIds.Count > 0)
            {
                fetchXml += "      <condition attribute='productid' operator='in'>";
                foreach (var stockingProductId in stockingProductIds)
                {
                    fetchXml += "        <value>" + stockingProductId + "</value>";
                }
                fetchXml += "      </condition>";
            }
            fetchXml += "            </filter>";
            fetchXml += "        </link-entity>";
            fetchXml += "        <link-entity name='blu_questionsetup' from='blu_questionsetupid' to='blu_questionsetupid' linke-type='inner' >";
            fetchXml += "           <attribute name='blu_questionsetupid' />";
            fetchXml += "           <attribute name='blu_name' />";
            fetchXml += "           <attribute name='blu_number' />";
            fetchXml += "           <attribute name='blu_type' />";
            fetchXml += "           <attribute name='blu_answerdatatype' />";
            fetchXml += "           <attribute name='blu_optionvalues' />";
            fetchXml += "           <attribute name='blu_uniquename' />";
            fetchXml += "           <attribute name='blu_answerismandatory' />";
            fetchXml += "           <attribute name='blu_additionalproduct' />";

            fetchXml += "        </link-entity>";
            fetchXml += "    </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "blu_blu_questionsetup_productset?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject != null && systemUserObject.value != null)
            {
                if (systemUserObject.value.Count != 0)
                {

                    foreach (var data in systemUserObject.value)
                    {

                        var questionSetup = new InspectionPortalQA()
                        {

                            Id = data["blu_questionsetupid"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetupid"].ToString()),
                            AnswerDataType = data["blu_questionsetup2_x002e_blu_answerdatatype"] == null ?
                                            0 : (int)data["blu_questionsetup2_x002e_blu_answerdatatype"],
                            AnswerDataTypeText = data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"] == null ?
                            "" : data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"].ToString(),
                            Name = data["blu_questionsetup2_x002e_blu_name"] == null ? "" : data["blu_questionsetup2_x002e_blu_name"].ToString(),
                            UniqueQuestionName = data["blu_questionsetup2_x002e_blu_uniquename"] == null ? "" : data["blu_questionsetup2_x002e_blu_uniquename"].ToString(),
                            Number = data["blu_questionsetup2_x002e_blu_number"] == null ?
                                            -99 : (int)data["blu_questionsetup2_x002e_blu_number"],
                            OptionSetValues = data["blu_questionsetup2_x002e_blu_optionvalues"] == null
                                ? new string[0]
                                : data["blu_questionsetup2_x002e_blu_optionvalues"].ToString().Split(','),
                            Type = data["blu_questionsetup2_x002e_blu_type"] == null ?
                                            0 : (int)data["blu_questionsetup2_x002e_blu_type"],
                            IsMandatory = data["blu_questionsetup2_x002e_blu_answerismandatory"] == null ?
                                    false : (bool)data["blu_questionsetup2_x002e_blu_answerismandatory"],
                            AdditionalProductId = data["blu_questionsetup2_x002e_blu_additionalproduct"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetup2_x002e_blu_additionalproduct"].ToString()),
                        };
                        questionSetup.UniqueName = Guid.NewGuid().ToString().Replace("-", "");
                        questionSetups.Add(questionSetup);

                    }
                }
            }

            fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "  <entity name='blu_blu_questionsetup_pricelevel' >";
            fetchXml += "    <filter type='and' >";
            fetchXml += "      <condition attribute='pricelevelid' operator='eq' value='"+ priceListId + "' />";
            fetchXml += "    </filter>";
            fetchXml += "    <link-entity name='blu_questionsetup' from='blu_questionsetupid' to='blu_questionsetupid' link-type='inner'  >";
            fetchXml += "           <attribute name='blu_questionsetupid' />";
            fetchXml += "           <attribute name='blu_name' />";
            fetchXml += "           <attribute name='blu_number' />";
            fetchXml += "           <attribute name='blu_type' />";
            fetchXml += "           <attribute name='blu_answerdatatype' />";
            fetchXml += "           <attribute name='blu_optionvalues' />";
            fetchXml += "           <attribute name='blu_uniquename' />";
            fetchXml += "           <attribute name='blu_answerismandatory' />";
            fetchXml += "           <attribute name='blu_additionalproduct' />";
            fetchXml += "    </link-entity>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            encodedQuery = SharedService.UrlEncode(fetchXml);

            odataQuery = webApiQueryUrl + "blu_blu_questionsetup_pricelevelset?fetchXml=" +
                             encodedQuery;

            retrieveResponse = httpClient.GetAsync(odataQuery);
            jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return questionSetups;
            if (systemUserObject.value.Count == 0) return questionSetups;

            foreach (var data in systemUserObject.value)
            {

                var questionSetup = new InspectionPortalQA()
                {
                    Id = data["blu_questionsetupid"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetupid"].ToString()),
                    AnswerDataType = data["blu_questionsetup1_x002e_blu_answerdatatype"] == null ?
                                    0 : (int)data["blu_questionsetup1_x002e_blu_answerdatatype"],
                    AnswerDataTypeText = data["blu_questionsetup1_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"] == null ?
                    "" : data["blu_questionsetup1_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"].ToString(),
                    Name = data["blu_questionsetup1_x002e_blu_name"] == null ? "" : data["blu_questionsetup1_x002e_blu_name"].ToString(),
                    UniqueQuestionName = data["blu_questionsetup1_x002e_blu_uniquename"] == null ? "" : data["blu_questionsetup1_x002e_blu_uniquename"].ToString(),
                    Number = data["blu_questionsetup1_x002e_blu_number"] == null ?
                                    -99 : (int)data["blu_questionsetup1_x002e_blu_number"],
                    OptionSetValues = data["blu_questionsetup1_x002e_blu_optionvalues"] == null
                        ? new string[0]
                        : data["blu_questionsetup1_x002e_blu_optionvalues"].ToString().Split(','),
                    Type = data["blu_questionsetup1_x002e_blu_type"] == null ?
                                    0 : (int)data["blu_questionsetup1_x002e_blu_type"],
                    IsMandatory = data["blu_questionsetup1_x002e_blu_answerismandatory"] == null ?
                                    false : (bool)data["blu_questionsetup1_x002e_blu_answerismandatory"],
                    AdditionalProductId = data["blu_questionsetup1_x002e_blu_additionalproduct"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetup1_x002e_blu_additionalproduct"].ToString()),
                };
                questionSetup.UniqueName = Guid.NewGuid().ToString().Replace("-", "");
                questionSetup.Answer = questionSetup.Type == (int)InspectionPortalQA_Type.TC ? "true" : "";
                questionSetups.Add(questionSetup);

            }

            return questionSetups;

        }

        public static List<InspectionPortalQA> QueryTermsAndConditionsByQuestionSetup(HttpClient httpClient, Guid questionSetupId)
        {

            var questionSetups = new List<InspectionPortalQA>();

            var fetchXml = string.Empty;
            fetchXml += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXml += "  <entity name='blu_parentcategorysetup_questionsetup' >";
            fetchXml += "	<attribute name='blu_questionsetupidone' />";
            fetchXml += "    <attribute name='blu_questionsetupidtwo' />";
            fetchXml += "    <link-entity name='blu_questionsetup' from='blu_questionsetupid' to='blu_questionsetupidone' link-type='inner' >";
            fetchXml += "      <filter>";
            fetchXml += "        <condition attribute='blu_questionsetupid' operator='eq' value='" + questionSetupId + "' />";
            fetchXml += "        <condition attribute='blu_requiredfor' operator='eq' value='" + (int)InspectionPortalQA_RequiredFor.Portal + "' />";
            fetchXml += "      </filter>";
            fetchXml += "    </link-entity>";
            fetchXml += "	<link-entity name='blu_questionsetup' from='blu_questionsetupid' to='blu_questionsetupidtwo' >";
            fetchXml += "      <attribute name='blu_questionsetupid' />";
            fetchXml += "      <attribute name='blu_name' />";
            fetchXml += "      <attribute name='blu_number' />";
            fetchXml += "      <attribute name='blu_type' />";
            fetchXml += "      <attribute name='blu_answerdatatype' />";
            fetchXml += "      <attribute name='blu_optionvalues' />";
            fetchXml += "      <attribute name='blu_uniquename' />";
            fetchXml += "      <filter>";
            fetchXml += "        <condition attribute='blu_requiredfor' operator='eq' value='" + (int)InspectionPortalQA_RequiredFor.Portal + "' />";
            fetchXml += "        <condition attribute='blu_type' operator='eq' value='858890002' />";
            fetchXml += "      </filter>";
            fetchXml += "    </link-entity>";
            fetchXml += "  </entity>";
            fetchXml += "</fetch>";

            var encodedQuery = SharedService.UrlEncode(fetchXml);

            var odataQuery = webApiQueryUrl + "blu_parentcategorysetup_questionsetupset?fetchXml=" +
                             encodedQuery;

            var retrieveResponse = httpClient.GetAsync(odataQuery);
            var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
            dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
            if (systemUserObject == null || systemUserObject.value == null) return null;
            if (systemUserObject.value.Count == 0) return null;

            foreach (var data in systemUserObject.value)
            {
                var questionSetup = new InspectionPortalQA()
                {
                    Id = data["blu_questionsetup2_x002e_blu_questionsetupid"] == null ? Guid.Empty : Guid.Parse(data["blu_questionsetup2_x002e_blu_questionsetupid"].ToString()),

                    AnswerDataType = data["blu_questionsetup2_x002e_blu_answerdatatype"] == null ?
                                    0 : (int)data["blu_questionsetup2_x002e_blu_answerdatatype"],

                    AnswerDataTypeText = data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"] == null ?
                    "" : data["blu_questionsetup2_x002e_blu_answerdatatype@OData.Community.Display.V1.FormattedValue"].ToString(),

                    Name = data["blu_questionsetup2_x002e_blu_name"] == null ? "" : data["blu_questionsetup2_x002e_blu_name"].ToString(),
                    UniqueQuestionName = data["blu_questionsetup2_x002e_blu_uniquename"] == null ? "" : data["blu_questionsetup2_x002e_blu_uniquename"].ToString(),
                    Number = data["blu_questionsetup2_x002e_blu_number"] == null ?
                                    -99 : (int)data["blu_questionsetup2_x002e_blu_number"],
                    OptionSetValues = data["blu_questionsetup2_x002e_blu_optionvalues"] == null
                        ? new string[0]
                        : data["blu_questionsetup2_x002e_blu_optionvalues"].ToString().Split(','),
                    Type = data["blu_questionsetup2_x002e_blu_type"] == null ?
                                    0 : (int)data["blu_questionsetup2_x002e_blu_type"],
                    IsMandatory = data["blu_questionsetup1_x002e_blu_answerismandatory"] == null ?
                                    false : (bool)data["blu_questionsetup1_x002e_blu_answerismandatory"],
                };
                questionSetup.UniqueName = Guid.NewGuid().ToString().Replace("-", "");
                questionSetups.Add(questionSetup);

            }

            return questionSetups;

        }

        public static Guid CreateInspectionPortalQA(HttpClient httpClient, InspectionPortalQA questionSetup)
        {

            var questionObject = new JObject()
            {
                ["blu_name"] = questionSetup.Name,
                ["blu_answer"] = questionSetup.Answer,
                ["blu_number"] = questionSetup.Number.ToString(),
                ["blu_type"] = questionSetup.Type,
            };

            if (questionSetup.PropertyId != Guid.Empty)
            {
                questionObject["blu_PropertyId@odata.bind"] = "/blu_inspections(" + questionSetup.PropertyId + ")";
            }
            if (questionSetup.InspectionId != Guid.Empty)
            {
                questionObject["blu_InspectionDetailId@odata.bind"] = "/blu_inspectiondetails(" + questionSetup.InspectionId + ")";
            }
            if (questionSetup.OpportunityId != Guid.Empty)
            {
                questionObject["blu_opportunity@odata.bind"] = "/opportunities(" + questionSetup.OpportunityId + ")";
            }
            if (questionSetup.OrderId != Guid.Empty)
            {
                questionObject["blu_Order@odata.bind"] = "/salesorders(" + questionSetup.OrderId + ")";
            }
            if (questionSetup.Id != Guid.Empty)
            {
                questionObject["blu_QuestionSetup@odata.bind"] = "/blu_questionsetups(" + questionSetup.Id + ")";
            }

            var request = new HttpRequestMessage(HttpMethod.Post,
                webApiQueryUrl + "blu_inspectionportalqas")
            {
                Content = new StringContent(questionObject.ToString(), Encoding.UTF8, "application/json")
            };
            var response = httpClient.SendAsync(request);
            if (!response.Result.IsSuccessStatusCode)
            {
                var error = response.Result.Content.ReadAsStringAsync();
                return Guid.Empty;
            }
            var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
            if (recordUrl == null) return Guid.Empty;
            var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
            var recordId = Guid.Parse(splitRetrievedData[1]);
            return recordId;

        }

        public static Guid CreateQA0(HttpClient httpClient, List<InspectionPortalQA> questions, Guid propertyId, Guid inspectionDetailId)
        {

            var questionMappingObject = new JObject();

            #region Questions
            var blu_dateoffirstopenhouse
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ24")?.Answer;
            if (blu_dateoffirstopenhouse != null)
            {
                var dateParts = blu_dateoffirstopenhouse.Split('/');
                questionMappingObject["blu_dateoffirstopenhouse"] = dateParts[0];
            }
            //---
            var blu_auctiondateendofsalescampaignexpiry
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ23")?.Answer;
            if (blu_auctiondateendofsalescampaignexpiry != null)
            {
                var dateParts = blu_auctiondateendofsalescampaignexpiry.Split('/');
                questionMappingObject["blu_auctiondateendofsalescampaignexpiry"] = dateParts[2];
            }
            //---
            var blu_doesthepropertyhaveapool
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ31")?.Answer;
            if (blu_doesthepropertyhaveapool != null)
                questionMappingObject["blu_doesthepropertyhaveapool"] = blu_doesthepropertyhaveapool;
            //---
            var blu_looingfillinsulationsamplestestedforasbestos
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ33")?.Answer;
            if (blu_looingfillinsulationsamplestestedforasbestos != null)
                questionMappingObject["blu_looingfillinsulationsamplestestedforasbestos"] = blu_looingfillinsulationsamplestestedforasbestos;
            //---
            var blu_poolcomplaincecertificaterequired
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ32")?.Answer;
            if (blu_poolcomplaincecertificaterequired != null)
                questionMappingObject["blu_poolcomplaincecertificaterequired"] = blu_poolcomplaincecertificaterequired;
            //---
            var blu_acknowlegementofinvoiceduewithinagreedte
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ30")?.Answer;
            if (blu_acknowlegementofinvoiceduewithinagreedte != null)
                questionMappingObject["blu_acknowlegementofinvoiceduewithinagreedte"] = blu_acknowlegementofinvoiceduewithinagreedte;
            //---
            var blu_additionalfeeforkeypickupanddropoff
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ18")?.Answer;
            if (blu_additionalfeeforkeypickupanddropoff != null)
                questionMappingObject["blu_additionalfeeforkeypickupanddropoff"] = blu_additionalfeeforkeypickupanddropoff;
            //---
            var blu_istheinspectorrequiredtopickupanddropoff
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ17")?.Answer;
            if (blu_istheinspectorrequiredtopickupanddropoff != null)
                questionMappingObject["blu_istheinspectorrequiredtopickupanddropoff"] = blu_istheinspectorrequiredtopickupanddropoff;
            //---
            var blu_propertyreadyforinspectionscheduleddate
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ20")?.Answer;
            if (blu_propertyreadyforinspectionscheduleddate != null)
                questionMappingObject["blu_propertyreadyforinspectionscheduleddate"] = blu_propertyreadyforinspectionscheduleddate;
            //---
            var blu_onaccountvendortosettle
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ29")?.Answer;
            if (blu_onaccountvendortosettle != null)
                questionMappingObject["blu_onaccountvendortosettle"] = blu_onaccountvendortosettle;
            //---
            var blu_letterofauthoritysentotstratamanager
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ41")?.Answer;
            if (blu_letterofauthoritysentotstratamanager != null)
                questionMappingObject["blu_letterofauthoritysentotstratamanager"] = blu_letterofauthoritysentotstratamanager;
            //---
            var blu_listingagentinternaladmin
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ08")?.Answer;
            if (blu_listingagentinternaladmin != null)
                questionMappingObject["blu_listingagentinternaladmin"] = blu_listingagentinternaladmin;
            //---
            var blu_listingagent
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ01")?.Answer;
            if (blu_listingagent != null)
                questionMappingObject["blu_listingagent"] = blu_listingagent;
            //---
            var blu_locationforkeystobecollectedandreturned
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ19")?.Answer;
            if (blu_locationforkeystobecollectedandreturned != null)
                questionMappingObject["blu_locationforkeystobecollectedandreturned"] = blu_locationforkeystobecollectedandreturned;
            //---
            var blu_lotnumbers
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ36")?.Answer;
            if (blu_lotnumbers != null)
                questionMappingObject["blu_lotnumbers"] = blu_lotnumbers;
            //---
            var ontheproposedateisthereanyscheduledactiv
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ21")?.Answer;
            if (ontheproposedateisthereanyscheduledactiv != null)
                questionMappingObject["ontheproposedateisthereanyscheduledactiv"] = ontheproposedateisthereanyscheduledactiv;
            //---
            var blusiteaddress
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ04")?.Answer;
            if (blusiteaddress != null)
                questionMappingObject["blusiteaddress"] = blusiteaddress;
            //---
            var blu_specialinstructions
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ25")?.Answer;
            if (blu_specialinstructions != null)
                questionMappingObject["blu_specialinstructions"] = blu_specialinstructions;
            //---
            var blu_stataplannumber
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ35")?.Answer;
            if (blu_stataplannumber != null)
                questionMappingObject["blu_stataplannumber"] = blu_stataplannumber;
            //---
            var blu_whowillmeettheinspectoronsite
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ15")?.Answer; //lookup
            if (blu_whowillmeettheinspectoronsite != null)
            {
                if (IsGuid(blu_whowillmeettheinspectoronsite))
                    questionMappingObject["blu_WhowillmeettheinspectoronSite@odata.bind"] = "/contacts(" + blu_whowillmeettheinspectoronsite + ")";
            }
            //---
            var blu_colistingagentsalesteam
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ11")?.Answer;
            if (blu_colistingagentsalesteam != null)
            {
                if (IsGuid(blu_colistingagentsalesteam))
                    questionMappingObject["blu_CoListingAgentSalesTeam@odata.bind"] = "/contacts(" + blu_colistingagentsalesteam + ")";
            }
            //---
            var blu_ifyesselectwhoistobecontacted
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ27")?.Answer;
            if (blu_ifyesselectwhoistobecontacted != null)
            {
                if (IsGuid(blu_ifyesselectwhoistobecontacted))
                    questionMappingObject["blu_IfYesselectwhoistobecontacted@odata.bind"] = "/contacts(" + blu_ifyesselectwhoistobecontacted + ")";
            }
            //---
            var blu_stratacompany
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ37")?.Answer;
            if (blu_stratacompany != null)
            {
                if (IsGuid(blu_stratacompany))
                    questionMappingObject["blu_StrataCompny@odata.bind"] = "/contacts(" + blu_stratacompany + ")";
            }
            //---
            var Blu_vendor
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ05")?.Answer; //lookup
            if (Blu_vendor != null)
            {
                if (IsGuid(Blu_vendor))
                    questionMappingObject["blu_Vendor@odata.bind"] = "/contacts(" + Blu_vendor + ")";
            }
            //---
            var blu_typeofsale
                = questions.FirstOrDefault(x => x != null && x.UniqueQuestionName == "PQ22")?.Answer;
            if (blu_typeofsale != null)
                questionMappingObject["blu_typeofsale"] = blu_typeofsale;

            questionMappingObject["blu_SiteAddress@odata.bind"] = "/blu_inspections(" + propertyId + ")";
            questionMappingObject["blu_Inspection@odata.bind"] = "/blu_inspectiondetails(" + inspectionDetailId + ")";

            #endregion

            var request = new HttpRequestMessage(HttpMethod.Post,
                webApiQueryUrl + "blu_questionmappings")
            {
                Content = new StringContent(questionMappingObject.ToString(), Encoding.UTF8, "application/json")
            };
            var response = httpClient.SendAsync(request);
            if (!response.Result.IsSuccessStatusCode)
            {
                var error = response.Result.Content.ReadAsStringAsync();
                return Guid.Empty;
            }
            var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
            if (recordUrl == null) return Guid.Empty;
            var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
            var recordId = Guid.Parse(splitRetrievedData[1]);
            return recordId;

        }

        public static bool IsGuid(string guidString)
        {
            Guid guid;
            if (guidString == null) throw new ArgumentNullException("guidString");
            try
            {
                guid = new Guid(guidString);
                return true;
            }
            catch (FormatException)
            {
                guid = default(Guid);
                return false;
            }
        }

    }
}
