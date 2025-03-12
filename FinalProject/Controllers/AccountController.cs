namespace FinalProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailService _mailService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IMailService mailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _mailService = mailService;
        }

        #region Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            AppUser user = new AppUser
            {
                Firstname = registerVM.Name,
                Lastname = registerVM.Surname,
                UserName = registerVM.Username.ToLower().Trim(),
                Email = registerVM.Email,
                Gender = registerVM.Gender.ToString(),
                Address = new Address
                {
                    Street = registerVM.Street,
                    City = registerVM.City,
                    State = registerVM.State,
                    PostalCode = registerVM.PostalCode,
                    Country = registerVM.Country
                }
            };


            IdentityResult result = await _userManager.CreateAsync(user, registerVM.Password);

            await _userManager.AddToRoleAsync(user, registerVM.UserRole.ToString());

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View(registerVM);
                }
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, protocol: Request.Scheme);

            await _mailService.SendEmailAsync(new MailRequestVM
            {
                ToEmail = user.Email,
                Subject = "Email Confirmation",
                Body = $"<a href=\"{confirmationLink}\">Confirm Email</a>"
            });

            return RedirectToAction(nameof(SuccessfullyRegistered), "Account");
        }
        #endregion

        public IActionResult SuccessfullyRegistered()
        {
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            AppUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                throw new BadRequestException();

            return View();
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked, please try again later.");
                return View();
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your Email.");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Entered password is incorrect.");
                return View();
            }

            if (returnUrl is null)
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }


        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordVM);

            // Find the user by username or email
            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(
                u => u.UserName == forgotPasswordVM.UsernameOrEmail || u.Email == forgotPasswordVM.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError("UsernameOrEmail", "User not found.");
                return View(forgotPasswordVM);
            }

            // Ensure the user has a valid email address
            if (string.IsNullOrEmpty(user.Email))
            {
                ModelState.AddModelError("UsernameOrEmail", "User does not have a valid email address.");
                return View(forgotPasswordVM);
            }

            // Generate a password reset token
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create the password reset link
            string link = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, HttpContext.Request.Scheme);

            // Send the password reset email
            await _mailService.SendEmailAsync(new MailRequestVM
            {
                ToEmail = user.Email, // Use the user's email address, not the username
                Subject = "Reset Password",
                Body = $"<a href='{link}'>Reset Password</a>"
            });

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                throw new BadRequestException();

            AppUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM, string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(resetPasswordVM);

            AppUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException();

            var identityUser = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.NewPassword);

            return RedirectToAction(nameof(Login));
        }




        public async Task<IActionResult> CreateRoles()
        {
            foreach (EUserRole role in Enum.GetValues(typeof(EUserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });
                }
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
