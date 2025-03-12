namespace FinalProject.Areas.Admin.ViewModels
{
    public class UpdateColorVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        public IFormFile? Photo { get; set; }

        public string? Image { get; set; }
    }
}
