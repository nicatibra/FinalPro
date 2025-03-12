namespace FinalProject.ViewModels
{
    public class OrderVM
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be more than 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [Display(Name = "Country")]
        public ECountry Country { get; set; }

        [Required(ErrorMessage = "City/Town is required.")]
        [StringLength(50, ErrorMessage = "City/Town cannot be more than 50 characters.")]
        [Display(Name = "City/Town")]
        public string CityOrTown { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(100, ErrorMessage = "Address cannot be more than 100 characters.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "State cannot be more than 50 characters.")]
        [Display(Name = "State")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Postcode/ZIP is required.")]
        [StringLength(20, ErrorMessage = "Postcode/ZIP cannot be more than 20 characters.")]
        [Display(Name = "Postcode/ZIP")]
        public string PostOrZipCode { get; set; }

        [StringLength(500, ErrorMessage = "Additional information cannot be more than 500 characters.")]
        [Display(Name = "Additional Information")]
        public string? AdditionalInfo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot be more than 256 characters.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public List<BasketInOrderItemVM>? BasketInOrderItemsVMs { get; set; }
    }
}