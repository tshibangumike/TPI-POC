using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Web.Mvc;
using tpi.Model;
using tpi.Service;
using tpi.SharePointConnector;

namespace tpi.Web.Controllers
{
    public class AccountController : BaseController
    {

        public ActionResult CreateContact(Customer customer, PortalUser portalUser)
        {

            var httpClient = GetHttpClient();
            var retailPriceList = PriceListService.QueryDefaultPriceList(httpClient);
            if (retailPriceList != null)
            {
                customer.PriceListId = retailPriceList.Id;
            }

            var customerId = CustomerService.CreateContact(httpClient, customer);

            portalUser.CustomerId = customerId;
            portalUser.Name = customer.Firstname + " " + customer.Lastname;

            var portalUserId = PortalUserService.CreatePortalUser(httpClient, portalUser, (int)CustomerType.Contact);

            return Json(portalUserId, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateAccount(Customer customer, PortalUser portalUser)
        {

            //var httpClient = GetHttpClient();
            //var retailPriceList = PriceListService.QueryDefaultPriceList(httpClient);
            //if (retailPriceList != null)
            //{
            //    customer.PriceListId = retailPriceList.Id;
            //}

            var service = GetService();

            var customerId = CustomerService.CreateAccount(service, customer);

            portalUser.CustomerId = customerId;
            portalUser.Name = customer.Name;

            var portalUserId = PortalUserService.CreatePortalUser(service, portalUser);

            return Json(portalUserId, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCustomer(Guid customerId, int portalUserRole, string customerTypeName)
        {

            Customer customer = null;
            switch (customerTypeName)
            {
                case "contact":
                    customer = CustomerService.QueryContact(GetHttpClient(), customerId);
                    break;
                case "account":
                    customer = CustomerService.QueryAccount(GetHttpClient(), customerId);
                    break;
            }

            return Json(customer, JsonRequestBehavior.AllowGet);

        }

        public JsonResult UpdateCustomer(Customer customer)
        {
            var returnValue = CustomerService.UpdateCustomer(GetHttpClient(), customer);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DownloadReport(string jobNumber, string id, string documentName)
        {

            try
            {

                var folderName = jobNumber + "_" + id.Replace("-", "").ToUpper();

                var context = SharePointConnect.GetClientContext();

                if (context != null)
                {

                    // Create CAML query to get Document

                    var camlQry = @"<View>
                                            <Query> 
                                                <Where>
                                                    <Eq>
                                                        <FieldRef Name='FileLeafRef' />
                                                        <Value Type='File'>" + folderName + @"</Value>
                                                    </Eq>
                                                </Where> 
                                            </Query> 
                                            <ViewFields>
                                                <FieldRef Name='Title'/>
                                                <FieldRef Name='FileRef'/>
                                            </ViewFields>
                                        </View>";

                    CamlQuery camlQuery = new CamlQuery();

                    camlQuery.ViewXml = camlQry;

                    // Add Query to Call for SharePoint List

                    List lstDocument = context.Web.Lists.GetByTitle("Inspection Detail");

                    ListItemCollection lstDocumentDetail = lstDocument.GetItems(camlQuery);

                    context.Load(lstDocumentDetail);

                    context.ExecuteQuery();

                    if (lstDocumentDetail != null)
                    {

                        foreach (var lstItem in lstDocumentDetail)
                        {
                            var fileRef = lstItem["FileRef"];
                            var fullFilePath = fileRef.ToString() + "/" + documentName;
                            FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(context, fullFilePath);
                            using (var memory = new MemoryStream())
                            {

                                //// Reading context of file into memoryStream

                                byte[] buffer = new byte[1024 * 64];
                                int nread = 0;
                                while ((nread = fileInfo.Stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    memory.Write(buffer, 0, nread);
                                }
                                memory.Seek(0, SeekOrigin.Begin);

                                // send the PDF file to browser

                                byte[] byteArray = memory.ToArray();

                                var fileData = Convert.ToBase64String(byteArray);

                                return Json(fileData, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }


        }

    }
}