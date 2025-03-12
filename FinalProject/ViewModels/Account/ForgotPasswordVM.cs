namespace FinalProject.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string UsernameOrEmail { get; set; }

    }
}
