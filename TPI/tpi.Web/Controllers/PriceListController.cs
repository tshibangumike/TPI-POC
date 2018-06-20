using System;
using System.Collections.Generic;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class PriceListController : BaseController
    {

        //public JsonResult GetPriceListItemsByPriceListsByInspection(PortalUser portalUser, Guid inspectionId)
        //{

        //    var currentUserPriceLists = portalUser.PriceLists;
        //    return Json(null, JsonRequestBehavior.AllowGet);

        //}

        //public JsonResult GetPriceListItemsByPriceLists(List<PriceList> priceLists)
        //{

        //    var priceListItems = PriceListService.QueryPriceListItemsByPriceLists(GetHttpClient(), priceLists);
        //    return Json(priceListItems, JsonRequestBehavior.AllowGet);

        //}

        public JsonResult GetPriceListItemsByPortalUserRole(int portalUserRole)
        {

            var priceListItems = PriceListService.QueryPriceListByPortalUserRole(GetHttpClient(), portalUserRole);
            return Json(priceListItems, JsonRequestBehavior.AllowGet);

        }

    }
}