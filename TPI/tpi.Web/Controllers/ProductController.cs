using System;
using System.Collections.Generic;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class ProductController : BaseController
    {
        public ActionResult GetProductsByInspection(Guid inspectionId)
        {
            var products = ProductService.QueryProductsByInspection(GetHttpClient(), inspectionId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        /******************************/

        public JsonResult GetCartItemsByInspection(Guid inspectionId, List<PriceList> priceLists)
        {
            var cartItems = CartItemService.QueryCartItemByInspection(GetHttpClient(), inspectionId, priceLists);
            return Json(cartItems, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPriceListItemsByPriceLists(List<PriceList> priceLists)
        {
            var cartItems = CartItemService.QueryPriceListItemsByPriceLists(GetHttpClient(), priceLists);
            return Json(cartItems, JsonRequestBehavior.AllowGet);
        }

    }
}