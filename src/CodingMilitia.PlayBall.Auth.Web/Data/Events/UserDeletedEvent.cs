namespace CodingMilitia.PlayBall.Auth.Web.Data.Events
{
    public class UserDeletedEvent : BaseAuthEvent
    {
        public string UserId { get; set; }
    }
}