using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CodingMilitia.PlayBall.Auth.Web.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events
{
    public class OutboxPublisher
    {
        private readonly AuthDbContext _db;
        private readonly ILogger<OutboxPublisher> _logger;

        public OutboxPublisher(AuthDbContext db, ILogger<OutboxPublisher> logger)
        {
            _db = db;
            _logger = logger;
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

                // TODO: publish message to broker
                _logger.LogInformation(
                    $"Publishing message: {Environment.NewLine}{JsonSerializer.Serialize(message)}");

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
        }
    }
}