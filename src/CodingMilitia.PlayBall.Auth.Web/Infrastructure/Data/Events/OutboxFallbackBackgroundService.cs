using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxFallbackBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxListener _listener;

        public OutboxFallbackBackgroundService(
            IServiceScopeFactory scopeFactory,
            OutboxListener listener)
        {
            _scopeFactory = scopeFactory;
            _listener = listener;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<OutboxFallbackPublisher>();
                await publisher.PublishPendingAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}