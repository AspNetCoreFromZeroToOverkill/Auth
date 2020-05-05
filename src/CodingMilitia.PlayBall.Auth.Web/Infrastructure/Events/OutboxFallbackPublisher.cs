using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxFallbackPublisher
    {
        private const int MaxBatchSize = 100;
        private static readonly TimeSpan MinimumMessageAgeToBatch = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxFallbackPublisher> _logger;

        public OutboxFallbackPublisher(IServiceScopeFactory serviceScopeFactory,
            ILogger<OutboxFallbackPublisher> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task PublishPendingAsync(CancellationToken ct)
        {
            // Invokes PublishBatchAsync while batches are being published, to exhaust all pending messages.

            // ReSharper disable once EmptyEmbeddedStatement - the logic is part of the method invoked in the condition 
            while (!ct.IsCancellationRequested && await PublishBatchAsync(ct)) ;
        }

        // returns true if there is a new batch to publish, false otherwise
        private async Task<bool> PublishBatchAsync(CancellationToken ct)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var messages = await GetMessageBatchAsync(db, ct);

                if (messages.Count > 0 && await TryDeleteMessagesAsync(db, messages, ct))
                {
                    // TODO: actually push the events to the event bus
                    _logger.LogInformation(
                        "Events with ids {eventIds} (outbox message ids [{messageIds}]) published -> {events}",
                        string.Join(", ", messages.Select(message => message.Event.Id)),
                        string.Join(", ", messages.Select(message => message.Id)),
                        Newtonsoft.Json.JsonConvert.SerializeObject(messages.Select(message => message.Event)));

                    // ReSharper disable once MethodSupportsCancellation - messages already published, try to delete them locally
                    await transaction.CommitAsync();

                    return await IsNewBatchAvailableAsync(db, ct);
                }

                await transaction.RollbackAsync(ct);

                // if we got here, there either aren't messages available or are being published concurrently
                // in either case, we can break the loop
                return false;
            }
            catch (Exception)
            {
                // ReSharper disable once MethodSupportsCancellation - try to clean up things before letting go
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static Task<List<OutboxMessage>> GetMessageBatchAsync(AuthDbContext db, CancellationToken ct)
            => MessageBatchQuery(db)
                .Take(MaxBatchSize)
                .ToListAsync(ct);

        private static Task<bool> IsNewBatchAvailableAsync(AuthDbContext db, CancellationToken ct)
            => MessageBatchQuery(db).AnyAsync(ct);

        private static IQueryable<OutboxMessage> MessageBatchQuery(AuthDbContext db)
            => db.Set<OutboxMessage>()
                .Where(m => m.CreatedAt < GetMinimumMessageAgeToBatch());

        private async Task<bool> TryDeleteMessagesAsync(
            AuthDbContext db,
            IReadOnlyCollection<OutboxMessage> messages,
            CancellationToken ct)
        {
            try
            {
                db.Set<OutboxMessage>().RemoveRange(messages);
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogDebug(
                    $"Delete messages [{string.Join(", ", messages.Select(m => m.Id))}] failed, as it was done concurrently.");
                return false;
            }
        }

        private static DateTime GetMinimumMessageAgeToBatch()
        {
            return DateTime.UtcNow - MinimumMessageAgeToBatch;
        }
    }
}