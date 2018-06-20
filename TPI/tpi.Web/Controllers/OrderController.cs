using System;
using System.Configuration;
using System.Web.Mvc;
using eWAY.Rapid;
using eWAY.Rapid.Enums;
using eWAY.Rapid.Models;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class OrderController : BaseController
    {

        public JsonResult GetOrder(Guid orderId)
        {

            var order = OrderService.QueryOrder(GetHttpClient(), orderId);
            return Json(order, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetOrderByAccessCode(string accessCode)
        {
            try
            {

                var response = GeteWayClient().QueryTransaction(accessCode);
                if (response.Transaction == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                if (response.TransactionStatus.Status != null && (bool)response.TransactionStatus.Status)
                {

                    var orderNumber =response.Transaction.PaymentDetails.InvoiceNumber;
                    var order = OrderService.QueryOrderByOrderNumber(GetHttpClient(), orderNumber);
                    return Json(order, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ConvertOpportunityToSaleOrder(Guid opportunityId)
        {

            var salesOrderId = OpportunityService.ConvertOpportunityToSalesOrder(GetService(), opportunityId);

            var newlyCreatedOrder = OrderService.QueryOrder(GetHttpClient(), salesOrderId);
            if (newlyCreatedOrder == null || newlyCreatedOrder.TotalAmount == 0)
            {
                return Json("Error occured while retrieveing order with id: " + salesOrderId, JsonRequestBehavior.AllowGet);
            }

            var opportunity = OpportunityService.QueryOpportunity(GetHttpClient(), opportunityId);
            if (opportunity.StateCode == 0)
            {
                OpportunityService.SetOpportunityState(GetService(), opportunityId);

            }

            return Json(newlyCreatedOrder, JsonRequestBehavior.AllowGet);

        }

    }
}