namespace FinalProject.Models
{
    public class AppUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public Address Address { get; set; }

        public List<BasketItem> BasketItems { get; set; }

    }
}
