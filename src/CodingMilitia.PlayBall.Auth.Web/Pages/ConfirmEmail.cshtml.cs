using System;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingMilitia.PlayBall.Auth.Web.Pages
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<PlayBallUser> _userManager;

        public ConfirmEmailModel(UserManager<PlayBallUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }

            return Page();
        }
    }
}