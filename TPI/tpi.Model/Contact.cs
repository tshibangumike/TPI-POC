using System;

namespace tpi.Model
{
    public class Contact
    {
        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
    }
}
