namespace FinalProject.ViewModels
{
    public class HomeVM
    {
        public ICollection<Product> Products { get; set; }
        public ICollection<Slide> Slides { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
