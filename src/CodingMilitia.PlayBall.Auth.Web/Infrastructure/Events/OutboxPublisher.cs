using System;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Events;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxPublisher
    {
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

        public async Task PublishAsync(long messageId, CancellationToken ct)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var message = await db.Set<OutboxMessage>().FindAsync(new object[] {messageId}, ct);

                if (await TryDeleteMessageAsync(db, message, ct))
                {
                    await _eventPublisher.PublishAsync(message.Event.ToBusEvent(), ct);

                    // ReSharper disable once MethodSupportsCancellation - messages already published, try to delete them locally
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync(ct);
                }
            }
            catch (Exception)
            {
                // ReSharper disable once MethodSupportsCancellation - try to clean up things before letting go
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<bool> TryDeleteMessageAsync(AuthDbContext db, OutboxMessage message, CancellationToken ct)
        {
            try
            {
                db.Set<OutboxMessage>().Remove(message);
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogDebug($"Delete message {message.Id} failed, as it was done concurrently.");
                return false;
            }
        }
    }
}