using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class PaymentMethodController : BaseController
    {

        public JsonResult GetPaymentMethods()
        {

            var paymentMehods = PaymentMethodService.QueryPaymentMethods(GetHttpClient());

            return Json(paymentMehods, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPaymentMethodsByPriceList(Guid priceListId)
        {

            var httpClient = GetHttpClient();

            var priceList = PriceListService.QueryPriceList(httpClient, priceListId);
            if(priceList == null) return Json(null, JsonRequestBehavior.AllowGet);
            var paymentMehods = PaymentMethodService.QueryPaymentMethodsByCategory(httpClient, priceList.PaymentMethods);

            return Json(paymentMehods, JsonRequestBehavior.AllowGet);
        }

    }
}