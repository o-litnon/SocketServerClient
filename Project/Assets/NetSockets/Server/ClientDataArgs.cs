using System;

namespace NetSockets.Server
{
    public class ClientDataArgs : EventArgs, IDisposable
    {
        public string ConnectionId { get; set; }
        public Channel ThisChannel { get; set; }

        public void Dispose()
        {
            ((IDisposable)ThisChannel).Dispose();
        }
    }
}
