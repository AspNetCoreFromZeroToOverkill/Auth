using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Policies.Handlers;
using CodingMilitia.PlayBall.Auth.Web.Policies.Requirements;
using CodingMilitia.PlayBall.Auth.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CodingMilitia.PlayBall.Auth.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddConfiguredMvc()
                .AddConfiguredLocalization()
                .AddConfiguredIdentity(_configuration)
                .ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Logout";
                    options.AccessDeniedPath = "/AccessDenied";
                })
                .AddConfiguredIdentityServer(_environment, _configuration);

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts(); // https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet
            app.UseHttpsRedirection(); // if a request comes in HTTP, it's redirect to HTTPS
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}