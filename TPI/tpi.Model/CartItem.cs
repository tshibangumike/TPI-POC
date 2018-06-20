using System;

namespace tpi.Model
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public bool ProductCategory { get; set; }

        public Guid ParentProductId { get; set; }
        public string ParentProductName { get; set; }

        public decimal Amount { get; set; }
        public string AmountText { get; set; }

        public decimal BuyerPays { get; set; }
        public string BuyerPaysText { get; set; }

        public string Conditions { get; set; }

        public decimal FinalBuyerPays { get; set; }
        public string FinalBuyerPaysText { get; set; }

        public int ReportIsSellableTo { get; set; }

        public int ReportIsReleasedTo { get; set; }
        public string ReportIsReleasedToText { get; set; }

        public Guid UomId { get; set; }        

        public int StateCode { get; set; }

        public int StatusCode { get; set; }

        public int InspectionDetailStateCode { get; set; }

        public int InspectionDetailStatusCode { get; set; }

        public string[] ProductSkills { get; set; }

        public int AppointmentDuration { get; set; }

        public bool IsStrataReport { get; set; }

        public Guid PriceListId { get; set; }

        public Guid PriceListItemId { get; set; }

        public bool IsUrgent { get; set; }

        public Guid OpportunityId { get; set; }

        public bool IsPriceOverriden { get; set; }

        public int ReportPriority {get;set;}

        public int PaymentMethodType { get; set; }

        public int SellableTo { get; set; }

        public bool FreeReport { get; set; }

        public bool OnBackOrder { get; set; }

    }
}
