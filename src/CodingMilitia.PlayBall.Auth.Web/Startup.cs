using CodingMilitia.PlayBall.Auth.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace CodingMilitia.PlayBall.Auth.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
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
                .AddConfiguredIdentityServer(_environment, _configuration)
                .AddEvents(_configuration);

            var dataProtectionKeysLocation =
                _configuration.GetSection<DataProtectionSettings>(nameof(DataProtectionSettings))?.Location;
            if (!string.IsNullOrWhiteSpace(dataProtectionKeysLocation))
            {
                services
                    .AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysLocation));
                // TODO: encrypt the keys
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts(); // https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet
                app.UseHttpsRedirection(); // if a request comes in HTTP, it's redirect to HTTPS    
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseIdentityServer();
            app.UseRequestLocalization(app.ApplicationServices
                .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}