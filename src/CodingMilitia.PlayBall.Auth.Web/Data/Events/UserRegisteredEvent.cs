namespace CodingMilitia.PlayBall.Auth.Web.Data.Events
{
    public class UserRegisteredEvent : BaseUserEvent
    {
        public string UserName { get; set; }
    }
}