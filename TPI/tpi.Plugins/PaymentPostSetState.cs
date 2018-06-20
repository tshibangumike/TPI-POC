using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpi.CrmConnector;
using tpi.Model;

namespace tpi.Plugins
{
    public class PaymentPostSetState : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {

            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var payment = ((Entity)context.InputParameters["Target"]);

            if (!payment.Attributes.Contains("blu_orderid")) return;

            var orderEr = (EntityReference)payment.Attributes["blu_orderid"];

            var columns = new[] { "opportunityid" };
            var conditions = new[] { new ConditionExpression("salesorderid", ConditionOperator.Equal, orderEr.Id) };
            var order = Qe.QueryRecord(service, "salesorder", columns, conditions);

            if (order == null || order.Attributes == null || !order.Attributes.Contains("opportunityid")) return;
            var opportunityEr = (EntityReference)order.Attributes["opportunityid"];

            columns = new[] { "blu_regardingpropertyid" };
            conditions = new[] { new ConditionExpression("opportunityid", ConditionOperator.Equal, opportunityEr.Id) };
            var opportunity = Qe.QueryRecord(service, "opportunity", columns, conditions);

            if (opportunity == null || opportunity.Attributes == null || !opportunity.Attributes.Contains("blu_regardingpropertyid")) return;
            var propertyEr = (EntityReference)opportunity.Attributes["blu_regardingpropertyid"];

            columns = new[] { "productid" };
            conditions = new[] { new ConditionExpression("opportunityid", ConditionOperator.Equal, opportunityEr.Id) };

            var opportunityProducts = Qe.QueryMultipleRecords(service, "opportunityproduct", columns, conditions);

            if (opportunityProducts == null || opportunityProducts.Count == 0) return;
            var opportunityProduct = opportunityProducts[0];

            var productEr = (EntityReference)opportunityProduct.Attributes["productid"];

            columns = new[] { "scheduledstart", "scheduledend", "ownerid" };
            conditions = new[] { new ConditionExpression("regardingobjectid", ConditionOperator.Equal, order.Id) };
            var appointments = Qe.QueryMultipleRecords(service, "appointment", columns, conditions);

            foreach (var appointment in appointments)
            {

                var inspectionDetail = new InspectionDetail()
                {
                    Name = "InspectionDetail | " + productEr.Name + " | " + propertyEr.Name,
                    InspectionId = propertyEr.Id,
                    AppointmentId = appointment.Id,
                    OrderId = order.Id,
                    OwnerId = ((EntityReference)appointment["ownerid"]).Id,
                    ProductId = productEr.Id
                };

                CreateInspectionDetail(service, inspectionDetail);

            }

        }

        public Guid CreateInspectionDetail(IOrganizationService service, InspectionDetail inspectionDetail)
        {
            var newInspection = new Entity("blu_inspectiondetail")
            {
                ["blu_name"] = inspectionDetail.Name,
                ["blu_inspectionid"] = new EntityReference("blu_inspection", inspectionDetail.InspectionId),
                ["blu_productid"] = new EntityReference("product", inspectionDetail.ProductId),
                ["blu_appointmentid"] = new EntityReference("appointment", inspectionDetail.AppointmentId),
                ["blu_orderid"] = new EntityReference("salesorder", inspectionDetail.OrderId),
                ["ownerid"] = new EntityReference("systemuser", inspectionDetail.OwnerId),
                ["blu_productcategory"] = inspectionDetail.ProductCategory
            };

            return service.Create(newInspection);
        }

    }
}
