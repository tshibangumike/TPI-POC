using System;
using System.Linq;
using System.Security.Principal;

namespace tpi.Model
{

    public class CurrentUser
    {
        public PortalUser PortalUser { get; set; }
        public Guid OpportunityId { get; set; }
        public Guid PriceListId { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public Guid AppointmentId { get; set; }
    }
}
