using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using tpi.CrmConnector;

namespace tpi.Web.Controllers
{
    public class CrmController : Controller
    {

        public ActionResult GetInspectionsByAddress(string searchText)
        {
            searchText = "%" + searchText + "%";
            StringBuilder linkFetch = new StringBuilder();
            linkFetch.Append("<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"true\">");
            linkFetch.Append("  <entity name=\"blu_inspection\">");
            linkFetch.Append("      <attribute name=\"blu_inspectionid\"/>");
            linkFetch.Append("      <attribute name=\"blu_inspectionaddress\"/>");
            linkFetch.Append("      <filter type=\"and\">");
            linkFetch.Append("          <condition attribute=\"blu_inspectionaddress\" operator=\"like\" value=\"" + searchText + "\"/>");
            linkFetch.Append("      </filter>");
            linkFetch.Append("  </entity>");
            linkFetch.Append("</fetch>");

            // Build fetch request and obtain results.
            RetrieveMultipleRequest efr = new RetrieveMultipleRequest()
            {
                Query = new FetchExpression(linkFetch.ToString())
            };
            EntityCollection entityResults = ((RetrieveMultipleResponse)CrmConnect.GetService().Execute(efr)).EntityCollection;
            return Json(entityResults.Entities, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInspectionDetailByInspectionId(Guid inspectionId)
        {
            StringBuilder linkFetch = new StringBuilder();
            linkFetch.Append("<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"true\">");
            linkFetch.Append("  <entity name=\"blu_inspectiondetail\">");
            linkFetch.Append("      <attribute name=\"blu_inspectiondetailid\"/>");
            linkFetch.Append("      <attribute name=\"blu_name\"/>");
            linkFetch.Append("      <attribute name=\"blu_productid\"/>");
            linkFetch.Append("      <attribute name=\"blu_inspectionid\"/>");
            linkFetch.Append("      <filter type=\"blu_inspection\">");
            linkFetch.Append("          <attribute name=\"blu_inspectionid\" operator=\"eq\" value=\"" + inspectionId + "\"/>");
            linkFetch.Append("      </filter>");
            linkFetch.Append("  </entity>");
            linkFetch.Append("</fetch>");
            // Build fetch request and obtain results.
            RetrieveMultipleRequest efr = new RetrieveMultipleRequest()
            {
                Query = new FetchExpression(linkFetch.ToString())
            };
            EntityCollection entityResults = ((RetrieveMultipleResponse)CrmConnect.GetService().Execute(efr)).EntityCollection;
            return Json(entityResults.Entities, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPriceListsProductsInspections()
        {
            StringBuilder linkFetch = new StringBuilder();
            linkFetch.Append("<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"true\">");
            linkFetch.Append("  <entity name=\"productpricelevel\">");
            linkFetch.Append("      <attribute name=\"productid\"/>");
            linkFetch.Append("      <attribute name=\"amount\"/>");
            linkFetch.Append("      <attribute name=\"uomid\"/>");
            linkFetch.Append("      <link-entity name=\"product\" from=\"productid\" to=\"productid\" link-type=\"outer\" alias=\"pp\">");
            linkFetch.Append("          <attribute name=\"parentproductid\"/>");
            linkFetch.Append("          <attribute name=\"name\"/>");
            linkFetch.Append("      </link-entity>");
            linkFetch.Append("      <link-entity name=\"pricelevel\" from=\"pricelevelid\" to=\"pricelevelid\" link-type=\"outer\" alias=\"pl\">");
            linkFetch.Append("          <attribute name=\"pricelevelid\"/>");
            linkFetch.Append("          <attribute name=\"name\"/>");
            linkFetch.Append("      </link-entity>");
            linkFetch.Append("  </entity>");
            linkFetch.Append("</fetch>");

            // Build fetch request and obtain results.
            RetrieveMultipleRequest efr = new RetrieveMultipleRequest()
            {
                Query = new FetchExpression(linkFetch.ToString())
            };
            EntityCollection entityResults = ((RetrieveMultipleResponse)CrmConnect.GetService().Execute(efr)).EntityCollection;
            return Json(entityResults.Entities, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInspectionData(string address)
        {
            address = "%" + address + "%";
            StringBuilder linkFetch = new StringBuilder();
            linkFetch.Append("<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"true\">");
            linkFetch.Append("  <entity name=\"blu_inspectiondetail\">");
            linkFetch.Append("      <attribute name=\"blu_inspectiondetailid\"/>");
            linkFetch.Append("      <attribute name=\"blu_productid\"/>");
            linkFetch.Append("      <attribute name=\"blu_inspectionid\"/>");
            linkFetch.Append("      <link-entity name=\"blu_inspection\" from=\"blu_inspectionid\" to=\"blu_inspectionid\" link-type=\"inner\" alias=\"aa\">");
            linkFetch.Append("          <attribute name=\"blu_inspectionaddress\"/>");
            linkFetch.Append("          <filter type=\"and\">");
            linkFetch.Append("              <condition attribute=\"blu_inspectionaddress\" operator=\"like\" value=\"" + address + "\"/>");
            linkFetch.Append("          </filter>");
            linkFetch.Append("      </link-entity>");
            linkFetch.Append("  </entity>");
            linkFetch.Append("</fetch>");

            // Build fetch request and obtain results.
            RetrieveMultipleRequest efr = new RetrieveMultipleRequest()
            {
                Query = new FetchExpression(linkFetch.ToString())
            };
            EntityCollection entityResults = ((RetrieveMultipleResponse)CrmConnect.GetService().Execute(efr)).EntityCollection;
            return Json(entityResults.Entities, JsonRequestBehavior.AllowGet);
        }

    }
}