using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class InspectionExplanationPostCreate : IPlugin
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
                #region query Question - Options
                var fetchXmlOptions = string.Empty;
                fetchXmlOptions += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlOptions += "    <entity name='blu_questionoption'>";
                fetchXmlOptions += "        <attribute name = 'blu_questionoptionid'/>";
                fetchXmlOptions += "        <attribute name = 'blu_name'/>";
                fetchXmlOptions += "        <attribute name = 'blu_explanation'/>";
                fetchXmlOptions += "        <filter>";
                fetchXmlOptions += "            <condition attribute='blu_explanation' operator='eq' value ='" + entity.GetAttributeValue<EntityReference>("blu_setupexplanation").Id.ToString() + "'/>";
                fetchXmlOptions += "        </filter>";
                fetchXmlOptions += "    </entity>";
                fetchXmlOptions += "</fetch>";
                #endregion

                EntityCollection ecOption = service.RetrieveMultiple(new FetchExpression(fetchXmlOptions));
                if (ecOption.Entities.Count > 0)
                {
                    foreach (var enOption in ecOption.Entities)
                    {
                        var enInspectionOption = new Entity("blu_inspectionoption")
                        {
                            ["blu_explanationid"] = new EntityReference("blu_inspectionexplanation", context.PrimaryEntityId),
                            ["blu_setupoption"] = new EntityReference("blu_questionoption", enOption.Id)
                        };

                        if (enOption.Attributes.Contains("blu_name"))
                        {
                            enInspectionOption["blu_name"] = enOption.GetAttributeValue<String>("blu_name");
                        }
                        service.Create(enInspectionOption);
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the InspectionExplanationPostCreate plug-in.", ex);
            }
        }
    }
}