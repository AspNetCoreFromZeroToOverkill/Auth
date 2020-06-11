using System;

namespace CodingMilitia.PlayBall.Auth.Events
{
    public class UserDeletedEvent : BaseUserEvent
    {
        public UserDeletedEvent(Guid id, DateTime occurredAt, string userId) : base(id, occurredAt, userId)
        {
        }
    }
}