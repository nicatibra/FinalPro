namespace FinalProject.ViewModels
{
    public class DetailVM
    {
        public Product Product { get; set; }

        public ICollection<Product> RelatedProducts { get; set; }
    }
}
