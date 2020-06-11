using System;

namespace CodingMilitia.PlayBall.Auth.Events
{
    public abstract class BaseAuthEvent
    {
        protected BaseAuthEvent(Guid id, DateTime occurredAt)
        {
            Id = id;
            OccurredAt = occurredAt;
        }
        
        public Guid Id { get; }

        public DateTime OccurredAt { get; }
        
        // TODO: event version
    }
}