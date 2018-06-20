using System;

namespace tpi.Model
{
    public class Property
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UnitNumber { get; set; }
        public string StreetNumber { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string SubLocality { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string InspectionAddress { get; set; }
        public Guid ContactId { get; set; }
        public Guid AccountId { get; set; }
    }
}
