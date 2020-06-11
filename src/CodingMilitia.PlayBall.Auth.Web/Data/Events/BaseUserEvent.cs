namespace CodingMilitia.PlayBall.Auth.Web.Data.Events
{
    public abstract class BaseUserEvent : BaseAuthEvent
    {
        public string UserId { get; set; }
    }
}