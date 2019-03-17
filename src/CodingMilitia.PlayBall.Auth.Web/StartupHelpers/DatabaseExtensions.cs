using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace - by design, to ease discoverability 
namespace Microsoft.AspNetCore.Hosting
{
    internal static class DatabaseExtensions
    {
        internal static async Task EnsureDbUpToDateAsync(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var hostingEnvironment = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                if (hostingEnvironment.IsDevelopment() || hostingEnvironment.IsEnvironment("DockerDevelopment"))
                {
                    var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                    await authDbContext.Database.MigrateAsync();

                    var grantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                    await grantDbContext.Database.MigrateAsync();
                }
            }
        }
    }
}