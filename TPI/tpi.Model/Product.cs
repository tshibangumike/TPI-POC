using System;
using System.Collections.Generic;

namespace tpi.Model
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ParentProductId { get; set; }
        public bool IsParentProduct { get; set; }
        public Product ParentProduct { get; set; }
        public decimal BuyerPays { get; set; }
        public string BuyerPaysDisplayName { get; set; }
        public Guid UomId { get; set; }
        public Guid DefaultPriceListId { get; set; }
        public PriceList DefaultPriceList { get; set; }
        public int Status { get; set; }
        public bool StrataReport { get; set; }
        public string[] InspectorSkills { get; set; }
        public int AppointmentDuration { get; set; }
        public bool FreeReport { get; set; }
        public int ReportIsReleasedTo { get; set; }
        public Guid StockingProductId { get; set; }

        public List<Product> ChildProducts { get; set; }
        public List<PriceListItem> PriceListItems { get; set; }
    }
}
