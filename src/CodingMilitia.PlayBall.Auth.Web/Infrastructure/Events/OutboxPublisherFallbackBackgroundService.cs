using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxPublisherFallbackBackgroundService : BackgroundService
    {
        private readonly OutboxFallbackPublisher _fallbackPublisher;
        private readonly ILogger<OutboxPublisherFallbackBackgroundService> _logger;

        public OutboxPublisherFallbackBackgroundService(
            OutboxFallbackPublisher fallbackPublisher,
            ILogger<OutboxPublisherFallbackBackgroundService> logger)
        {
            _fallbackPublisher = fallbackPublisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _fallbackPublisher.PublishPendingAsync(stoppingToken);
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