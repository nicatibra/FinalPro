namespace FinalProject.Areas.Admin.ViewModels
{
    public class UpdateProductVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name can not be longer than 50 characters.")]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100.")]
        public int DiscountPercentage { get; set; }


        [Required]
        public string Description { get; set; }


        [Required]
        public string SKU { get; set; }


        public string? Weight { get; set; }
        public string? Volume { get; set; }


        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative value.")]
        public int Stock { get; set; }


        [Required]
        public string Ingredients { get; set; }


        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }

        public ICollection<int>? ImageIds { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }


        public IFormFile? PrimaryPhoto { get; set; }
        public IFormFile? HoverPhoto { get; set; }
        public List<IFormFile>? AdditionalPhotos { get; set; }


        [Required]
        public int BrandId { get; set; }
        public ICollection<Brand>? Brands { get; set; }



        [Required]
        public int CategoryId { get; set; }
        public ICollection<Category>? Categories { get; set; }


        public ICollection<int>? ColorIds { get; set; }
        public ICollection<Color>? Colors { get; set; }


        public ICollection<int>? TagIds { get; set; }
        public ICollection<Tag>? Tags { get; set; }
    }
}
