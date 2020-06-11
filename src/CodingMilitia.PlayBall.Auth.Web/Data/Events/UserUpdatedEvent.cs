namespace CodingMilitia.PlayBall.Auth.Web.Data.Events
{
    public class UserUpdatedEvent : BaseUserEvent
    {
        public string UserName { get; set; }
    }
}