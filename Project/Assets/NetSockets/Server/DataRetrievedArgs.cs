using System;

namespace NetSockets.Server
{
    public class DataReceivedArgs : ClientDataArgs, IDisposable
    {
        public byte[] Message { get; set; }
    }
}
