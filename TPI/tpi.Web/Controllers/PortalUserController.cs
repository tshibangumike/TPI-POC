using System;
using System.Collections.Generic;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class PortalUserController : BaseController
    {

        public JsonResult DoesUsernameExist(string username)
        {
            var existingPortalUser = PortalUserService.QueryPortalUserByUsername(GetHttpClient(), username);
            return Json(existingPortalUser != null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidatePortalUser(string username, string password)
        {

            var httpClient = GetHttpClient();

            var portalUser = PortalUserService.QueryPortalUserByUsernameByPassword(httpClient, username, password);
            return Json(portalUser, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetPortalUser(string username, string password)
        {

            var httpClient = GetHttpClient();

            var portalUser = PortalUserService.QueryPortalUserByUsernameByPassword(httpClient, username, password);
            if (portalUser == null) return Json("error", JsonRequestBehavior.AllowGet);

            if(portalUser.PortalUserRole == 0) return Json("error:Portaluserrole attribute of portal user object is null!", JsonRequestBehavior.AllowGet);

            List<PriceList> portalUserPriceLists;

            switch (portalUser.PortalUserRole)
            {
                case (int)PortalUserrole.Consumer:
                    portalUserPriceLists = PriceListService.QueryPriceListByPortalUserRole(httpClient, portalUser.PortalUserRole);
                    portalUser.PriceLists.AddRange(portalUserPriceLists);
                    break;
                case (int)PortalUserrole.Vendor:
                    if(portalUser.CustomerId == Guid.Empty)
                        return Json("error:CustomerId attribute of portal user object is null!", JsonRequestBehavior.AllowGet);
                    portalUserPriceLists = PriceListService.QueryPriceListByCustomer(GetHttpClient(), portalUser.CustomerId);
                    if (portalUserPriceLists == null || portalUserPriceLists.Count == 0)
                    {
                        portalUserPriceLists = PriceListService.QueryPriceListByPortalUserRole(httpClient, (int)PortalUserrole.Consumer);
                    }
                    portalUser.PriceLists.AddRange(portalUserPriceLists);
                    break;
            }

            var currentUser = new CurrentUser()
            {
                PortalUser = portalUser
            };

            return Json(currentUser, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCurrentLoggedInUser()
        {
            return Json(GetCurrentUser(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdatePortalUser(PortalUser portalUser)
        {
            var returnValue = PortalUserService.UpdatePassword(GetHttpClient(), portalUser);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Logout()
        {
            //FormsAuthentication.SignOut();
            return Json("", JsonRequestBehavior.AllowGet);
        }

    }
}