using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class InspectionAnswerPostCreate : IPlugin
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
                #region query Question - Explanations
                var fetchXmlExplanations = string.Empty;
                fetchXmlExplanations += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlExplanations += "    <entity name='blu_questionexplanation'>";
                fetchXmlExplanations += "        <attribute name = 'blu_questionexplanationid'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_answerismandatory'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_name'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_visibleondevice'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_question'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_photosaremandatory'/>";
                fetchXmlExplanations += "        <attribute name = 'blu_explanationtype'/>";
                fetchXmlExplanations += "        <link-entity name='blu_questionquestion' from='blu_questionquestionid' to='blu_question' link-type='inner'>";
                fetchXmlExplanations += "           <link-entity name='blu_questionanswer' from='blu_question' to='blu_questionquestionid' link-type='inner'>";
                fetchXmlExplanations += "              <filter type='and'>";
                fetchXmlExplanations += "                  <condition attribute='blu_questionanswerid' operator='eq' value = '" + entity.GetAttributeValue<EntityReference>("blu_setupanswer").Id.ToString() + "'/>";
                fetchXmlExplanations += "              </filter>";
                fetchXmlExplanations += "           </link-entity>";
                fetchXmlExplanations += "        </link-entity>";
                fetchXmlExplanations += "    </entity>";
                fetchXmlExplanations += "</fetch>";
                #endregion

                EntityCollection ecExplanation = service.RetrieveMultiple(new FetchExpression(fetchXmlExplanations));
                if (ecExplanation.Entities.Count > 0)
                {
                    foreach (var enExplanation in ecExplanation.Entities)
                    {
                        var enInspectionExplanation = new Entity("blu_inspectionexplanation")
                        {
                            ["blu_answerismandatory"] = enExplanation.GetAttributeValue<bool>("blu_answerismandatory"),
                            ["blu_name"] = enExplanation.GetAttributeValue<String>("blu_name"),
                            ["blu_visibleondevice"] = enExplanation.GetAttributeValue<bool>("blu_visibleondevice"),
                            ["blu_answerid"] = new EntityReference("blu_inspectionanswer", context.PrimaryEntityId),
                            ["blu_photosaremandatory"] = enExplanation.GetAttributeValue<bool>("blu_photosaremandatory"),
                            ["blu_explanationtype"] = enExplanation.GetAttributeValue<OptionSetValue>("blu_explanationtype"),
                            ["blu_setupexplanation"] = new EntityReference("blu_questionexplanation", enExplanation.Id)
                        };
                        service.Create(enInspectionExplanation);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the InspectionAnswerPostCreate plug-in.", ex);
            }
        }
    }
}
