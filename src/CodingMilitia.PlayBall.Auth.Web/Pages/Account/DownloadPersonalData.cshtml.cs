using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CodingMilitia.PlayBall.Auth.Web.Pages.Account
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<PlayBallUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<PlayBallUser> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation(
                "User with ID '{UserId}' asked for their personal data.",
                _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = typeof(PlayBallUser)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)))
                .ToDictionary(p => p.Name, p => p.GetValue(user)?.ToString() ?? "null");

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)),
                "text/json");
        }
    }
}