using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace tpi.CustomWFA
{
    public sealed class WFCreateReportImageFromNoteImages : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference enInspectionOption = InspectionOptionLookup.Get<EntityReference>(executionContext);

            #region Query Notes associated to the Inspection - Option
            var fetchXmlInspectionOptionNotes = string.Empty;
            fetchXmlInspectionOptionNotes += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
            fetchXmlInspectionOptionNotes += "  <entity name='annotation'>";
            fetchXmlInspectionOptionNotes += "     <attribute name='annotationid'/>";
            fetchXmlInspectionOptionNotes += "     <attribute name='documentbody'/>";
            fetchXmlInspectionOptionNotes += "     <order attribute='createdon' descending='true'/>";
            fetchXmlInspectionOptionNotes += "     <filter type='and'>";
            fetchXmlInspectionOptionNotes += "        <condition attribute='objectid' operator='eq' value='" + enInspectionOption.Id.ToString() + "'/>";
            fetchXmlInspectionOptionNotes += "        <condition attribute='isdocument' operator='eq' value='1'/>";
            fetchXmlInspectionOptionNotes += "        <condition attribute='subject' operator='not-like' value='ReportImage%'/>";
                fetchXmlInspectionOptionNotes += "     </filter>";
            fetchXmlInspectionOptionNotes += "  </entity>";
            fetchXmlInspectionOptionNotes += "</fetch>";
            #endregion

            EntityCollection ecAnnotation = service.RetrieveMultiple(new FetchExpression(fetchXmlInspectionOptionNotes));
            if (ecAnnotation.Entities.Count > 0)
            {
                CombineImages(service, ecAnnotation, enInspectionOption.Id);
            }

        }

        [Input("Inspection Option Record")]
        [ReferenceTarget("blu_inspectionoption")]
        [RequiredArgument]
        public InArgument<EntityReference> InspectionOptionLookup { get; set; }
        
        private static void CombineImages(IOrganizationService service , EntityCollection ecAnnotation, Guid inspectionOptionId)
        {
            /* Preferred size 
             * 640 pixels x 480 pixels
             */
             
            int nIndex = 0;
            bool newPage = false;
            int width = 0;
            int height = 0;
            int maxColumns = 3;
            int maxRows = 3;
            int maxWidth = 580 / maxColumns;
            int maxHeight = 780 / maxRows;
            int imageSpacing = 5;
            int remainingImages = 0;
            int pageCount = 1;

            //Get Number of Files
            int imageCount = 0;
            foreach (var enAnnotation in ecAnnotation.Entities)
            {
                imageCount++;
            }

            decimal numberRows = Math.Ceiling(Decimal.Divide(imageCount, maxColumns));
            decimal numberPages = Math.Ceiling(Decimal.Divide(numberRows, maxRows));

            while (pageCount <= numberPages)
            {
                if (pageCount == numberPages)
                {
                    //trim whitespace on last image set
                    remainingImages = imageCount - nIndex;
                    numberRows = Math.Ceiling(Decimal.Divide(remainingImages, maxColumns));
                }
                else
                {
                    remainingImages = imageCount;
                }

                if (remainingImages <= maxColumns)
                {
                    width = (maxWidth + imageSpacing) * remainingImages + imageSpacing;
                    height = (maxHeight + imageSpacing) + imageSpacing;
                }
                else if (remainingImages > maxColumns && numberRows <= maxRows)
                {
                    width = (maxWidth + imageSpacing) * maxColumns + imageSpacing;
                    height = (maxHeight + imageSpacing) * Convert.ToInt16(numberRows) + imageSpacing;
                }
                else if (remainingImages > maxColumns && numberRows > maxRows)
                {
                    width = (maxWidth + imageSpacing) * maxColumns + imageSpacing;
                    height = (maxHeight + imageSpacing) * maxRows + imageSpacing;
                }
                
                string finalImage = @"ReportImage" + pageCount + @".jpg";
                Bitmap img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(Color.White);

                int row = 0;
                int col = 0;
                int w = 0;
                int h = 0;

                foreach (var enAnnotation in ecAnnotation.Entities.Skip(nIndex))
                {
                    Bitmap img = ConvertBase64ToImage(enAnnotation.GetAttributeValue<string>("documentbody"));
                    img = ResizeImage(img, maxHeight, maxWidth);

                    //Rows
                    if (nIndex == 0 || newPage == true)
                    {
                        w = imageSpacing;
                    }
                    else if (row < maxColumns - 1)
                    {
                        w += (maxWidth + imageSpacing);
                        row++;
                    }
                    else
                    {
                        w = imageSpacing;
                        row = 0;
                    }

                    //columns
                    if (nIndex == 0 || newPage == true)
                    {
                        h = imageSpacing;
                        col = nIndex;
                        newPage = false;
                    }
                    else if (row % maxColumns == 0)
                    {
                        h += (maxHeight + imageSpacing);
                        //col++;
                    }

                    g.DrawImage(img, new Point(w, h));
                    img.Dispose();
                    nIndex++;
                    if (nIndex == maxRows * maxColumns)
                    {
                        newPage = true;
                        break;
                    }
                }
                pageCount++;

                g.Dispose();

                Entity enNote = new Entity("annotation")
                {
                    ["subject"] = finalImage,
                    ["filename"] = finalImage,
                    ["documentbody"] = ConvertImageToBase64(img3),
                    ["objectid"] = new EntityReference("blu_inspectionoption", inspectionOptionId)
                };
                img3.Dispose();
                service.Create(enNote);
            }
        }

        private static Bitmap ResizeImage(Bitmap image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

        public static Bitmap ConvertBase64ToImage(string base64)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                Bitmap bm2 = new Bitmap(ms);
                return bm2;
            }
        }
        
        public static string ConvertImageToBase64(Image image)
        {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    string base64 = Convert.ToBase64String(ms.ToArray());
                    return base64;
                }
        }
    }
}

