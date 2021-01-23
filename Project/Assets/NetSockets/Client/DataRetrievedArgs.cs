using System;

namespace NetSockets.Client
{
    public class DataReceivedArgs : EventArgs
    {
        public byte[] Message { get; set; }
    }
}
