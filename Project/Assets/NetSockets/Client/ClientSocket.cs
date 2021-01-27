using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Client
{
    public class ClientSocket : ISocket, ISender
    {
        private readonly byte[] buffer;
        private IPEndPoint endpoint;
        private TcpClient tcpClient;
        public UdpClient udpClient;
        private NetworkStream stream;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool Running => tcpClient != null && tcpClient.Connected;

        public ClientSocket(string ip, int port, int bufferSize)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            buffer = new byte[bufferSize];
        }

        ~ClientSocket()
        {
            stream?.Dispose();
            tcpClient?.Dispose();
            udpClient?.Dispose();
        }

        public virtual async Task Open()
        {
            if (Running)
                return;

            tcpClient = new TcpClient
            {
                SendBufferSize = buffer.Length,
                ReceiveBufferSize = buffer.Length
            };

            await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);

            StartListeners();
        }

        private void StartListeners()
        {
            stream = tcpClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, tcpClient);

            udpClient = new UdpClient((IPEndPoint)tcpClient.Client.LocalEndPoint);
            udpClient.Connect(endpoint);
            udpClient.BeginReceive(UdpReceive, udpClient);
        }

        public virtual async Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return;

            switch (type)
            {
                case ConnectionType.UDP:
                    await udpClient.SendAsync(data, data.Length);
                    break;
                case ConnectionType.TCP:
                default:
                    await stream.WriteAsync(data, 0, data.Length);
                    break;
            }
        }

        private async void TcpReceive(IAsyncResult ar)
        {
            int position = stream.EndRead(ar);

            if (position == 0)
            {
                await Close();
                return;
            }

            await OnDataIn(new DataReceivedArgs()
            {
                Type = ConnectionType.TCP,
                Data = buffer.Take(position).ToArray()
            });

            if (Running)
                stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, tcpClient);
        }

        private async void UdpReceive(IAsyncResult ar)
        {
            var remoteEndpoint = default(IPEndPoint);
            byte[] data = udpClient.EndReceive(ar, ref remoteEndpoint);

            if (Running)
                udpClient.BeginReceive(UdpReceive, udpClient);

            await OnDataIn(new DataReceivedArgs()
            {
                Type = ConnectionType.UDP,
                Data = data
            });
        }

        public virtual Task Close()
        {
            if (Running)
            {
                udpClient.Close();
                tcpClient.Close();
            }

            return Task.CompletedTask;
        }

        private Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { lock (DataReceived) DataReceived?.Invoke(this, e); });
        }
    }
}