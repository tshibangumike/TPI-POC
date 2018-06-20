using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace tpi.Plugins
{

    public class VoucherNumberGenerator : IPlugin
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

                CreateVoucherNumber(service, entity);

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in the VoucherNumberGenerator plug-in.", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("VoucherNumberGenerator: {0}", ex.ToString());
                throw;
            }

        }


        public void CreateVoucherNumber(IOrganizationService service, Entity entity)
        {

            string newVoucherNumber = string.Empty;
            while (newVoucherNumber == string.Empty)
            {
                newVoucherNumber = RandomString(8);
                QueryExpression query = new QueryExpression("blu_voucher");
                query.ColumnSet = new ColumnSet(new string[] { "blu_vouchernumber" });
                query.Criteria.AddCondition("blu_vouchernumber", ConditionOperator.Equal, newVoucherNumber);
                query.TopCount = 1;
                var crmMeta = service.RetrieveMultiple(query);
                if (crmMeta.Entities.Count > 0)
                    newVoucherNumber = string.Empty;
                entity["blu_vouchernumber"] = newVoucherNumber;
            }
        }

        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

}
