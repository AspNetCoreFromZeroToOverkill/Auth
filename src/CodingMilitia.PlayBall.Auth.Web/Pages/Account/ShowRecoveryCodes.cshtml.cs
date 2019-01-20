using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingMilitia.PlayBall.Auth.Web.Pages.Account
{
    public class ShowRecoveryCodesModel : PageModel
    {
        [TempData] public string[] RecoveryCodes { get; set; }

        [TempData] public string StatusMessage { get; set; }

        public IActionResult OnGet()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./ManageTwoFactor");
            }

            return Page();
        }
    }
}