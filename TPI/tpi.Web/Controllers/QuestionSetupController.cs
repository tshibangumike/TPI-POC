using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class QuestionSetupController : BaseController
    {
        
        public JsonResult GetQuestionSetupByQuestionSetup(List<Guid> productIds, Guid priceListId)
        {
            var records = QuestionSetupService.QueryQuestionSetupByQuestionSetup(GetHttpClient(), productIds, priceListId);
            return Json(records, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTermsAndConditionsByQuestionSetup(Guid questionSetupId)
        {
            var records = QuestionSetupService.QueryQuestionSetupByQuestionSetup(GetHttpClient(), questionSetupId);
            return Json(records, JsonRequestBehavior.AllowGet);
        }

    }
}