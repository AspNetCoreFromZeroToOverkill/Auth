using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Events;
using CodingMilitia.PlayBall.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxPublisher
    {
        private const int MaxBatchSize = 100;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxPublisher> _logger;
        private readonly IEventPublisher<BaseAuthEvent> _eventPublisher;

        public OutboxPublisher(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OutboxPublisher> logger,
            IEventPublisher<BaseAuthEvent> eventPublisher)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _eventPublisher = eventPublisher;
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
                    await _eventPublisher.PublishAsync(messages.Select(m => m.Event.ToBusEvent()), ct);

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
            => db.Set<OutboxMessage>()
                .Take(MaxBatchSize)
                .ToListAsync(ct);

        private static Task<bool> IsNewBatchAvailableAsync(AuthDbContext db, CancellationToken ct)
            => db.Set<OutboxMessage>().AnyAsync(ct);

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
    }
}