using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxFallbackBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxFallbackBackgroundService> _logger;

        public OutboxFallbackBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxFallbackBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<OutboxFallbackPublisher>();
                    await publisher.PublishPendingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // We don't want the background service to stop while the application continues,
                    // so catching and logging.
                    // Should certainly have some extra checks for the reasons, to act on it. 
                    _logger.LogWarning(ex, "Unexpected error while publishing pending outbox messages.");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}