using System;

namespace tpi.Model
{
    public class Opportunity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ParentContactId { get; set; }
        public Guid ParentAccountId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PriceListId { get; set; }
        public Guid PropertyId { get; set; }
        public int StateCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountDisplayName { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountAmountDisplayName { get; set; }
        public int OpportunityType { get; set; }
        public Guid VoucherId { get; set; }
    }

    public enum OpportunityStatusCode
    {
        IncompleteChart = 858890001,
        InProgress = 1,
        OnHold = 2,
        Invoiced = 858890000,
        Ordered = 858890002,
        Won = 3
    }

    public enum OpportunityType
    {
        SalesOpportunity = 858890000,
        OnlineShopping = 858890001
    }

}