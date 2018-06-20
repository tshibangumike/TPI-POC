using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class InspectionCategoryPostCreate : IPlugin
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
                #region query Question - Question
                var fetchXmlQuestion = string.Empty;
                fetchXmlQuestion += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlQuestion += "  <entity name='blu_questionquestion'>";
                fetchXmlQuestion += "    <attribute name='blu_questionquestionid'/>";
                fetchXmlQuestion += "    <attribute name='blu_name'/>";
                fetchXmlQuestion += "    <attribute name='blu_allowmultipleanswers'/>";
                fetchXmlQuestion += "    <attribute name='blu_questionorder'/>";
                fetchXmlQuestion += "    <attribute name='blu_answertype'/>";
                fetchXmlQuestion += "    <attribute name='blu_category'/>";
                fetchXmlQuestion += "    <attribute name='blu_inspectorisrequiredtoanswerthisquestion'/>";
                fetchXmlQuestion += "    <attribute name='blu_notesvisibleoninspectionreport'/>";
                fetchXmlQuestion += "    <attribute name='blu_notesvisibletoinspector'/>";
                fetchXmlQuestion += "    <attribute name='blu_diagramvisibletoinspector'/>";
                fetchXmlQuestion += "    <attribute name='blu_diagramvisibleoninspectionreport'/>";
                fetchXmlQuestion += "    <filter>";
                fetchXmlQuestion += "      <condition attribute='blu_category' operator='eq' value='" + entity.GetAttributeValue<EntityReference>("blu_setupcategory").Id.ToString() + "'/>";
                fetchXmlQuestion += "    </filter>";
                fetchXmlQuestion += "  </entity>";
                fetchXmlQuestion += "</fetch>";
                #endregion

                EntityCollection ecQuestion = service.RetrieveMultiple(new FetchExpression(fetchXmlQuestion));
                foreach (var enQuestion in ecQuestion.Entities)
                {
                    if (ecQuestion.Entities.Count > 0)
                    {
                        var enInspectionQuestion = new Entity("blu_inspectionquestion")
                        {
                            ["blu_name"] = enQuestion.GetAttributeValue<String>("blu_name"),
                            ["blu_allowmultipleanswers"] = enQuestion.GetAttributeValue<bool>("blu_allowmultipleanswers"),
                            ["blu_questionorder"] = enQuestion.GetAttributeValue<int>("blu_questionorder"),
                            ["blu_answertype"] = enQuestion.GetAttributeValue<OptionSetValue>("blu_answertype"),
                            ["blu_categoryid"] = new EntityReference("blu_inspectioncategory", context.PrimaryEntityId),
                            ["blu_inspectorisrequiredtoanswerthisquestion"] = enQuestion.GetAttributeValue<bool>("blu_inspectorisrequiredtoanswerthisquestion"),
                            ["blu_notesvisibleoninspectionreport"] = enQuestion.GetAttributeValue<bool>("blu_notesvisibleoninspectionreport"),
                            ["blu_notesvisibletoinspector"] = enQuestion.GetAttributeValue<bool>("blu_notesvisibletoinspector"),
                            ["blu_diagramvisibletoinspector"] = enQuestion.GetAttributeValue<bool>("blu_diagramvisibletoinspector"),
                            ["blu_diagramvisibleoninspectionreport"] = enQuestion.GetAttributeValue<bool>("blu_diagramvisibleoninspectionreport"),
                            ["blu_setupquestion"] = new EntityReference("blu_questionquestion", enQuestion.Id)
                        };
                        service.Create(enInspectionQuestion);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the InspectionCategoryPostCreate plug-in.", ex);
            }
        }
    }
}
