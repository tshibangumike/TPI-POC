using System;
using System.Collections.Generic;

namespace tpi.Model
{
    public class PriceList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PortalUserRole { get; set; }
        public Guid QuestionSetupId { get; set; }
        public int ReportPriority { get; set; }
        public string[] PaymentMethods { get; set; }

        public List<PriceListItem> PriceListItems { get; set; }
    }
}
