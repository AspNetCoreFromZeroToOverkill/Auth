using System.Linq;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventDetectors
{
    public class UserUpdatedEventDetector : IEventDetector
    {
        private readonly ILogger<UserUpdatedEventDetector> _logger;

        public UserUpdatedEventDetector(ILogger<UserUpdatedEventDetector> logger)
        {
            _logger = logger;
        }

        public void Detect(AuthDbContext db)
        {
            const string UserNameProperty = nameof(PlayBallUser.UserName);

            var userUpdatedChanges =
                db
                    .ChangeTracker
                    .Entries<PlayBallUser>()
                    .Where(u => u.State == EntityState.Modified
                                &&
                                u.OriginalValues.GetValue<string>(UserNameProperty) !=
                                u.CurrentValues.GetValue<string>(UserNameProperty))
                    .ToList();

            foreach (var change in userUpdatedChanges)
            {
                _logger.LogInformation("UserUpdatedEvent - {username}", change.Entity.UserName);
            }
        }
    }
}