using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingMilitia.PlayBall.Auth.Web.Pages.Admin
{
    [Authorize(Policy = "SamplePolicy")]
    public class AttributePolicyProtectedModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}