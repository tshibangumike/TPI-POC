using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace tpi.CustomWFA
{
    public sealed class WFCloneQuestionAnswerNotes : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference enQuestionAnswer = QuestionAnswerLookup.Get<EntityReference>(executionContext);

            #region Query Notes associated to the Inspection - Option
            var fetchXmlInspectionAnswerNotes = string.Empty;
            fetchXmlInspectionAnswerNotes += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXmlInspectionAnswerNotes += "  <entity name='annotation'>";
            fetchXmlInspectionAnswerNotes += "     <attribute name='annotationid'/>";
            fetchXmlInspectionAnswerNotes += "     <attribute name='documentbody'/>";
            fetchXmlInspectionAnswerNotes += "     <attribute name='subject'/>";
            fetchXmlInspectionAnswerNotes += "     <attribute name='filename'/>";
            fetchXmlInspectionAnswerNotes += "     <order attribute='createdon' descending='true'/>";
            fetchXmlInspectionAnswerNotes += "     <filter type='and'>";
            fetchXmlInspectionAnswerNotes += "        <condition attribute='objectid' operator='eq' value='" + enQuestionAnswer.Id.ToString() + "'/>";
            fetchXmlInspectionAnswerNotes += "        <condition attribute='isdocument' operator='eq' value='1'/>";
            fetchXmlInspectionAnswerNotes += "     </filter>";
            fetchXmlInspectionAnswerNotes += "  </entity>";
            fetchXmlInspectionAnswerNotes += "</fetch>";
            #endregion

            EntityCollection ecAnnotation = service.RetrieveMultiple(new FetchExpression(fetchXmlInspectionAnswerNotes));
            if (ecAnnotation.Entities.Count > 0)
            {
                foreach (var enAnnotation in ecAnnotation.Entities)
                {
                    Entity enNote = new Entity("annotation")
                    {
                        ["subject"] = enAnnotation.GetAttributeValue<string>("subject"),
                        ["filename"] = enAnnotation.GetAttributeValue<string>("filename"),
                        ["documentbody"] = enAnnotation.GetAttributeValue<string>("documentbody"),
                        ["objectid"] = new EntityReference("blu_inspectionanswer", context.PrimaryEntityId)
                    };
                    service.Create(enNote);
                }
            }
        }

        [Input("Setup Answer Record")]
        [ReferenceTarget("blu_questionanswer")]
        [RequiredArgument]
        public InArgument<EntityReference> QuestionAnswerLookup { get; set; }
    }
}


