using System;

namespace NetSockets.Server
{
    public class DataReceivedArgs : ClientDataArgs
    {
        public byte[] Data { get; set; }
    }
}
