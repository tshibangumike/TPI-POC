using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class OrderProductPostCreate : IPlugin
    {
        private Guid propertyId = Guid.Empty;
        private Guid stockingProductId = Guid.Empty;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var image = (Entity)context.PostEntityImages["PostImage"];

            try
            {
                if (!image.Attributes.Contains("productid"))
                {
                    return;
                }
                #region Update Sales Order Product > Product Inventory Lookup

                //Stocking Product
                var erProduct = image.GetAttributeValue<EntityReference>("productid");
                #region query Product > Stocking Product
                var fetchXmlProduct = string.Empty;
                fetchXmlProduct += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
                fetchXmlProduct += "  <entity name='product'>";
                fetchXmlProduct += "    <attribute name='blu_stockingproduct'/>";
                fetchXmlProduct += "    <filter type='and'>";
                fetchXmlProduct += "      <condition attribute='productid' operator='eq' value='" + erProduct.Id.ToString() + "'/>";
                fetchXmlProduct += "    </filter>";
                fetchXmlProduct += " </entity>";
                fetchXmlProduct += "</fetch>";
                

                EntityCollection ecProduct = service.RetrieveMultiple(new FetchExpression(fetchXmlProduct));

                if (ecProduct.Entities.Count > 0)
                {
                    foreach (var enProduct in ecProduct.Entities)
                    {
                        stockingProductId = enProduct.GetAttributeValue<EntityReference>("blu_stockingproduct").Id;
                    }
                }
                #endregion

                //Order Property
                var erSalesOrder = image.GetAttributeValue<EntityReference>("salesorderid");
                #region query Sales Order > Property
                var fetchXmlSalesOrder = string.Empty;
                fetchXmlSalesOrder += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
                fetchXmlSalesOrder += "  <entity name='salesorder'>";
                fetchXmlSalesOrder += "    <attribute name='blu_regardingpropertyid'/>";
                fetchXmlSalesOrder += "    <filter type='and'>";
                fetchXmlSalesOrder += "      <condition attribute='salesorderid' operator='eq' value='" + erSalesOrder.Id.ToString() + "'/>";
                fetchXmlSalesOrder += "      <condition attribute='statecode' operator='eq' value='0'/>";
                fetchXmlSalesOrder += "    </filter>";
                fetchXmlSalesOrder += " </entity>";
                fetchXmlSalesOrder += "</fetch>";

                EntityCollection ecSalesOrder = service.RetrieveMultiple(new FetchExpression(fetchXmlSalesOrder));

                if (ecSalesOrder.Entities.Count > 0)
                {
                    foreach (var enSalesOrder in ecSalesOrder.Entities)
                    {
                        propertyId = enSalesOrder.GetAttributeValue<EntityReference>("blu_regardingpropertyid").Id;
                    }
                }
                #endregion


                if (stockingProductId != Guid.Empty & propertyId != Guid.Empty)
                {
                    //Check if Active Product Inventory exists for this product
                    #region query and associate Product Inventory
                    var fetchXmlProductInventory = string.Empty;
                    fetchXmlProductInventory += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
                    fetchXmlProductInventory += "  <entity name='blu_productinventory'>";
                    fetchXmlProductInventory += "    <attribute name='blu_productinventoryid'/>";
                    fetchXmlProductInventory += "    <filter type='and'>";
                    fetchXmlProductInventory += "      <condition attribute='blu_property' operator='eq' value='" + propertyId.ToString() + "'/>";
                    fetchXmlProductInventory += "      <condition attribute='blu_reporttype' operator='eq' value='" + stockingProductId.ToString() + "'/>";
                    fetchXmlProductInventory += "      <condition attribute='statecode' operator='eq' value='0'/>";
                    fetchXmlProductInventory += "    </filter>";
                    fetchXmlProductInventory += " </entity>";
                    fetchXmlProductInventory += "</fetch>";

                    EntityCollection ecProductInventory = service.RetrieveMultiple(new FetchExpression(fetchXmlProductInventory));

                    if (ecProductInventory.Entities.Count > 0)
                    {
                        foreach (var enProductInventory in ecProductInventory.Entities)
                        {
                            Entity enSalesOrderProduct = new Entity(context.PrimaryEntityName)
                            {
                                ["salesorderdetailid"] = context.PrimaryEntityId,
                                ["blu_productinventory"] = new EntityReference("blu_productinventory", enProductInventory.Id)
                            };
                            service.Update(enSalesOrderProduct);
                        }
                    }
                }
                #endregion
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
