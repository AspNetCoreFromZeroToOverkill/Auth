using System;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;
using Bus = CodingMilitia.PlayBall.Auth.Events;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    internal static class EventMappings
    {
        internal static Bus.BaseAuthEvent ToBusEvent(this BaseAuthEvent @event)
            => @event switch
            {
                UserRegisteredEvent registered => new Bus.UserRegisteredEvent(
                    registered.Id,
                    registered.OccurredAt,
                    registered.UserId,
                    registered.UserName),
                UserUpdatedEvent updated => new Bus.UserUpdatedEvent(
                    updated.Id,
                    updated.OccurredAt,
                    updated.UserId,
                    updated.UserName),
                UserDeletedEvent deleted => new Bus.UserDeletedEvent(
                    deleted.Id,
                    deleted.OccurredAt,
                    deleted.UserId),
                _ => throw new NotImplementedException($"Mapping not implemented for event of type {@event.GetType()}")
            };
    }
}