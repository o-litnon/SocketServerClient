using System;
using System.Net;
using System.Threading.Tasks;
using NetSockets.Sockets;

namespace NetSockets.Client
{
    public class ClientSocket : ISocket, ISender
    {
        private int bufferSize;
        private IPEndPoint endpoint;
        private TcpSocket tcpSocket;
        private UdpSocket udpSocket;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool Running => tcpSocket != null && tcpSocket.Connected;

        public ClientSocket(int port = 25565, int bufferSize = 8192) : this(IPAddress.Loopback, port, bufferSize) { }
        public ClientSocket(IPAddress ip, int port = 25565, int bufferSize = 8192)
        {
            this.bufferSize = bufferSize;
            endpoint = new IPEndPoint(ip, port);
        }

        ~ClientSocket()
        {
            tcpSocket?.Dispose();
            udpSocket?.Dispose();
        }

        public virtual async Task Open()
        {
            if (Running)
                return;

            tcpSocket = new TcpSocket();
            tcpSocket.ReceiveBufferSize = bufferSize;

            await tcpSocket.ConnectAsync(endpoint.Address, endpoint.Port);
            tcpSocket.Listen(DataRecieved);

            udpSocket = new UdpSocket(tcpSocket.LocalEndPoint);
            udpSocket.Client.ReceiveBufferSize = bufferSize;

            udpSocket.Connect(endpoint);
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
                await OnDataIn(new DataReceivedArgs()
                {
                    Type = e.Type,
                    Data = e.Data
                });
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

        public virtual async Task OnDataIn(DataReceivedArgs e)
        {
            if (DataReceived != null)
                await Task.Run(() => { lock (DataReceived) DataReceived.Invoke(this, e); });
        }
    }
}