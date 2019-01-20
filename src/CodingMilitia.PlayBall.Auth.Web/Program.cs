using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            await host.EnsureDbUpToDateAsync();
            host.Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}