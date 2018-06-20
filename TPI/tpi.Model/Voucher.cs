using System;

namespace tpi.Model
{
    public class Voucher
    {
        public Guid Id { get; set; }
        public string VoucherNumber { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplayName { get; set; }
        public Guid CustomerId { get; set; }
        public Guid InspectionDetailId { get; set; }
        public Guid InspectionId { get; set; }
        public Guid OrderId { get; set; }
        public int StateCode { get; set; }
        public int StatusCode { get; set; }
    }
}
