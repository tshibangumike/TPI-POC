using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class ProductInventoryPostCreate : IPlugin
    {
        private Guid propertyId = Guid.Empty;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var image = (Entity)context.PostEntityImages["PostImage"];

            try
            {
                if (!image.Attributes.Contains("blu_reporttype"))
                {
                    return;
                }

                var productEr = image.GetAttributeValue<EntityReference>("blu_reporttype");

                #region Update Sales Order Product > Product Inventory Lookup
                propertyId = image.GetAttributeValue<EntityReference>("blu_property").Id;
                
                #region query Sales Order Product
                var fetchXmlSalesOrderProduct = string.Empty;
                fetchXmlSalesOrderProduct += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlSalesOrderProduct += "  <entity name='salesorderdetail'>";
                fetchXmlSalesOrderProduct += "    <attribute name='salesorderdetailid'/>";
                fetchXmlSalesOrderProduct += "    <filter type='and'>";
                fetchXmlSalesOrderProduct += "      <condition attribute='blu_productinventory' operator='null'/>";
                fetchXmlSalesOrderProduct += "    </filter>";
                fetchXmlSalesOrderProduct += "    <link-entity name='product' from='productid' to='productid' link-type='inner'>";
                fetchXmlSalesOrderProduct += "      <filter type='and'>";
                fetchXmlSalesOrderProduct += "          <condition attribute='blu_stockingproduct' operator='eq' value='" + productEr.Id.ToString() + "'/>";
                fetchXmlSalesOrderProduct += "      </filter>";
                fetchXmlSalesOrderProduct += "    </link-entity>";
                fetchXmlSalesOrderProduct += "    <link-entity name='salesorder' from='salesorderid' to='salesorderid' link-type='inner'>";
                fetchXmlSalesOrderProduct += "        <attribute name='blu_regardingpropertyid'/>";
                fetchXmlSalesOrderProduct += "        <filter type='and'>";
                fetchXmlSalesOrderProduct += "            <condition attribute='blu_regardingpropertyid' operator='eq' value='" + propertyId.ToString() + "'/>";
                fetchXmlSalesOrderProduct += "            <condition attribute='statecode' operator='eq' value='0'/>";
                fetchXmlSalesOrderProduct += "        </filter>";
                fetchXmlSalesOrderProduct += "    </link-entity>";
                fetchXmlSalesOrderProduct += " </entity>";
                fetchXmlSalesOrderProduct += "</fetch>";
                #endregion
                
                EntityCollection ecSalesOrderProduct = service.RetrieveMultiple(new FetchExpression(fetchXmlSalesOrderProduct));

                if (ecSalesOrderProduct.Entities.Count > 0)
                {
                    foreach (var enSalesOrderProduct in ecSalesOrderProduct.Entities)
                    {
                        enSalesOrderProduct["blu_productinventory"] = new EntityReference("blu_productinventory", context.PrimaryEntityId);
                        service.Update(enSalesOrderProduct);
                    }
                }
                #endregion
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the InspectionDetailPostCreate plug-in.", ex);
            }
            catch (Exception ex)
            { 
                throw;
            }
        }
    }
}
