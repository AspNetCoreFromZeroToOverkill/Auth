using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxListener _listener;

        public OutboxBackgroundService(
            IServiceScopeFactory scopeFactory,
            OutboxListener listener)
        {
            _scopeFactory = scopeFactory;
            _listener = listener;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO: one message at a time might hinder performance, consider batching some messages
            await foreach (var messageId in _listener.GetStoredMessageIdsAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<OutboxPublisher>();
                await publisher.PublishAsync(messageId, stoppingToken);
            }
        }
    }
}