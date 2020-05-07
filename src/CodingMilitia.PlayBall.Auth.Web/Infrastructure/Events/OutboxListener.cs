using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxListener
    {
        private readonly ILogger<OutboxListener> _logger;
        private readonly Channel<long> _messageIdChannel;

        public OutboxListener(ILogger<OutboxListener> logger)
        {
            _logger = logger;

            // If the consumer is slow, this should be a bounded channel to avoid memory growing indefinitely.
            // To make an informed decision we should instrument the code and gather metrics.
            _messageIdChannel = Channel.CreateUnbounded<long>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public void OnNewMessages(IEnumerable<long> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                // we don't care too much if it succeeds because we'll have a fallback to handle "forgotten" messages 
                if (!_messageIdChannel.Writer.TryWrite(messageId) && _logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Could not add outbox message {messageId} to the channel.", messageId);
                }
            }
        }

        public IAsyncEnumerable<long> GetAllMessageIdsAsync(CancellationToken ct)
            => _messageIdChannel.Reader.ReadAllAsync(ct);
    }
}