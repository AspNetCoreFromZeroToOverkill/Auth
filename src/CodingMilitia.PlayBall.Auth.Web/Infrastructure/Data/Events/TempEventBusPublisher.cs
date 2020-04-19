using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Shared.EventBus;
using Microsoft.Extensions.Logging;
using EventContracts = CodingMilitia.PlayBall.Auth.Events; 

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class TempEventBusPublisher : IEventPublisher<EventContracts.BaseAuthEvent>
    {
        private readonly ILogger<TempEventBusPublisher> _logger;

        public TempEventBusPublisher(ILogger<TempEventBusPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(EventContracts.BaseAuthEvent @event, CancellationToken ct)
        {
            _logger.LogInformation(
                $"Publishing message: {Environment.NewLine}{JsonSerializer.Serialize(@event)}");

            return Task.CompletedTask;
        }

        public Task PublishAsync(IEnumerable<EventContracts.BaseAuthEvent> events, CancellationToken ct)
        {
            _logger.LogInformation(
                $"Publishing message batch: {Environment.NewLine}{JsonSerializer.Serialize(events)}");


            return Task.CompletedTask;
        }
    }
}