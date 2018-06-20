using System;
using System.Collections.Generic;

namespace tpi.Model
{
    public class PortalUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PortalUserRole { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public List<PriceList> PriceLists { get; set; }
    }

    public enum PortalUserrole
    {
        Consumer = 858890000,
        Vendor = 858890001,
        Agent = 858890002
    };

    public enum PortalUserStatusCode
    {
        Active = 1,
        PendingApproval = 858890000,
        Suspended = 858890001
    };

}
