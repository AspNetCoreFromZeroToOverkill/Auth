namespace CodingMilitia.PlayBall.Auth.Events
{
    public class UserUpdatedEvent : BaseAuthEvent
    {
        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}