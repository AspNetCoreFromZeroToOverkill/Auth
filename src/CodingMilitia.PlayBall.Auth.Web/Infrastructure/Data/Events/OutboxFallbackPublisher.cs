using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxFallbackPublisher
    {
        private const int MaxBatchSize = 100;
        
        private readonly AuthDbContext _db;
        private readonly TempEventBusPublisher _eventBusPublisher;

        public OutboxFallbackPublisher(AuthDbContext db, TempEventBusPublisher eventBusPublisher)
        {
            _db = db;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task PublishPendingAsync(CancellationToken ct)
        {
            bool batchPublished;
            do
            {
                batchPublished = await PublishBatchAsync(ct);
            } while (batchPublished);
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
                    batchPublished = true;

                    _db.Set<OutboxMessage>().RemoveRange(messages);

                    await _db.SaveChangesAsync(ct);

                    await _eventBusPublisher.PublishAsync(messages.Select(message => message.Event), ct);
                }

                // ReSharper disable once MethodSupportsCancellation - messages already published to the broker, try to delete them locally
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
            return DateTime.UtcNow - TimeSpan.FromSeconds(30);
        }
    }
}