using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class OrderPostCreate : IPlugin
    {
        private ITracingService tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var entity = ((Entity)context.InputParameters["Target"]);
            var postOrderEntity = (Entity)context.PostEntityImages["PostOrderImage"];

            try
            {

                CreateFinalOrder(service, postOrderEntity);

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

        public void CreateFinalOrder(IOrganizationService service, Entity order)
        {

            if (order.Attributes.Contains("blu_issplitorder"))
            {
                var isSplitOrder = (bool)order["blu_issplitorder"];
                if (isSplitOrder) return;
            }

            if (!order.Attributes.Contains("opportunityid")) return;

            var opportunityEr = (EntityReference)order["opportunityid"];

            #region Get all opportunity products for this opportunity
            var fetchXmlOpprotunityProducts = string.Empty;
            fetchXmlOpprotunityProducts += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXmlOpprotunityProducts += "   <entity name='opportunityproduct' >";
            fetchXmlOpprotunityProducts += "	    <attribute name='blu_sellableto' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='blu_productcategory' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='blu_reportisresellable' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='blu_stratareport' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='productid' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='blu_productcategory' />";
            fetchXmlOpprotunityProducts += "	    <attribute name='uomid' />";
            fetchXmlOpprotunityProducts += "       <filter>";
            fetchXmlOpprotunityProducts += "           <condition attribute='opportunityid' operator='eq' value='" + opportunityEr.Id + "' />";
            fetchXmlOpprotunityProducts += "       </filter>";
            fetchXmlOpprotunityProducts += "   </entity>";
            fetchXmlOpprotunityProducts += "</fetch>";
            #endregion

            EntityCollection ecOpportunityProducts = service.RetrieveMultiple(new FetchExpression(fetchXmlOpprotunityProducts));
            if (ecOpportunityProducts.Entities.Count == 0) return;

            var orderProducts = new List<Entity>();
            var countOfSellableToOpportunityProducts = 0;

            foreach (var opportunityProduct in ecOpportunityProducts.Entities)
            {

                if (!opportunityProduct.Attributes.Contains("blu_sellableto")) return;
                var sellableToOs = (OptionSetValue)opportunityProduct["blu_sellableto"];

                /*If Option Set Value is not equal to "Multi Party"*/
                if (sellableToOs.Value != 858890002) continue;

                countOfSellableToOpportunityProducts++;
                orderProducts.Add(GetOrderProductEntity(opportunityProduct));

            }

            if (countOfSellableToOpportunityProducts == 0) return;

            /*Create Order*/

            var newOrder = new Entity("salesorder")
            {
                Id = Guid.NewGuid(),
                ["blu_issplitorder"] = true
            };

            if (order.Attributes.Contains("name"))
                newOrder["name"] = order["name"];

            if (order.Attributes.Contains("blu_regardingpropertyid"))
                newOrder["blu_regardingpropertyid"] = order["blu_regardingpropertyid"];

            var newOrderId = service.Create(newOrder);

            foreach (var newOrderProduct in orderProducts)
            {

                newOrderProduct["salesorderid"] = new EntityReference("salesorder", newOrderId);
                service.Create(newOrderProduct);

            }

        }

        public Entity GetOrderProductEntity(Entity opportunityProduct)
        {

            var newOrderProduct = new Entity("salesorderdetail")
            {
                Id = Guid.NewGuid()
            };

            if (opportunityProduct.Attributes.Contains("blu_productcategory"))
                newOrderProduct["blu_productcategory"] = opportunityProduct["blu_productcategory"];

            if (opportunityProduct.Attributes.Contains("blu_reportisresellable"))
                newOrderProduct["blu_reportisresellable"] = opportunityProduct["blu_reportisresellable"];

            if (opportunityProduct.Attributes.Contains("blu_sellableto"))
                newOrderProduct["blu_sellableto"] = opportunityProduct["blu_sellableto"];

            if (opportunityProduct.Attributes.Contains("blu_stratareport"))
                newOrderProduct["blu_stratareport"] = opportunityProduct["blu_stratareport"];

            if (opportunityProduct.Attributes.Contains("productid"))
                newOrderProduct["productid"] = opportunityProduct["productid"];

            if (opportunityProduct.Attributes.Contains("quantity"))
                newOrderProduct["quantity"] = opportunityProduct["quantity"];

            if (opportunityProduct.Attributes.Contains("uomid"))
                newOrderProduct["uomid"] = opportunityProduct["uomid"];

            return newOrderProduct;

        }

        public void CreateLog(IOrganizationService service, string name)
        {
            var entity = new Entity("annotation")
            {
                ["subject"] = name
            };
            service.Create(entity);
        }

    }
}
