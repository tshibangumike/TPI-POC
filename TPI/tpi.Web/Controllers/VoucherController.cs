using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class VoucherController : BaseController
    {

        public JsonResult GetVoucherByVoucherNumberByCustomer(string voucherNumber, Guid customerId)
        {
            var vouchers =
                VoucherService.QueryVouchersByVoucherNumberByCustomer(GetHttpClient(), voucherNumber, customerId);
            return Json(vouchers, JsonRequestBehavior.AllowGet);
        }

    }
}