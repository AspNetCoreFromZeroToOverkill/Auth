using System;
using System.Collections.Generic;
using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventMappers
{
    public class UserRegisteredEventMapper : IEventMapper
    {
        public IEnumerable<OutboxMessage> Map(AuthDbContext db, DateTime occurredAt)
            => db
                .ChangeTracker
                .Entries<PlayBallUser>()
                .Where(entry => entry.State == EntityState.Added)
                .Select(entry =>
                    new OutboxMessage(occurredAt,
                        new UserRegisteredEvent
                        {
                            Id = Guid.NewGuid(),
                            OccurredAt = occurredAt,
                            UserId = entry.Entity.Id,
                            UserName = entry.Entity.UserName
                        }));
    }
}