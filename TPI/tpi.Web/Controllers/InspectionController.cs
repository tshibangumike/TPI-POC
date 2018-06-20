using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class InspectionController : BaseController
    {

        public ActionResult GetInspectionsByAddress(string searchText)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var inspections = InspectionService.QueryInspectionByAddress(GetHttpClient(), searchText);

                return Json(inspections, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInspectionByAddressParts(string searchText, string unitNumber,
            string streetNumber, string streetAddress, string subLocality, string suburb, string city, string state,
            string country, string postalCode)
        {
            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var inspections = InspectionService.QueryInspectionByAddressParts(GetHttpClient(), searchText, unitNumber,
             streetNumber, streetAddress, subLocality, suburb, city, state,
             country, postalCode);

                return Json(inspections, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInspectionById(Guid inspectionId)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var inspection = InspectionService.QueryInspection(GetHttpClient(), inspectionId);
                return Json(inspection, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetCustomerOrders(Guid customerId)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var inspectionDetails = InspectionService.QueryCustomerOrders(GetHttpClient(), customerId);
                return Json(inspectionDetails, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetInspectionDetailsByStateCodeByInspection(int stateCode, Guid inspectionId)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var inspections = InspectionService.QueryInspectionDetailsByStateCodeByInspection(httpClient, stateCode, inspectionId);

                return Json(inspections, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CreateInspection(Property inspection)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var newInspectionId = InspectionService.CreateProperty(httpClient, inspection);

                return Json(newInspectionId, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CreateInspectionDetail(Inspection inspectionDetail)
        {
            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var newInspectionDetailId = InspectionService.CreateInspection(httpClient, inspectionDetail);

                return Json(newInspectionDetailId, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CreateInspectionDetails(List<Inspection> inspectionDetails)
        {

            HttpClient httpClient = null;

            try
            {

                httpClient = GetHttpClient();

                var newInspectionDetailIds = new List<Guid>();

                foreach (var inspectionDetail in inspectionDetails)
                {

                    var newInspectionDetailId = InspectionService.CreateInspection(httpClient, inspectionDetail);
                    newInspectionDetailIds.Add(newInspectionDetailId);

                }
                return Json(newInspectionDetailIds, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = this.GetType().Name + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
 