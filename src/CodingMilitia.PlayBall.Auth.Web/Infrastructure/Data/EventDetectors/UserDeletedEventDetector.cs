using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventDetectors
{
    public class UserDeletedEventDetector : IEventDetector
    {
        private readonly ILogger<UserDeletedEventDetector> _logger;

        public UserDeletedEventDetector(ILogger<UserDeletedEventDetector> logger)
        {
            _logger = logger;
        }

        public void Detect(AuthDbContext db)
        {
            var userDeletedChanges =
                db
                    .ChangeTracker
                    .Entries<PlayBallUser>()
                    .Where(u => u.State == EntityState.Deleted)
                    .ToList();

            foreach (var change in userDeletedChanges)
            {
                _logger.LogInformation("UserDeletedEvent - {username}", change.Entity.UserName);
            }
        }
    }
}