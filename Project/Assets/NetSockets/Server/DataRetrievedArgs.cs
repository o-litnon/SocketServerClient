using System;

namespace NetSockets.Server
{
    public class DataReceivedArgs : ClientDataArgs
    {
        public ConnectionType Type { get; set; }
        public byte[] Data { get; set; }
    }
}
