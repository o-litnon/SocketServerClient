using System;

namespace NetSockets.Client
{
    public class DataReceivedArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
}
