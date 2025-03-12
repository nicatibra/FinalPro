namespace FinalProject.Models
{
    public class Tag : BaseNameableEntity
    {
        //Relational
        public List<ProductTag> ProductTags { get; set; }

    }
}
