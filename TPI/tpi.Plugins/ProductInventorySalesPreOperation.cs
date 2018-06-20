using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{

    public class ProductInventorySalesPreOperation : IPlugin
    {

        private ITracingService tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var entity = ((Entity)context.InputParameters["Target"]);

            try
            {
                if (entity.Attributes.Contains("blu_product") && entity.Attributes.Contains("blu_property"))
                {
                    GetNextNumber(service, entity, tracingService);
                }
                else
                {
                    tracingService.Trace("ProductInventorySalesNumberGenerator: {0}", "No Product or Property records exist");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the ProductInventorySalesNumberGenerator plug-in. {0}", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("ProductInventorySalesNumberGenerator: {0}", ex.ToString());
                throw;
            }

        }
        
        public void GetNextNumber(IOrganizationService service, Entity entity, ITracingService tracingService)
        {
            var productId = entity.GetAttributeValue<EntityReference>("blu_product").Id;
            var propertyId = entity.GetAttributeValue<EntityReference>("blu_property").Id;
            var intNextNumber = 1;

            #region query Product Inventory Sales
            var fetchXmlProductInventorySales = string.Empty;
            fetchXmlProductInventorySales += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
            fetchXmlProductInventorySales += "  <entity name='blu_productinventorysale'>";
            fetchXmlProductInventorySales += "    <attribute name='blu_productinventorysaleid'/>";
            fetchXmlProductInventorySales += "    <attribute name='createdon'/>";
            fetchXmlProductInventorySales += "    <attribute name='blu_salenumber'/>";
            fetchXmlProductInventorySales += "    <order attribute='createdon' descending='true'/>";
            fetchXmlProductInventorySales += "    <filter type='and'>";
            fetchXmlProductInventorySales += "      <condition attribute='blu_product' operator='eq' value='" + productId.ToString() + "'/>";
            fetchXmlProductInventorySales += "      <condition attribute='blu_property' operator='eq' value='" + propertyId.ToString() + "'/>";
            fetchXmlProductInventorySales += "    </filter>";
            fetchXmlProductInventorySales += " </entity>";
            fetchXmlProductInventorySales += "</fetch>";
            #endregion
            EntityCollection ecProductInventorySales = service.RetrieveMultiple(new FetchExpression(fetchXmlProductInventorySales));

            if (ecProductInventorySales.Entities.Count > 0)
            {
                foreach (var enProductInventorySalesResult in ecProductInventorySales.Entities)
                {
                    if (enProductInventorySalesResult.Attributes.Contains("blu_salenumber"))
                    {
                        intNextNumber = enProductInventorySalesResult.GetAttributeValue<int>("blu_salenumber");
                        intNextNumber += 1;
                    }
                }
            }
            entity["blu_salenumber"] = intNextNumber;
        }
    }
}
