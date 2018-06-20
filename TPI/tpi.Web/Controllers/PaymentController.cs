using System;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class PaymentController : BaseController
    {

        public JsonResult HasPaymentBeenMade(string paymentReference, string orderNumnber)
        {
            var hasPaymentBeenMade = PaymentService.HasPaymentBeenMade(GetHttpClient(), paymentReference, orderNumnber);
            return Json(hasPaymentBeenMade, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreatePayment(Payment payment)
        {

            var paymentId = PaymentService.CreatePayment(GetHttpClient(), payment);

            if(paymentId == Guid.Empty) return Json("error", JsonRequestBehavior.AllowGet);

            var invoiceId = OrderService.ConvertOrderToInvoice(GetService(), payment.OrderId);

            //OrderService.SetOrderState(GetService(), payment.OrderId);

            return Json(invoiceId, JsonRequestBehavior.AllowGet);

        }

    }
}