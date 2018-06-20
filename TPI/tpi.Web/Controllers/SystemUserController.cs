using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class SystemUserController : BaseController
    {

        public JsonResult GetSystemUsers()
        {

            var systemUsers = SystemUserService.QuerySystemUsers(GetHttpClient());
            return Json(systemUsers, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetSystemUsersBySkills(string[] skills)
        {

            var systemUsers = SystemUserService.QuerySystemUsersBySkills(GetHttpClient(), skills);
            return Json(systemUsers, JsonRequestBehavior.AllowGet);

        }

    }
}