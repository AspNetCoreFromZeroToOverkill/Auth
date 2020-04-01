using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public class AuthDbContext : IdentityDbContext<PlayBallUser>
    {
        private readonly IEnumerable<IEventMapper> _eventMappers;

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IEnumerable<IEventMapper> eventMappers)
            : base(options)
        {
            _eventMappers = eventMappers;
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var wereEventsDetected = MapAndAddEventsIfAnyEvents();

            var result = await base.SaveChangesAsync(cancellationToken);

            // TODO: publish the events persisted in the outbox

            return result;
        }

        private bool MapAndAddEventsIfAnyEvents()
        {
            var now = DateTime.UtcNow;

            var events = _eventMappers
                .SelectMany(mapper => mapper.Map(this, now))
                .ToList();

            if (events.Count > 0)
            {
                OutboxMessages.AddRange(events);
            }

            return events.Count > 0;
        }
    }
}