namespace CodingMilitia.PlayBall.Auth.Web.Configuration
{
    public class WebFrontendClientSettings
    {
        public string Secret { get; set; }
        public string[] RedirectUris { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}
