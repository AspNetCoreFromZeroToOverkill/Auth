using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events
{
    public class OutboxListener
    {
        private readonly AsyncAutoResetEvent _autoResetEvent = new AsyncAutoResetEvent();

        public void OnNewMessages() => _autoResetEvent.Set();

        public Task WaitForMessagesAsync(CancellationToken ct) => _autoResetEvent.WaitAsync(ct);
    }
}