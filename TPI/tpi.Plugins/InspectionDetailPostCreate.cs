using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace tpi.Plugins
{
    public class InspectionDetailPostCreate : IPlugin
    {
        private Guid salesOrderId = Guid.Empty;
        
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var entity = ((Entity)context.InputParameters["Target"]);

            try
            {
                if (!entity.Attributes.Contains("blu_productid"))
                {
                    tracingService.Trace("No Product found", null);
                    return;
                }

                var productEr = (EntityReference)entity["blu_productid"];
                
                #region Update Sales Order Product Inspection Lookup
                salesOrderId = entity.GetAttributeValue<EntityReference>("blu_orderid").Id;

                #region query Sales Order Product
                var fetchXmlSalesOrderProduct = string.Empty;
                fetchXmlSalesOrderProduct += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXmlSalesOrderProduct += "  <entity name='salesorderdetail'>";
                fetchXmlSalesOrderProduct += "    <attribute name='salesorderdetailid'/>";
                fetchXmlSalesOrderProduct += "    <filter type='and'>";
                fetchXmlSalesOrderProduct += "      <condition attribute='productid' operator='eq' value='" + productEr.Id.ToString() + "'/>";
                fetchXmlSalesOrderProduct += "      <condition attribute='salesorderid' operator='eq' value='" + salesOrderId.ToString() + "'/>";
                fetchXmlSalesOrderProduct += "      <condition attribute='blu_inspection' operator='null'/>";
                fetchXmlSalesOrderProduct += "    </filter>";
                fetchXmlSalesOrderProduct += "    <link-entity name='salesorder' from='salesorderid' to='salesorderid' link-type='inner'>";
                fetchXmlSalesOrderProduct += "      <filter type='and'>";
                fetchXmlSalesOrderProduct += "         <condition attribute='statecode' operator='eq' value='0'/>";
                fetchXmlSalesOrderProduct += "      </filter>";
                fetchXmlSalesOrderProduct += "    </link-entity>";
                fetchXmlSalesOrderProduct += "  </entity>";
                fetchXmlSalesOrderProduct += "</fetch>";
                #endregion

                EntityCollection ecSalesOrderProduct = service.RetrieveMultiple(new FetchExpression(fetchXmlSalesOrderProduct));

                if (ecSalesOrderProduct.Entities.Count > 0)
                {
                    foreach (var enSalesOrderProduct in ecSalesOrderProduct.Entities)
                    {
                        enSalesOrderProduct["blu_inspection"] = new EntityReference("blu_inspectiondetail", entity.Id);
                        //Logging(service, "Associate Inspection to Sales Order Product", String.Empty, "Inspection | Create", new OptionSetValue(2));
                        service.Update(enSalesOrderProduct);
                    }
                }
                #endregion

                #region create Inspection Cateogries
                #region query Question - Category
                var fetchXmlCategory = string.Empty;
                fetchXmlCategory += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>";
                fetchXmlCategory += "  <entity name='blu_questioncategory'>";
                fetchXmlCategory += "     <attribute name='blu_questioncategoryid'/>";
                fetchXmlCategory += "     <attribute name='blu_name'/>"; 
                fetchXmlCategory += "     <link-entity name='blu_questionquestion' from='blu_category' to='blu_questioncategoryid' link-type='inner'>'";
                fetchXmlCategory += "       <link-entity name='blu_blu_questionquestion_product' from='blu_questionquestionid' to='blu_questionquestionid' link-type='inner' intersect='true'>";
                fetchXmlCategory += "         <link-entity name='product' from='blu_stockingproduct' to='productid' link-type='inner'>";
                fetchXmlCategory += "           <filter type='and'>";
                fetchXmlCategory += "             <condition attribute='productid' operator='eq' value='" + productEr.Id.ToString() + "'/>";
                fetchXmlCategory += "           </filter>";
                fetchXmlCategory += "         </link-entity>";
                fetchXmlCategory += "       </link-entity>";
                fetchXmlCategory += "     </link-entity>";
                fetchXmlCategory += "  </entity>";
                fetchXmlCategory += "</fetch>";
                #endregion

                EntityCollection ecCategory = service.RetrieveMultiple(new FetchExpression(fetchXmlCategory));
                if (ecCategory.Entities.Count > 0)
                {
                    foreach (var enCategory in ecCategory.Entities)
                    {
                        var enInspectionCategory = new Entity("blu_inspectioncategory")
                        {
                            ["blu_name"] = enCategory.GetAttributeValue<String>("blu_name"),
                            ["blu_inspectionid"] = new EntityReference("blu_inspectiondetail", entity.Id),
                            ["blu_setupcategory"] = new EntityReference("blu_questioncategory", enCategory.Id)
                        };
                        service.Create(enInspectionCategory);
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
                tracingService.Trace("InspectionDetailPostCreate: {0}", ex.ToString(), null);
                throw;
            }
        }
    }
}
