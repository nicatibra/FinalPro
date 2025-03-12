namespace FinalProject.Areas.Admin.ViewModels
{
    public class UpdateTagVM
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Name { get; set; }
    }
}
