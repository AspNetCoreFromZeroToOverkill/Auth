using System;
using System.Collections.Generic;
using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventMappers
{
    public class UserUpdatedEventMapper : IEventMapper
    {
        private readonly ILogger<UserUpdatedEventMapper> _logger;

        public UserUpdatedEventMapper(ILogger<UserUpdatedEventMapper> logger)
        {
            _logger = logger;
        }

        public IEnumerable<OutboxMessage> Map(AuthDbContext db, DateTime occurredAt)
        {
            const string UserNameProperty = nameof(PlayBallUser.UserName);
            
            return db
                .ChangeTracker
                .Entries<PlayBallUser>()
                .Where(entry => entry.State == EntityState.Modified
                                &&
                                entry.OriginalValues.GetValue<string>(UserNameProperty) !=
                                entry.CurrentValues.GetValue<string>(UserNameProperty))
                .Select(entry =>
                    new OutboxMessage(occurredAt,
                        new UserUpdatedEvent
                        {
                            Id = Guid.NewGuid(),
                            OccurredAt = occurredAt,
                            UserId = entry.Entity.Id,
                            UserName = entry.Entity.UserName
                        }));
        }
    }
}