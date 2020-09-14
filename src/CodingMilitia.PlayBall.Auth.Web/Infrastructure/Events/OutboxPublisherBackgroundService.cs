using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxPublisherBackgroundService : BackgroundService
    {
        private readonly OutboxPublisher _publisher;
        private readonly OutboxListener _listener;
        private readonly ILogger<OutboxPublisherBackgroundService> _logger;

        public OutboxPublisherBackgroundService(
            OutboxPublisher publisher,
            OutboxListener listener,
            ILogger<OutboxPublisherBackgroundService> logger)
        {
            _publisher = publisher;
            _listener = listener;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _publisher.PublishPendingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // We don't want the background service to stop while the application continues,
                    // so catching and logging.
                    // Should certainly have some extra checks for the reasons, to act on it. 
                    _logger.LogWarning(ex, "Unexpected error while publishing pending outbox messages.");
                }

                // wait for whatever occurs first:
                // - being notified of new messages added to the outbox
                // - poll the outbox every 30s, for example, in cases where another instance of the service persisted
                //   something but didn't publish, or some error occurred when publishing and there are pending messages
                await Task.WhenAny(
                    _listener.WaitForMessagesAsync(stoppingToken),
                    Task.Delay(TimeSpan.FromSeconds(30), stoppingToken));
            }
        }
    }
}