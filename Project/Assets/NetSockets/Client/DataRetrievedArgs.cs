using System;

namespace NetSockets.Client
{
    public class DataReceivedArgs : EventArgs, IDisposable
    {
        public byte[] Message { get; set; }

        public void Dispose()
        {

        }
    }
}
