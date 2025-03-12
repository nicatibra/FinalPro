namespace FinalProject.Models
{
    public class Order
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }


        public string DateString { get; set; }
        public decimal TotalPrice { get; set; }
        public bool? Status { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string CityOrTown { get; set; }
        public string Address { get; set; }
        public string? State { get; set; }
        public string PostOrZipCode { get; set; }
        public string Email { get; set; }
        public string? AdditionalInfo { get; set; }


        //Relational
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
