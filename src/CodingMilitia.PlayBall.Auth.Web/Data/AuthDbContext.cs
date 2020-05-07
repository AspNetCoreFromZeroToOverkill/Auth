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
        private readonly OnNewOutboxMessages _onNewOutboxMessages;
        
        public AuthDbContext(
            DbContextOptions<AuthDbContext> options,
            IEnumerable<IEventMapper> eventMappers,
            OnNewOutboxMessages onNewOutboxMessages)
            : base(options)
        {
            _eventMappers = eventMappers;
            _onNewOutboxMessages = onNewOutboxMessages;
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
            var eventsDetected = GetEvents();
            AddEventsIfAny(eventsDetected);

            var result = await base.SaveChangesAsync(cancellationToken);

            NotifyEventsIfAny(eventsDetected);

            return result;
        }

        private IReadOnlyCollection<OutboxMessage> GetEvents()
        {
            var now = DateTime.UtcNow;

            return _eventMappers
                .SelectMany(mapper => mapper.Map(this, now))
                .ToList();
        }

        private void AddEventsIfAny(IReadOnlyCollection<OutboxMessage> eventsDetected)
        {
            if (eventsDetected.Count > 0)
            {
                Set<OutboxMessage>().AddRange(eventsDetected);
            }
        }
        
        private void NotifyEventsIfAny(IReadOnlyCollection<OutboxMessage> eventsDetected)
        {
            if (eventsDetected.Count > 0)
            {
                _onNewOutboxMessages(eventsDetected.Select(e => e.Id));
            }
        }
    }
}