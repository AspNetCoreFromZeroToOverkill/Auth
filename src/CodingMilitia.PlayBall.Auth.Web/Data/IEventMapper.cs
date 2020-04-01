using System;
using System.Collections.Generic;

namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public interface IEventMapper
    {
        IEnumerable<OutboxMessage> Map(AuthDbContext db, DateTime occurredAt);
    }
}