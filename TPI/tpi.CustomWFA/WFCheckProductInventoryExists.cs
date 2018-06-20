using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace tpi.CustomWFA
{
    public sealed class WFCheckProductInventoryExists : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference enProperty = PropertyLookup.Get<EntityReference>(executionContext);
            EntityReference enReportType = ProductLookup.Get<EntityReference>(executionContext);

            #region query Product Inventory
            var fetchXmlProductInventory = string.Empty;
            fetchXmlProductInventory += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>";
            fetchXmlProductInventory += "  <entity name='blu_productinventory'>";
            fetchXmlProductInventory += "     <attribute name='blu_productinventoryid'/>";
            fetchXmlProductInventory += "     <order attribute='createdon' descending='true'/>";
            fetchXmlProductInventory += "     <filter type='and'>";
            fetchXmlProductInventory += "        <condition attribute='blu_property' operator='eq' value='" + enProperty.Id.ToString() + "'/>";
            fetchXmlProductInventory += "        <condition attribute='blu_reporttype' operator='eq' value='" + enReportType.Id.ToString() + "'/>";
            fetchXmlProductInventory += "        <condition attribute='statecode' operator='eq' value='0'/>";
            fetchXmlProductInventory += "     </filter>";
            fetchXmlProductInventory += "  </entity>";
            fetchXmlProductInventory += "</fetch>";
            #endregion

            EntityCollection ecProductInventory = service.RetrieveMultiple(new FetchExpression(fetchXmlProductInventory));
            if (ecProductInventory.Entities.Count > 0)
            {
                foreach (var enProductInventory in ecProductInventory.Entities)
                {
                    ProductInventoryLookup.Set(executionContext, new EntityReference("blu_productinventory", enProductInventory.Id));
                }
            }
        }
        
        [Input("Property Record")]
        [ReferenceTarget("blu_inspection")]
        [RequiredArgument]
        public InArgument<EntityReference> PropertyLookup { get; set; }

        [Input("Product (Report Type) Record")]
        [ReferenceTarget("product")]
        [RequiredArgument]
        public InArgument<EntityReference> ProductLookup { get; set; }

        [Output("Product Inventory")]
        [ReferenceTarget("blu_productinventory")]
        public OutArgument<EntityReference> ProductInventoryLookup { get; set; }
    }
}

