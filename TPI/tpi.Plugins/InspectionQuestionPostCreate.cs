using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class InspectionQuestionPostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var entity = ((Entity)context.InputParameters["Target"]);

            try
            {
                #region query Question - Answers
                var fetchXmlAnswer = string.Empty;
                fetchXmlAnswer += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlAnswer += "  <entity name='blu_questionanswer'>";
                fetchXmlAnswer += "    <attribute name='blu_questionanswerid'/>";
                fetchXmlAnswer += "    <attribute name='blu_name'/>";
                fetchXmlAnswer += "    <attribute name='blu_question'/>";
                fetchXmlAnswer += "    <attribute name='blu_explanationrequired'/>";
                fetchXmlAnswer += "    <attribute name='blu_supplementarytext'/>";
                fetchXmlAnswer += "    <filter>";
                fetchXmlAnswer += "      <condition attribute='blu_question' operator='eq' value = '" + entity.GetAttributeValue<EntityReference>("blu_setupquestion").Id.ToString() + "'/>";
                fetchXmlAnswer += "    </filter>";
                fetchXmlAnswer += "  </entity>";
                fetchXmlAnswer += "</fetch>";
                #endregion

                EntityCollection ecAnswer = service.RetrieveMultiple(new FetchExpression(fetchXmlAnswer));
                if (ecAnswer.Entities.Count > 0)
                {
                    foreach (var enAnswer in ecAnswer.Entities)
                    {
                        var enInspectionAnswer = new Entity("blu_inspectionanswer")
                        {
                            ["blu_name"] = enAnswer.GetAttributeValue<String>("blu_name"),
                            ["blu_questionid"] = new EntityReference("blu_inspectionquestion", context.PrimaryEntityId),
                            ["blu_explanationrequired"] = enAnswer.GetAttributeValue<bool>("blu_explanationrequired"),
                            ["blu_supplementarytext"] = enAnswer.GetAttributeValue<string>("blu_supplementarytext"),
                            ["blu_setupanswer"] = new EntityReference("blu_questionanswer", enAnswer.Id)
                        };
                        service.Create(enInspectionAnswer);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the InspectionQuestionPostCreate plug-in.", ex);
            }
        }
    }
}