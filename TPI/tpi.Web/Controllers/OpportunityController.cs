using System;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class OpportunityController : BaseController
    {

        public JsonResult GetOpportunity(Guid opportunityId)
        {

            var opportunity =
                OpportunityService.QueryOpportunity(GetHttpClient(), opportunityId);

            return Json(opportunity, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetOpportunitiesProductsByCustomerIdByInspectionId(Guid opportunityId)
        {

            var opportunities =
                OpportunityService.QueryOpportunityProductsByOpportunityId(GetHttpClient(), opportunityId);

            return Json(opportunities, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CreateOpportunity(Opportunity opportunity)
        {

            //var newOpportunityId = OpportunityService.CreateOpporutnity(GetService(), opportunity);
            var newOpportunityId = OpportunityService.CreateOpporutnity(GetHttpClient(), opportunity);

            return Json(newOpportunityId, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CreateOpportunityProduct(OpportunityProduct opportunityProduct)
        {

            var newOpportunityProductId =
                OpportunityService.CreateOpportunityProduct(GetHttpClient(), opportunityProduct);

            return Json(newOpportunityProductId, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CreateOpportunityAndOpportunityProduct(Opportunity opportunity,
            OpportunityProduct opportunityProduct)
        {

            var httpClient = GetHttpClient();

            var newOpportunityId = OpportunityService.CreateOpporutnity(httpClient, opportunity);
            opportunityProduct.OpportunityId = newOpportunityId;

            if(newOpportunityId == Guid.Empty)
            {
                return Json(newOpportunityId, JsonRequestBehavior.AllowGet);
            }

            var newOpportunityProductId =
                OpportunityService.CreateOpportunityProduct(httpClient, opportunityProduct);

            return Json(newOpportunityId, JsonRequestBehavior.AllowGet);

        }

        public JsonResult ApplyDiscount(Opportunity opportunity, Guid voucherId)
        {

            var httpClient = GetHttpClient();
            var success = OpportunityService.UpdateOpportunity(httpClient, opportunity);
            VoucherService.SetVoucherState(GetService(), voucherId);
            return Json(success, JsonRequestBehavior.AllowGet);

        }

    }
}