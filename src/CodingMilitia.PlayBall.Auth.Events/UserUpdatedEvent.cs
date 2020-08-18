using System;

namespace CodingMilitia.PlayBall.Auth.Events
{
    public class UserUpdatedEvent : BaseUserEvent
    {
        public UserUpdatedEvent(Guid id, DateTime occurredAt, string userId, string userName)
            : base(id, occurredAt, userId)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }
}