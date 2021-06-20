using System.Net;
using System.Threading.Tasks;
using NetSockets.Sockets;

namespace NetSockets.Client
{
    public abstract class ClientSocket : ISocket, ISender
    {
        private int bufferSize;
        private IPEndPoint endpoint;
        private TcpSocket tcpSocket;
        private UdpSocket udpSocket;

        public bool Running => tcpSocket != null && tcpSocket.Connected;

        public ClientSocket(int port = 25565, int bufferSize = 8192) : this(IPAddress.Loopback, port, bufferSize) { }
        public ClientSocket(IPAddress ip, int port = 25565, int bufferSize = 8192)
        {
            this.bufferSize = bufferSize;
            endpoint = new IPEndPoint(ip, port);
        }

        ~ClientSocket()
        {
            Close().Wait();
            tcpSocket?.Dispose();
            udpSocket?.Dispose();
        }

        public virtual async Task Open()
        {
            if (Running)
                return;

            tcpSocket = new TcpSocket();
            tcpSocket.ReceiveBufferSize = bufferSize;

            await tcpSocket.ConnectAsync(endpoint);
            tcpSocket.Listen(DataRecieved);

            udpSocket = new UdpSocket(tcpSocket.LocalEndPoint);
            udpSocket.ReceiveBufferSize = bufferSize;

            await udpSocket.ConnectAsync(endpoint);
            udpSocket.Listen(DataRecieved);
        }

        public virtual async Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return;

            switch (type)
            {
                case ConnectionType.UDP:
                    await udpSocket.SendAsync(data);
                    break;
                case ConnectionType.TCP:
                default:
                    await tcpSocket.SendAsync(data);
                    break;
            }
        }

        private async Task DataRecieved(SocketDataReceived e)
        {
            if (e.Data.Length > 0)
                _ = Task.Run(() => OnDataIn(new DataReceivedArgs()
                {
                    Type = e.Type,
                    Data = e.Data
                }));
            else if (e.Type == ConnectionType.TCP)
                await Close();
        }

        public virtual Task Close()
        {
            if (Running)
            {
                udpSocket.Close();
                tcpSocket.Close();
            }

            return Task.CompletedTask;
        }

        public abstract void OnDataIn(DataReceivedArgs e);
    }
}