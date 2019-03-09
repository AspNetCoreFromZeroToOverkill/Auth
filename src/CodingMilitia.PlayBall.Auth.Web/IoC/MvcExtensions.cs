using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcExtensions
    {
        public static IServiceCollection AddConfiguredMvc(this IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account");
                    options.Conventions.AuthorizeFolder("/Consent");
                })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            return services;
        }
    }
}