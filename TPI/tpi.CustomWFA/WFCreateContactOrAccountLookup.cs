using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace tpi.CustomWFA
{
    public sealed class WFCreateContactLookup : CodeActivity
    {
        private String strFirstName = String.Empty;
        private String strLastName = String.Empty;
        private String strEmailAddress = String.Empty;
        private String strMobilePhone = String.Empty;
        Guid recordId = Guid.Empty;
        String queryAttribute = string.Empty;
        bool isInspection = false;
        bool isOrder = false;

        protected override void Execute(CodeActivityContext executionContext)
        {
            //ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (InspectionLookup.Get<EntityReference>(executionContext) != null)
            {
                recordId = InspectionLookup.Get<EntityReference>(executionContext).Id;
                queryAttribute = "blu_inspectiondetailid";
                isInspection = true;
            }
            if (OrderLookup.Get<EntityReference>(executionContext) != null)
            {
                recordId = OrderLookup.Get<EntityReference>(executionContext).Id;
                queryAttribute = "blu_order";
                isOrder = true;
            }
            if (isInspection == false && isOrder == false)
            {
                return;
            }

            strFirstName = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                                , FirstNameQuestion.Get<string>(executionContext)
                                , queryAttribute
                                , recordId
                                , service);
            strLastName = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                                , LastNameQuestion.Get<string>(executionContext)
                                , queryAttribute
                                , recordId
                                , service);
            strEmailAddress = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                                    , EmailAddressQuestion.Get<string>(executionContext)
                                    , queryAttribute
                                    , recordId
                                    , service);
            strMobilePhone = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                                    , MobileNumberQuestion.Get<string>(executionContext)
                                    , queryAttribute
                                    , recordId
                                    , service);
            
            if (strFirstName != String.Empty && strLastName != String.Empty && strEmailAddress != String.Empty && strMobilePhone != String.Empty)
            {
                Entity enContact = new Entity("contact")
                {
                    ["firstname"] = strFirstName,
                    ["lastname"] = strLastName,
                    ["emailaddress1"] = strEmailAddress,
                    ["mobilephone"] = strMobilePhone
                };

                Guid contactId = Guid.Empty;
                //Check if Contact record already exists otherwise create new
                #region query Contact
                var fetchXmlContact = string.Empty;
                fetchXmlContact += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlContact += "  <entity name='contact'>";
                fetchXmlContact += "     <attribute name='contactid'/>";
                fetchXmlContact += "     <filter type='and'>";
                fetchXmlContact += "        <condition attribute='firstname' operator='eq' value='" + enContact["firstname"].ToString() + "'/>";
                fetchXmlContact += "        <condition attribute='lastname' operator='eq' value='" + enContact["lastname"].ToString() + "'/>";
                fetchXmlContact += "        <condition attribute='emailaddress1' operator='eq' value='" + enContact["emailaddress1"].ToString() + "'/>";
                fetchXmlContact += "        <condition attribute='mobilephone' operator='eq' value='" + enContact["mobilephone"].ToString() + "'/>";
                fetchXmlContact += "        <condition attribute='statecode' operator='eq' value='0'/>";
                fetchXmlContact += "     </filter>";
                fetchXmlContact += "  </entity>";
                fetchXmlContact += "</fetch>";
                #endregion

                EntityCollection ecContact = service.RetrieveMultiple(new FetchExpression(fetchXmlContact));
                if (ecContact.Entities.Count > 0)
                {
                    foreach (var enContactResult in ecContact.Entities)
                    {
                        contactId = enContactResult.Id;
                    }
                }
                else
                {
                    contactId = service.Create(enContact);
                }

                ContactLookup.Set(executionContext, new EntityReference("contact", contactId));
            }
        }

        [Input("Inspection Record")]
        [ReferenceTarget("blu_inspectiondetail")]
        public InArgument<EntityReference> InspectionLookup { get; set; }

        [Input("Inspection Field Schema Name to Update")]
        public InArgument<string> InspectionFieldSchemaName { get; set; }

        [Input("Sales Order Record")]
        [ReferenceTarget("salesorder")]
        public InArgument<EntityReference> OrderLookup { get; set; }

        [Input("Order Field Schema Name to Update")]
        public InArgument<string> OrderFieldSchemaName { get; set; }

        [Input("Question Number for First Name")]
        [RequiredArgument]
        public InArgument<string> FirstNameQuestion { get; set; }

        [Input("Question Number for Last Name")]
        [RequiredArgument]
        public InArgument<string> LastNameQuestion { get; set; }

        [Input("Question Number for Email")]
        [RequiredArgument]
        public InArgument<string> EmailAddressQuestion { get; set; }

        [Input("Question Number for Mobile Number")]
        [RequiredArgument]
        public InArgument<string> MobileNumberQuestion { get; set; }

        [Output("Contact Lookup")]
        [ReferenceTarget("contact")]
        public OutArgument<EntityReference> ContactLookup { get; set; }
    }

    public sealed class WFCreateAccountLookup : CodeActivity
    {
        private String strName = String.Empty;
        private String strAddress = String.Empty;
        private String strEmailAddress = String.Empty;
        private String strMainPhone = String.Empty;
        Guid recordId = Guid.Empty;
        String queryAttribute = string.Empty;
        bool isInspection = false;
        bool isOrder = false;

        protected override void Execute(CodeActivityContext executionContext)
        {
            //ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            
            if (InspectionLookup.Get<EntityReference>(executionContext) != null)
            {
                recordId = InspectionLookup.Get<EntityReference>(executionContext).Id;
                queryAttribute = "blu_inspectiondetailid";
                isInspection = true;
            }
            if (OrderLookup.Get<EntityReference>(executionContext) != null)
            {
                recordId = OrderLookup.Get<EntityReference>(executionContext).Id;
                queryAttribute = "blu_order";
                isOrder = true;
            }
            if (isInspection == false && isOrder == false)
            {
                return;
            }

            strName = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                , NameQuestion.Get<string>(executionContext)
                , queryAttribute
                , recordId
                , service);
            strAddress = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                , AddressQuestion.Get<string>(executionContext)
                , queryAttribute
                , recordId
                , service);
            strEmailAddress = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                , EmailAddressQuestion.Get<string>(executionContext)
                , queryAttribute
                , recordId
                , service);
            strMainPhone = WFHelpers.GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(executionContext
                , TelephoneNumberQuestion.Get<string>(executionContext)
                , queryAttribute
                , recordId
                , service);

                if (strName != String.Empty && strAddress != String.Empty && strEmailAddress != String.Empty && strMainPhone != String.Empty)
            {
                Entity enAccount = new Entity("account")
                {
                    ["name"] = strName,
                    ["address1_composite"] = strAddress,
                    ["emailaddress1"] = strEmailAddress,
                    ["telephone1"] = strMainPhone
                };

                Guid accountId = Guid.Empty;
                //Check if Account record already exists otherwise create new
                #region query Account
                var fetchXmlAccount = string.Empty;
                fetchXmlAccount += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlAccount += "  <entity name='account'>";
                fetchXmlAccount += "     <attribute name='accountid'/>";
                fetchXmlAccount += "     <filter type='and'>";
                fetchXmlAccount += "        <condition attribute='name' operator='eq' value='" + enAccount["name"].ToString() + "'/>";
                //fetchXmlAccount += "        <condition attribute='address1_composite' operator='eq' value='" + enAccount["address1_composite"].ToString() + "'/>";
                fetchXmlAccount += "        <condition attribute='emailaddress1' operator='eq' value='" + enAccount["emailaddress1"].ToString() + "'/>";
                fetchXmlAccount += "        <condition attribute='telephone1' operator='eq' value='" + enAccount["telephone1"].ToString() + "'/>";
                fetchXmlAccount += "        <condition attribute='statecode' operator='eq' value='0'/>";
                fetchXmlAccount += "     </filter>";
                fetchXmlAccount += "  </entity>";
                fetchXmlAccount += "</fetch>";
                #endregion

                EntityCollection ecAccount = service.RetrieveMultiple(new FetchExpression(fetchXmlAccount));
                if (ecAccount.Entities.Count > 0)
                {
                    foreach (var enAccountResult in ecAccount.Entities)
                    {
                        accountId = enAccountResult.Id;
                    }
                }
                else
                {
                    accountId = service.Create(enAccount);
                }
                
                AccountLookup.Set(executionContext, new EntityReference("account", accountId));
            }
        }

        [Input("Inspection Record")]
        [ReferenceTarget("blu_inspectiondetail")]
        public InArgument<EntityReference> InspectionLookup { get; set; }
        
        [Input("Inspection Field Schema Name to Update")]
        public InArgument<string> InspectionFieldSchemaName { get; set; }

        [Input("Sales Order Record")]
        [ReferenceTarget("salesorder")]
        public InArgument<EntityReference> OrderLookup { get; set; }
        
        [Input("Order Field Schema Name to Update")]
        public InArgument<string> OrderFieldSchemaName { get; set; }

        [Input("Question Number for Name")]
        [RequiredArgument]
        public InArgument<string> NameQuestion { get; set; }

        [Input("Question Number for Address")]
        [RequiredArgument]
        public InArgument<string> AddressQuestion { get; set; }

        [Input("Question Number for Email")]
        [RequiredArgument]
        public InArgument<string> EmailAddressQuestion { get; set; }

        [Input("Question Number for Telephone Number")]
        [RequiredArgument]
        public InArgument<string> TelephoneNumberQuestion { get; set; }

        [Output("Account Lookup")]
        [ReferenceTarget("account")]
        public OutArgument<EntityReference> AccountLookup { get; set; }
    }

    public class WFHelpers
    {
        public static String GetFirstPortalQuestionAsnwerByNumberAndLookupRecord(CodeActivityContext executionContext
            , string questionNumber, String queryAttribute, Guid recordId, IOrganizationService service)
        {
            String strAnswer = String.Empty;

            #region query Inspection Portal Q&A
            var fetchXmlInspPortalQA = string.Empty;
            fetchXmlInspPortalQA += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
            fetchXmlInspPortalQA += "  <entity name='blu_inspectionportalqa'>";
            fetchXmlInspPortalQA += "     <attribute name='blu_answer'/>";
            fetchXmlInspPortalQA += "     <filter type='and'>";
            fetchXmlInspPortalQA += "        <condition attribute='blu_number' operator='eq' value='" + questionNumber + "'/>";
            fetchXmlInspPortalQA += "        <condition attribute='blu_type' operator='eq' value='858890001'/>"; //Questions only
            fetchXmlInspPortalQA += "        <condition attribute='" + queryAttribute + "' operator='eq' value='" + recordId.ToString() + "'/>";
            fetchXmlInspPortalQA += "     </filter>";
            fetchXmlInspPortalQA += "  </entity>";
            fetchXmlInspPortalQA += "</fetch>";
            #endregion

            EntityCollection ecInspPortalQA = service.RetrieveMultiple(new FetchExpression(fetchXmlInspPortalQA));
            if (ecInspPortalQA.Entities.Count > 0)
            {
                foreach (var enInspPortalQAResult in ecInspPortalQA.Entities)
                {
                    strAnswer = enInspPortalQAResult.GetAttributeValue<string>("blu_answer");
                }
            }
            return strAnswer;
        }
    }
}