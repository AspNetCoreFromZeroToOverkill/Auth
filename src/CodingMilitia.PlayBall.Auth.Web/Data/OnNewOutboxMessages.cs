using System.Collections.Generic;

namespace CodingMilitia.PlayBall.Auth.Web.Data
{
    public delegate void OnNewOutboxMessages(IEnumerable<long> messageIds);

}