using System;
using System.Collections.Generic;

namespace tpi.Model
{
    public class PriceListItem
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplayName { get; set; }
        public Guid UomId { get; set; }
        public Guid PriceListId { get; set; }
        public Guid ProductId { get; set; }
        public bool IsPriceOverriden { get; set; }
        public Product Product { get; set; }
        public PriceList PriceList { get; set; }

        public List<Product> Products { get; set; }
    }
}
