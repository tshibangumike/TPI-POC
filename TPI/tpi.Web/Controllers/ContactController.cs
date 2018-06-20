using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class ContactController : BaseController
    {

        public ActionResult GetInspectionsByAddress()
        {

            var contacts = ContactService.QueryContacts(GetHttpClient());
            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

    }
}