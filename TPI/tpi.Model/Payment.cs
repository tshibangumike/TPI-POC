using System;

namespace tpi.Model
{
    public class Payment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public  int PaymentMethod { get; set; }
        public  decimal Amount { get; set; }
        public  DateTime PaymentDate { get; set; }
        public  string Reference { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public int StatusCode { get; set; }
        public int StateCode { get; set; }
    }
}
