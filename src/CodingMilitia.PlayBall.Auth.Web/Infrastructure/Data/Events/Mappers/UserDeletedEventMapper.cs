using System;
using System.Collections.Generic;
using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events.Mappers
{
    public class UserDeletedEventMapper : IEventMapper
    {
        private readonly ILogger<UserDeletedEventMapper> _logger;

        public UserDeletedEventMapper(ILogger<UserDeletedEventMapper> logger)
        {
            _logger = logger;
        }

        public IEnumerable<OutboxMessage> Map(AuthDbContext db, DateTime occurredAt)
            => db
                .ChangeTracker
                .Entries<PlayBallUser>()
                .Where(entry => entry.State == EntityState.Deleted)
                .Select(entry =>
                    new OutboxMessage(occurredAt,
                        new UserDeletedEvent
                        {
                            Id = Guid.NewGuid(),
                            OccurredAt = occurredAt,
                            UserId = entry.Entity.Id
                        }));
    }
}