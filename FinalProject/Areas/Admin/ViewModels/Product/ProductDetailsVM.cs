namespace FinalProject.Areas.Admin.ViewModels
{
    public class ProductDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string Image { get; set; }
        public int InStock { get; set; }
        public bool IsDeleted { get; set; }
        public string SecondaryImage { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public string? Weight { get; set; }
        public string? Volume { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public string Ingredients { get; set; }
        public List<string> AdditionalImages { get; set; }
        public List<string> Tags { get; set; }
        public List<string>? Colors { get; set; }

    }
}