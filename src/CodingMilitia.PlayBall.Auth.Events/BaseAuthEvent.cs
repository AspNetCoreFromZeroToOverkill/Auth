using System;

namespace CodingMilitia.PlayBall.Auth.Events
{
    public abstract class BaseAuthEvent
    {
        public Guid Id { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        
        // TODO: event version
    }
}