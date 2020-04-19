namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public interface IEventDetector
    {
        void Detect(AuthDbContext db);
    }
}