using System;

namespace tpi.Model
{
    public class OrderProduct
    {
        public Guid Id { get; set; }
        public bool IsProductOverridden { get; set; }
        public Guid ProductId { get; set; }
        public Guid UomId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public bool OnBackOrder { get; set; }
        public string AmountDisplayName { get; set; }
        public Guid OrderId { get; set; }
        public string ProductDescription { get; set; }
        public bool IsPriceOverriden { get; set; }
        public bool IsProductOverriden { get; set; }
        public bool? ProductCategory { get; set; }
        public Product Product { get; set; }
        public int ReportPriority { get; set; }
        public Guid VoucherId { get; set; }
        public int SellableTo { get; set; }
        public bool FreeReport { get; set; }
        public int ReportIsReleasedTo { get; set; }
    }
}