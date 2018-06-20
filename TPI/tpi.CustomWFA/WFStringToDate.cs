using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace tpi.CustomWFA
{
    public sealed class WFStringToDate : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            string dateInput = DateInput.Get<string>(executionContext);
            DateTime formattedDate = new DateTime();
            DateTime.TryParse(dateInput, out formattedDate);
            DateOutput.Set(executionContext, formattedDate);
        }

        [Input("Date Text")]
        [RequiredArgument]
        public InArgument<String> DateInput { get; set; }

        [Output("Date Output")]
        public OutArgument<DateTime> DateOutput { get; set; }
    }
}

