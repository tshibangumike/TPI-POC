using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using eWAY.Rapid;
using eWAY.Rapid.Enums;
using eWAY.Rapid.Models;
using tpi.Model;
using tpi.Service;

namespace tpi.Web.Controllers
{
    public class TransactionController : BaseController
    {

        public JsonResult EwayTransaction(Order order)
        {

            var transaction = new Transaction()
            {
                PaymentDetails = new PaymentDetails()
                {
                    TotalAmount = (int)order.TotalAmount * 100,
                    InvoiceNumber = order.OrderNumber,
                    InvoiceDescription = order.Id.ToString()
                },
                SaveCustomer = true,
                RedirectURL = ConfigurationManager.AppSettings["redirectURL"],
                CancelURL = ConfigurationManager.AppSettings["cancelURL"],
                TransactionType = TransactionTypes.Purchase
            };
            var response = GeteWayClient().Create(eWAY.Rapid.Enums.PaymentMethod.ResponsiveShared, transaction);
            if (response.Errors != null)
            {
                var transactionErrorMessage = string.Empty;
                foreach (var errorCode in response.Errors)
                {
                    transactionErrorMessage += "Error: " + RapidClientFactory.UserDisplayMessage(errorCode, "EN");
                }
                return Json(transactionErrorMessage, JsonRequestBehavior.AllowGet);
            }

            var transactionObject = new TransactionObject()
            {
                Order = order,
                SharedPaymentUrl = response.SharedPaymentUrl
            };

            return Json(transactionObject, JsonRequestBehavior.AllowGet);

        }

        public JsonResult VerifyAccessCode(string accessCode)
        {

            try
            {
               
                var response = GeteWayClient().QueryTransaction(accessCode);
                if(response.Transaction == null)
                {

                    var _returnMessage = "success" + SharedService.GenerateCoupon(6).ToUpper();
                    return Json(_returnMessage, JsonRequestBehavior.AllowGet);
                }
                var returnMessage = string.Empty;
                if (response.TransactionStatus.Status != null && (bool)response.TransactionStatus.Status)
                {
                    returnMessage = "success" + response.TransactionStatus.TransactionID;
                }
                else
                {
                    var errorCodes = response.TransactionStatus.ProcessingDetails.ResponseMessage.Split(new[] { ", " }, StringSplitOptions.None);

                    returnMessage = errorCodes.Aggregate(returnMessage, (current, errorCode) => current + ("Response Message: " + RapidClientFactory.UserDisplayMessage(errorCode, "EN")));
                }
                return Json(returnMessage, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}