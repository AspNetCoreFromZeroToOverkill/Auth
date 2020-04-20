using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EventContracts = CodingMilitia.PlayBall.Auth.Events;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxFallbackPublisher
    {
        private const int MaxBatchSize = 100;
        private static readonly TimeSpan MinimumMessageAgeToBatch = TimeSpan.FromSeconds(30);

        private readonly AuthDbContext _db;
        private readonly IEventPublisher<EventContracts.BaseAuthEvent> _eventBusPublisher;

        public OutboxFallbackPublisher(
            AuthDbContext db,
            IEventPublisher<EventContracts.BaseAuthEvent> eventBusPublisher)
        {
            _db = db;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task PublishPendingAsync(CancellationToken ct)
        {
            // ReSharper disable once EmptyEmbeddedStatement - the logic is part of the method invoked in the condition 
            while (!ct.IsCancellationRequested && await PublishBatchAsync(ct));
        }

        private async Task<bool> PublishBatchAsync(CancellationToken ct)
        {
            IDbContextTransaction transaction = null;
            var batchPublished = false;

            try
            {
                transaction = await _db.Database.BeginTransactionAsync(ct);

                var minimumMessageAgeToBatch = GetMinimumMessageAgeToBatch();

                var messages = await _db.Set<OutboxMessage>()
                    .Where(m => m.CreatedAt < minimumMessageAgeToBatch)
                    .Take(MaxBatchSize)
                    .ToListAsync(ct);

                if (messages.Count > 0)
                {
                    _db.Set<OutboxMessage>().RemoveRange(messages);

                    await _db.SaveChangesAsync(ct);

                    await _eventBusPublisher.PublishAsync(messages.Select(message => message.Event.ToExternal()), ct);

                    batchPublished = true;
                }

                // ReSharper disable once MethodSupportsCancellation - messages already published, try to delete them locally
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    // ReSharper disable once MethodSupportsCancellation - try to clean up things before letting go
                    await transaction.RollbackAsync();
                }

                throw;
            }

            return batchPublished;
        }

        private static DateTime GetMinimumMessageAgeToBatch()
        {
            return DateTime.UtcNow - MinimumMessageAgeToBatch;
        }
    }
}