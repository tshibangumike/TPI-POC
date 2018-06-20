using System;
using System.Collections.Generic;

namespace tpi.Model
{
    public class Inspection
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Guid PropertyId { get; set; }
        public string InspectionText { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid OrderId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ContactId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime AppointmentStartTime { get; set; }
        public string StatusCodeText { get; set; }
        public int StateCode { get; set; }
        public int StatusCode { get; set; }
        public bool StrataReport { get; set; }
        public bool ProductCategory { get; set; }
        public int ReportPriority { get; set; }
        public string ReportUrl { get; set; }
        public bool FreeReport { get; set; }
        public bool OnBackOrder { get; set; }
        public int ReportIsReleasedTo { get; set; }
        public int ReportIsSellableTo { get; set; }
        public int SellableTo { get; set; }
        public List<byte[]> ReportData { get; set; }
    }
    
    public enum ReportPriority
    {
        Normal = 858890000,
        Urgent = 858890001
    }

}
