namespace FinalProject.Areas.Admin.ViewModels
{
    public class CreateProductVM
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


        [Required(ErrorMessage = "Please upload at least one image.")]
        public IFormFile PrimaryPhoto { get; set; }


        [Required(ErrorMessage = "Please upload at least one image.")]
        public IFormFile HoverPhoto { get; set; }


        public List<IFormFile>? AdditionalPhotos { get; set; }


        [Required]
        public int BrandId { get; set; }
        public List<Brand>? Brands { get; set; }



        [Required]
        public int CategoryId { get; set; }
        public List<Category>? Categories { get; set; }


        public List<int>? ColorIds { get; set; }
        public List<Color>? Colors { get; set; }


        public List<int>? TagIds { get; set; }
        public List<Tag>? Tags { get; set; }
    }
}
