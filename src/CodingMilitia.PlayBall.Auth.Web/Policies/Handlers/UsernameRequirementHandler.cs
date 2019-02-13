using CodingMilitia.PlayBall.Auth.Web.Policies.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodingMilitia.PlayBall.Auth.Web.Policies.Handlers
{
    public class UsernameRequirementHandler : AuthorizationHandler<UsernameRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UsernameRequirement requirement)
        {
            if (Regex.IsMatch(context.User.Identity.Name, requirement.UsernamePattern))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
