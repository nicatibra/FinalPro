namespace FinalProject.Models
{
    public class Product : BaseNameableEntity
    {
        public decimal Price { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal DiscountPrice { get; set; }

        public string Description { get; set; }
        public string SKU { get; set; }


        public string? Weight { get; set; }
        public string? Volume { get; set; }

        public int SalesCount { get; set; }


        public ICollection<ProductBatch> ProductBatches { get; set; }


        public List<ProductImage> ProductImages { get; set; }


        public string Ingredients { get; set; }


        public int CategoryId { get; set; }
        public Category Category { get; set; }


        public int BrandId { get; set; }
        public Brand Brand { get; set; }


        public ICollection<ProductTag> ProductTags { get; set; }
        public ICollection<ProductColor>? ProductColors { get; set; }
    }
}
