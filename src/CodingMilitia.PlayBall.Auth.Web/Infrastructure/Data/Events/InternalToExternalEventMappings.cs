using System;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    internal static class InternalToExternalEventMappings
    {
        internal static Auth.Events.BaseAuthEvent ToExternal(this BaseAuthEvent @event)
            => @event switch
            {
                UserRegisteredEvent userRegistered => new Auth.Events.UserRegisteredEvent
                {
                    Id = userRegistered.Id,
                    OccurredAt = userRegistered.OccurredAt,
                    UserId = userRegistered.UserId,
                    UserName = userRegistered.UserName
                },
                UserDeletedEvent userDeleted => new Auth.Events.UserDeletedEvent()
                {
                    Id = userDeleted.Id,
                    OccurredAt = userDeleted.OccurredAt,
                    UserId = userDeleted.UserId
                },
                UserUpdatedEvent userUpdatedEvent => new Auth.Events.UserUpdatedEvent
                {
                    Id = userUpdatedEvent.Id,
                    OccurredAt = userUpdatedEvent.OccurredAt,
                    UserId = userUpdatedEvent.UserId,
                    UserName = userUpdatedEvent.UserName
                },
                _ => throw new NotImplementedException($"Unexpected event of type {@event.GetType()}")
            };
    }
}