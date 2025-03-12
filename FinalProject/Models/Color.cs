namespace FinalProject.Models
{
    public class Color : BaseNameableEntity
    {
        public string Image { get; set; }
        //Relational
        public List<ProductColor> ProductColors { get; set; }

    }
}
