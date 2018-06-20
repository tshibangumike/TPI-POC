using System;

namespace tpi.Model
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public DateTime StartTime { get; set; }
        public string StartTimeText { get; set; }
        public string StartTimeDisplayName { get; set; }
        public string EndTimeDisplayName { get; set; }
        public DateTime EndTime { get; set; }
        public string EndTimeText { get; set; }
        public Guid RegardingObjectId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ProductId { get; set; }
        public int PriorityCode { get; set; }
        public SystemUser Owner { get; set; }
    }
    public enum ApprovalStatus
    {
        New = 858890000,
        ApprovedEmailCustomer = 858890001,
        ApprovedDonotEmailCustomer = 858890002
    }
}
