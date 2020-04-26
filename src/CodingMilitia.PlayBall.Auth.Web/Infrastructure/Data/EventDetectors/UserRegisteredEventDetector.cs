using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventDetectors
{
    public class UserRegisteredEventDetector : IEventDetector
    {
        private readonly ILogger<UserRegisteredEventDetector> _logger;

        public UserRegisteredEventDetector(ILogger<UserRegisteredEventDetector> logger)
        {
            _logger = logger;
        }

        public void Detect(AuthDbContext db)
        {
            var userRegisteredChanges =
                db
                    .ChangeTracker
                    .Entries<PlayBallUser>()
                    .Where(u => u.State == EntityState.Added)
                    .ToList();

            foreach (var change in userRegisteredChanges)
            {
                _logger.LogInformation("UserRegisteredEvent - {username}", change.Entity.UserName);
            }
        }
    }
}