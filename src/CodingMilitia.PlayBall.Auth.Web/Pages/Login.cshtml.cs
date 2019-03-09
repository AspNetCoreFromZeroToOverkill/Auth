using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<PlayBallUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IStringLocalizer<LoginModel> _localizer;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public LoginModel(
            SignInManager<PlayBallUser> signInManager, 
            ILogger<LoginModel> logger, 
            IStringLocalizer<LoginModel> localizer,
            IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _sharedLocalizer = sharedLocalizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "RememberMe")]
            public bool RememberMe { get; set; }
        }
        
        public void OnGet(string returnUrl = null)
        {
            _logger.LogDebug(_sharedLocalizer["SampleSharedString"]);
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl;
        }
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return Redirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWithTwoFactor", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["InvalidLoginAttempt"]);
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}