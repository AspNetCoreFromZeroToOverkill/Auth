using System;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;

namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public class OutboxMessage
    {
        public OutboxMessage(DateTime createdAt, BaseAuthEvent @event)
        {
            CreatedAt = createdAt;
            Event = @event;
        }
        
        public long Id { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public BaseAuthEvent Event { get; private set; }
    }
}