using System;
using System.Net;

namespace NetSockets.Sockets
{
    internal class SocketDataReceived : EventArgs
    {
        public IPEndPoint RemoteEndpoint { get; set; }
        public ConnectionType Type { get; set; }
        public byte[] Data { get; set; }
    }
}
