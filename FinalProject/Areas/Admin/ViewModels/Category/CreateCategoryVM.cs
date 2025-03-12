namespace FinalProject.Areas.Admin.ViewModels
{
    public class CreateCategoryVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }

        [Required]
        public IFormFile Photo { get; set; }
    }
}
