using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class BankingDetailController : BaseController
    {
        
        public ActionResult GetBankAccountDetails()
        {
            var bankingDetail = BankingDetailService.QueryBankingDetail(GetHttpClient());
            return Json(bankingDetail, JsonRequestBehavior.AllowGet);
        }
    }
}