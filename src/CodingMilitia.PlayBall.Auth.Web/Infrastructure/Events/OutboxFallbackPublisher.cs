using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxFallbackPublisher
    {
        private const int MaxBatchSize = 100;
        private static readonly TimeSpan MinimumMessageAgeToBatch = TimeSpan.FromSeconds(30);

        private readonly AuthDbContext _db;
        private readonly ILogger<OutboxFallbackPublisher> _logger;

        public OutboxFallbackPublisher(
            AuthDbContext db,
            ILogger<OutboxFallbackPublisher> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task PublishPendingAsync(CancellationToken ct)
        {
            // Invokes PublishBatchAsync while batches are being published, to exhaust all pending messages.

            // ReSharper disable once EmptyEmbeddedStatement - the logic is part of the method invoked in the condition 
            while (!ct.IsCancellationRequested && await PublishBatchAsync(ct)) ;
        }

        // returns true if a batch was published, false otherwise
        private async Task<bool> PublishBatchAsync(CancellationToken ct)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var minimumMessageAgeToBatch = GetMinimumMessageAgeToBatch();

                var messages = await _db.Set<OutboxMessage>()
                    .Where(m => m.CreatedAt < minimumMessageAgeToBatch)
                    .Take(MaxBatchSize)
                    .ToListAsync(ct);

                if (messages.Count > 0 && await TryDeleteMessagesAsync(messages, ct))
                {
                    // TODO: actually push the events to the event bus
                    _logger.LogInformation(
                        "Events with ids {eventIds} (outbox message ids [{messageIds}]) published -> {events}",
                        string.Join(", ", messages.Select(message => message.Event.Id)),
                        string.Join(", ", messages.Select(message => message.Id)),
                        Newtonsoft.Json.JsonConvert.SerializeObject(messages.Select(message => message.Event)));

                    // ReSharper disable once MethodSupportsCancellation - messages already published, try to delete them locally
                    await transaction.CommitAsync();

                    return true;
                }

                await transaction.RollbackAsync(ct);

                return false;
            }
            catch (Exception)
            {
                // ReSharper disable once MethodSupportsCancellation - try to clean up things before letting go
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static DateTime GetMinimumMessageAgeToBatch()
        {
            return DateTime.UtcNow - MinimumMessageAgeToBatch;
        }

        private async Task<bool> TryDeleteMessagesAsync(
            IReadOnlyCollection<OutboxMessage> messages,
            CancellationToken ct)
        {
            try
            {
                _db.Set<OutboxMessage>().RemoveRange(messages);
                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogDebug(
                    $"Delete messages [{string.Join(", ", messages.Select(m => m.Id))}] failed, as it was done concurrently.");
                return false;
            }
        }
    }
}