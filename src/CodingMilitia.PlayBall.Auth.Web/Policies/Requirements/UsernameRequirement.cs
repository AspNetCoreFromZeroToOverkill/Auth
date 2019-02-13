using Microsoft.AspNetCore.Authorization;

namespace CodingMilitia.PlayBall.Auth.Web.Policies.Requirements
{
    public class UsernameRequirement : IAuthorizationRequirement
    {
        public UsernameRequirement(string usernamePattern)
        {
            UsernamePattern = usernamePattern;
        }

        public string UsernamePattern { get; }
    }
}
