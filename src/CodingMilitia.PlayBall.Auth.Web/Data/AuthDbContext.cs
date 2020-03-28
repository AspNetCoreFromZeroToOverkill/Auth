using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public class AuthDbContext : IdentityDbContext<PlayBallUser>
    {
        private readonly IEnumerable<IEventDetector> _eventDetectors;

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IEnumerable<IEventDetector> eventDetectors)
            : base(options)
        {
            _eventDetectors = eventDetectors;
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");
            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var detector in _eventDetectors)
            {
                detector.Detect(this);
            }
            
            // TODO: publish detected events
            
            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}