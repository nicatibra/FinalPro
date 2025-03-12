namespace FinalProject.Areas.Admin.ViewModels
{
    public class UpdateSlideVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }

        public string? Image { get; set; }

        public string? Title { get; set; }

        public string? SubTitle { get; set; }

        public IFormFile? Photo { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }
    }
}
