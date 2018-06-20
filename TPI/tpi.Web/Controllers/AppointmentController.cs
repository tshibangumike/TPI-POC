  using System;
using System.Collections.Generic;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class AppointmentController : BaseController
    {

        public JsonResult GetAppointment(Guid appointmentId)
        {

            var appointment = AppointmentService.QueryAppointment(GetHttpClient(), appointmentId);

            return Json(appointment, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetInspectorsAppointments(int numberOfInspectors)
        {

            var appointment = AppointmentService.QueryInspectorsAppointments(GetHttpClient(), numberOfInspectors);

            return Json(appointment, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetAppointmentsByInspector(Guid inspectorId)
        {

            var appointment = AppointmentService.QueryAppointmentsByInspector(GetHttpClient(), inspectorId);

            return Json(appointment, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateAppointments(List<Appointment> appointments)
        {

            var appointmentIds = new List<Guid>();

            foreach (var appointment in appointments)
            {
                var appointmentId = AppointmentService.CreateAppointment(GetHttpClient(), GetService(), appointment);
                appointmentIds.Add(appointmentId);
            }

            return Json(appointmentIds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateAppointment(Appointment appointment)
        {

            var appointmentId = AppointmentService.CreateAppointment(GetService(), appointment);

            return Json(appointmentId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProceedWithCheckout2(List<CombinedPropertyObject> combinedPropertyObjects, List<InspectionPortalQA> questions)
        {

            var httpClient = GetHttpClient();
            var service = GetService();

            var orderId = Guid.Empty;
            Order newlyCreatedOrder = null;

            var existingOrder = OrderService.QueryOrderByOpportunity(httpClient, combinedPropertyObjects[0].OpportunityId);
            if (existingOrder == null)
            {
                orderId = OpportunityService.ConvertOpportunityToSalesOrder(GetService(), combinedPropertyObjects[0].OpportunityId);
            }
            else
            {
                orderId = existingOrder.Id;
            }

            newlyCreatedOrder = OrderService.QueryOrder(httpClient, orderId);
            if (newlyCreatedOrder == null)
            {
                return Json("Error occured while retrieveing order with id: " + orderId, JsonRequestBehavior.AllowGet);
            }

            var opportunity = OpportunityService.QueryOpportunity(GetHttpClient(), combinedPropertyObjects[0].OpportunityId);
            if (opportunity.StateCode == 0)
            {
                OpportunityService.SetOpportunityState(GetService(), combinedPropertyObjects[0].OpportunityId);
            }

            var newPropertyId = Guid.Empty;
            var newInspectionDetailId = Guid.Empty;

            foreach (var cpo in combinedPropertyObjects)
            {

                if (cpo.Apppointment != null)
                {
                    var newAppointmentId = AppointmentService.CreateAppointment(httpClient, service, cpo.Apppointment);
                    cpo.Inspection.AppointmentId = newAppointmentId;


                    cpo.Inspection.OrderId = orderId;
                    if (cpo.DoCreateAnInspection)
                    {
                        if (newPropertyId == Guid.Empty)
                        {
                            newPropertyId = InspectionService.CreateProperty(httpClient, cpo.Property);
                            cpo.Inspection.PropertyId = newPropertyId;
                        }
                        else
                        {
                            cpo.Inspection.PropertyId = newPropertyId;
                        }
                    }
                    else
                    {
                        cpo.Inspection.PropertyId = cpo.Property.Id;
                    }
                    newInspectionDetailId = InspectionService.CreateInspection(httpClient, cpo.Inspection);

                }

                if (questions == null || questions.Count == 0) continue;

                var questionCount = 0;
                foreach (var question in questions)
                {
                    if (question == null) continue;
                    question.InspectionId = newInspectionDetailId;
                    question.OrderId = orderId;
                    question.PropertyId = newPropertyId == Guid.Empty ? cpo.Property.Id : newPropertyId;
                    question.OpportunityId = opportunity.Id;
                    QuestionSetupService.CreateInspectionPortalQA(httpClient, question);
                    questionCount++;
                    if (question.AdditionalProductId != Guid.Empty && (question.AnswerDataTypeText == "Boolean (Yes/No)" || question.AnswerDataTypeText == "Boolean (True/False)"))
                    {
                        if (question.Answer != "Yes") continue;

                        var product = ProductService.QueryProduct(httpClient, question.AdditionalProductId);

                        if (product == null) continue;

                        var orderProduct = new OrderProduct()
                        {
                            OrderId = orderId,
                            Amount = product.BuyerPays,
                            ProductId = product.Id,
                            UomId = product.UomId
                        };

                        OrderService.CreateOrderProduct(httpClient, orderProduct);

                    }
                }

            }

            return Json(newlyCreatedOrder, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ProceedWithCheckout(List<CombinedPropertyObject> combinedPropertyObjects, List<InspectionPortalQA> questions)
        {

            var httpClient = GetHttpClient();
            var service = GetService();

            var orderId = Guid.Empty;
            var propertyId = Guid.Empty;
            var inspectionDetailId = Guid.Empty;

            #region Create Property
            var firstItem = combinedPropertyObjects[0];
            if (firstItem.DoCreateAnInspection)
            {
                if (propertyId == Guid.Empty)
                {
                    propertyId = InspectionService.CreateProperty(httpClient, firstItem.Property);
                    firstItem.Inspection.PropertyId = propertyId;
                }
                else
                {
                    firstItem.Inspection.PropertyId = propertyId;
                }
            }
            else
            {
                propertyId = firstItem.Property.Id;
                //firstItem.Inspection.PropertyId = propertyId;
            }
            #endregion

            /*Query order. If order does not exist, then conver opportunity to order*/
            var existingOrder = OrderService.QueryOrderByOpportunity(httpClient, combinedPropertyObjects[0].OpportunityId);
            if (existingOrder == null)
            {
                orderId = OpportunityService.ConvertOpportunityToSalesOrder(service, combinedPropertyObjects[0].OpportunityId);
            }
            else
            {
                orderId = existingOrder.Id;
            }

            #region Opportunity Update
            var opportunity = OpportunityService.QueryOpportunity(httpClient, firstItem.OpportunityId);
            if (opportunity != null)
            {
                opportunity.PropertyId = propertyId;
                OpportunityService.UpdateOpportunity(httpClient, opportunity);
                if (opportunity.StateCode == 0)
                {
                    OpportunityService.SetOpportunityState(service, firstItem.OpportunityId);
                }
            }
            #endregion

            #region Appointments, Inspection
            foreach (var cpo in combinedPropertyObjects)
            {
                if (cpo.Apppointment == null)
                {
                    continue;
                }
                var newAppointmentId = AppointmentService.CreateAppointment(httpClient, service, cpo.Apppointment);
                cpo.Inspection.AppointmentId = newAppointmentId;
                cpo.Inspection.OrderId = orderId;
                cpo.Inspection.PropertyId = propertyId;
                inspectionDetailId = InspectionService.CreateInspection(httpClient, cpo.Inspection);
            }
            #endregion

            #region Order

            var newlyCreatedOrder = OrderService.QueryOrder(httpClient, orderId);
            if (newlyCreatedOrder == null)
            {
                return Json("Error occured while retrieveing order with id: " + orderId, JsonRequestBehavior.AllowGet);
            }
            newlyCreatedOrder.PropertyId = propertyId;
            OrderService.UpdateOrder(httpClient, newlyCreatedOrder);
            #endregion

            #region Questions
            if (questions != null)
            {
                foreach (var question in questions)
                {
                    if (question == null) continue;
                    question.InspectionId = inspectionDetailId;
                    question.OrderId = orderId;
                    question.PropertyId = propertyId;
                    question.OpportunityId = opportunity.Id;
                    QuestionSetupService.CreateInspectionPortalQA(httpClient, question);
                    if (question.AdditionalProductId != Guid.Empty && (question.AnswerDataTypeText == "Boolean (Yes/No)" || question.AnswerDataTypeText == "Boolean (True/False)"))
                    {
                        if (question.Answer != "Yes") continue;
                        var product = ProductService.QueryProduct(httpClient, question.AdditionalProductId);
                        if (product == null) continue;
                        var orderProduct = new OrderProduct()
                        {
                            OrderId = orderId,
                            Amount = product.BuyerPays,
                            ProductId = product.Id,
                            UomId = product.UomId
                        };
                        OrderService.CreateOrderProduct(httpClient, orderProduct);
                    }
                }
            }
            #endregion

            return Json(newlyCreatedOrder, JsonRequestBehavior.AllowGet);

        }

    }
}