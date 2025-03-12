namespace FinalProject.Models
{
    public class Category : BaseNameableEntity
    {
        public string Image { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }

        //Relational
        public ICollection<Product> Products { get; set; }
    }
}
