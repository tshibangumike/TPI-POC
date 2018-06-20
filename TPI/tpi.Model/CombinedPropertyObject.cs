using System;

namespace tpi.Model
{
    public class CombinedPropertyObject
    {
        public Guid OpportunityId { get; set; }
        public Appointment Apppointment { get; set; }
        public Inspection Inspection { get; set; }
        public Property Property { get; set; }
        public bool DoCreateAnInspection { get; set; }
    }
}
