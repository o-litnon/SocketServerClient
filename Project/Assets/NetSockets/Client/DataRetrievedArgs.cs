using System;

namespace NetSockets.Client
{
    public class DataReceivedArgs : EventArgs
    {
        public ConnectionType Type { get; set; }
        public byte[] Data { get; set; }
    }
}
