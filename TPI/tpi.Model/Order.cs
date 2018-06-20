using System;

namespace tpi.Model
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountDisplayName { get; set; }
        public string CurrentUserSessionDetail { get; set; }
        public  Guid CustomerId { get; set; }
        public Guid PropertyId { get; set; }
        public int StateCode { get; set; }
    }
}
