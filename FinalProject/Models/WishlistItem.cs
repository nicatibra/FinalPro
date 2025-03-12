namespace FinalProject.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        // Relational properties
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}