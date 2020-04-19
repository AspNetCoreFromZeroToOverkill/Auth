using System;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Shared.EventBus;
using Microsoft.EntityFrameworkCore.Storage;
using EventContracts = CodingMilitia.PlayBall.Auth.Events;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxPublisher
    {
        private readonly AuthDbContext _db;
        private readonly IEventPublisher<EventContracts.BaseAuthEvent> _eventPublisher;

        public OutboxPublisher(AuthDbContext db, IEventPublisher<EventContracts.BaseAuthEvent> eventPublisher)
        {
            _db = db;
            _eventPublisher = eventPublisher;
        }

        public async Task PublishAsync(long messageId, CancellationToken ct)
        {
            IDbContextTransaction transaction = null;
            
            try
            {
                transaction = await _db.Database.BeginTransactionAsync(ct);

                var message = await _db.Set<OutboxMessage>().FindAsync(new object[] {messageId}, ct);

                _db.Set<OutboxMessage>().Remove(message);
                
                await _db.SaveChangesAsync(ct);

                await _eventPublisher.PublishAsync(message.Event.ToExternal(), ct);

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
        }
    }
}