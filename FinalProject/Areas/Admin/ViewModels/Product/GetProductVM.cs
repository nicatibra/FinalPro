namespace FinalProject.Areas.Admin.ViewModels
{
    public class GetProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public int DiscountPercentage { get; set; }

        public string CategoryName { get; set; }
        public string BrandName { get; set; }

        public string Image { get; set; }
        public string SecondaryImage { get; set; }
        public int InStock { get; set; }
        public bool IsDeleted { get; set; }
    }
}
