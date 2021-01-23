using System;

namespace NetSockets.Server
{
    public class ClientDataArgs : EventArgs
    {
        public string Id { get; set; }
        public Channel Channel { get; set; }
    }
}
