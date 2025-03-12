namespace FinalProject.Models
{
    public class Brand : BaseNameableEntity
    {
        public string Image { get; set; }
        //public string ContactMail { get; set; }

        //Relational
        public ICollection<Product> Products { get; set; }
    }
}
