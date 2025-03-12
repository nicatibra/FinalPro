namespace FinalProject.Models
{
    public class ProductImage : BaseEntity
    {
        public string Image { get; set; }

        public bool? IsPrimary { get; set; }


        //Relational
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
