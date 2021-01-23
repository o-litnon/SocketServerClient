using System;

namespace NetSockets.Server
{
    public class DataReceivedArgs : ClientDataArgs
    {
        public byte[] Message { get; set; }
    }
}
