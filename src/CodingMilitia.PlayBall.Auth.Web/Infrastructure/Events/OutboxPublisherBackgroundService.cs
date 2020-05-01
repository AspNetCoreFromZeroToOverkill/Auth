using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxPublisherBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxListener _listener;
        private readonly ILogger<OutboxPublisherBackgroundService> _logger;

        public OutboxPublisherBackgroundService(
            IServiceScopeFactory scopeFactory,
            OutboxListener listener,
            ILogger<OutboxPublisherBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _listener = listener;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: one message at a time might hinder throughput, consider batching
            await foreach (var messageId in _listener.GetAllMessageIdsAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<OutboxPublisher>();
                    await publisher.PublishAsync(messageId, stoppingToken);
                }
                catch (Exception ex)
                {
                    // We don't want the background service to stop while the application continues,
                    // so catching and logging.
                    // Should certainly have some extra checks for the reasons, to act on it. 
                    _logger.LogWarning(ex, "Unexpected error while publishing pending outbox messages.");
                }
            }
        }
    }
}