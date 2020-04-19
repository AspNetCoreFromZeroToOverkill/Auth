namespace CodingMilitia.PlayBall.Auth.Events
{
    public class UserDeletedEvent : BaseAuthEvent
    {
        public string UserId { get; set; }
    }
}