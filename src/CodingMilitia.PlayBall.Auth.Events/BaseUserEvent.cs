using System;

namespace CodingMilitia.PlayBall.Auth.Events
{
    public abstract class BaseUserEvent : BaseAuthEvent
    {
        public BaseUserEvent(Guid id, DateTime occurredAt, string userId) : base(id, occurredAt)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}