namespace FinalProject.Models
{
    public class BasketItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }


        //Relational
        public int ProductId { get; set; }
        public Product Product { get; set; }


        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
