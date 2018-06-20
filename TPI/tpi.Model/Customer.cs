using System;

namespace tpi.Model
{

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public int CustomerType { get; set; }
        public string CustomerTypeText { get; set; }
        public bool HasAccount { get; set; }
        public Guid PriceListId { get; set; }
    }

    public enum CustomerType
    {
        Account = 1,
        Contact = 2
    };

    public enum PortalRegisteredAs
    {
        PortalUser = 858890000,
        Guest = 858890001
    };

}
