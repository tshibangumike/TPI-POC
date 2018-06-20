using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class ProductPostUpdate: IPlugin
    {

        private ITracingService tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var entity = ((Entity)context.InputParameters["Target"]);
            var postProductEntity = (Entity)context.PostEntityImages["PostProductImage"];

            if (context.Depth > 1) return;

            try
            {

                UpdatePriceListItems(service, postProductEntity);

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the OrderPostCreate plug-in.", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("OrderPostCreate: {0}", ex.ToString());
                throw;
            }

        }

        public void UpdatePriceListItems(IOrganizationService service, Entity product)
        {

            if (!product.Attributes.Contains("price")) return;

            var fetchXML = string.Empty;
            fetchXML += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXML += "  <entity name='productpricelevel'>";
            fetchXML += "    <attribute name='productid' />";
            fetchXML += "    <attribute name='amount' />";
            fetchXML += "    <filter type='and'>";
            fetchXML += "      <condition attribute='productid' operator='eq' value='" + product.Id + "' />";
            fetchXML += "    </filter>";
            fetchXML += "  </entity>";
            fetchXML += "</fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXML));
            if (ec.Entities.Count > 0)
            {
                foreach (var priceListItem in ec.Entities)
                {
                    try
                    {

                        priceListItem["amount"] = product["price"];
                        service.Update(priceListItem);

                    }
                    catch (Exception ex)
                    {
                        tracingService.Trace("InspectionDetailPostCreate: {0}", ex.ToString());
                        continue;
                    }

                }
            }
        }

    }
}
