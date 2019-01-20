using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account");
                });
            
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(_configuration.GetConnectionString("AuthDbContext"));
            });
            

            services
                .AddIdentity<PlayBallUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    //TODO: uncomment after some tests
                    //options.Password.RequiredLength = 12; 
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/AccessDenied";
            });

            services.AddSingleton<IEmailSender, DummyEmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts(); // added - https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet
            app.UseHttpsRedirection(); // added - if a request comes in HTTP, it's redirect to HTTPS
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }

    internal class DummyEmailSender : IEmailSender
    {
        private readonly ILogger<DummyEmailSender> _logger;

        public DummyEmailSender(ILogger<DummyEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogWarning("Dummy IEmailSender implementation is being used!!!");
            // added to be able to use the confirmation link
            _logger.LogDebug($"{email}{Environment.NewLine}{subject}{Environment.NewLine}{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}