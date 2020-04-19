namespace CodingMilitia.PlayBall.Auth.Events
{
    public class UserRegisteredEvent : BaseAuthEvent
    {
        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}