using System;
using System.Net;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal interface IHttpSocket : IDisposable
    {
        bool Connected { get; }
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        void Listen(Func<SocketDataReceived, Task> dataReceived);
        Task SendAsync(byte[] data);
        Task ConnectAsync(IPEndPoint endPoint);
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        void Close();
    }
}
