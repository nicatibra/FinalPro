namespace FinalProject.ViewModels
{
    public class WishlistItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
    }

}
