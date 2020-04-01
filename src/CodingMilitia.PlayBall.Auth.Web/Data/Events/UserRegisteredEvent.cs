namespace CodingMilitia.PlayBall.Auth.Web.Data.Events
{
    public class UserRegisteredEvent : BaseAuthEvent
    {
        public string UserId { get; set; }

        public string UserName { get; set; }
    }
}