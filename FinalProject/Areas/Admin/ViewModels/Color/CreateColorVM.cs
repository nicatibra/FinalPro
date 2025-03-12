namespace FinalProject.Areas.Admin.ViewModels
{
    public class CreateColorVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }


        [Required]
        public IFormFile Photo { get; set; }

    }
}
