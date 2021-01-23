using System;

namespace NetSockets.Server
{
    public class DataReceivedArgs : EventArgs, IDisposable
    {
        public string ConnectionId { get; set; }
        public byte[] Message { get; set; }
        public Channel ThisChannel { get; set; }

        public void Dispose()
        {
            ((IDisposable)ThisChannel).Dispose();
        }
    }
}
