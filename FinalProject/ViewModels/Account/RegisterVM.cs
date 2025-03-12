namespace FinalProject.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "First Name must be between 3 and 30 characters.")]
        [Display(Name = "First Name")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Last Name must be between 3 and 30 characters.")]
        [Display(Name = "Last Name")]
        public string Surname { get; set; }


        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [Display(Name = "Password")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }


        [Display(Name = "Gender (Optional)")]
        public EGender Gender { get; set; }


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot be more than 256 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "User Type is required.")]
        [Display(Name = "User Type")]
        public EUserRole UserRole { get; set; }


        [Required(ErrorMessage = "Street is required")]
        public string Street { get; set; }


        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }


        public string? State { get; set; }


        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }


        [Required(ErrorMessage = "Country is required")]
        public ECountry Country { get; set; }

        [Display(Name = "Agree to Terms & Policy")]
        public bool AgreeToTerms { get; set; }
    }
}
