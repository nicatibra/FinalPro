﻿namespace FinalProject.Models
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string PostalCode { get; set; }
        public ECountry Country { get; set; }
    }
}
