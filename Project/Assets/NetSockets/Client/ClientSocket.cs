using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Client
{
    public class ClientSocket : ISocket, ISender, IDisposable
    {
        private readonly byte[] buffer;
        private IPEndPoint endpoint;
        private TcpClient tcpClient;
        public UdpClient udpClient;
        private NetworkStream stream;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool Running => tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected
            && (udpClient == null || udpClient.Client != null && udpClient.Client.Connected);

        public ClientSocket(string ip, int port, int bufferSize)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            buffer = new byte[bufferSize];
        }

        public Task Open()
        {
            if (Running)
                return Task.CompletedTask;

            tcpClient = new TcpClient
            {
                SendBufferSize = buffer.Length,
                ReceiveBufferSize = buffer.Length
            };

            var tcpConnecting = tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);

            tcpConnecting.ContinueWith((task, sender) =>
            {
                UdpListen();
                TcpListen();
            }, tcpConnecting);

            return tcpConnecting;
        }

        public virtual Task Close()
        {
            if (!Running)
                return Task.CompletedTask;
            else
                return Task.Run(() =>
                {
                    tcpClient.Close();
                    udpClient.Close();
                });
        }

        public Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return Task.CompletedTask;

            switch (type)
            {
                case ConnectionType.UDP:
                    return udpClient.SendAsync(data, data.Length, endpoint);
                case ConnectionType.TCP:
                default:
                    return stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async void UdpReceiveCallback(IAsyncResult ar)
        {
            byte[] data = udpClient.EndReceive(ar, ref endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            var result = new DataReceivedArgs
            {
                Message = data
            };

            await OnDataIn(result);
        }
        private void UdpListen()
        {
            udpClient = new UdpClient((IPEndPoint)tcpClient.Client.LocalEndPoint);

            udpClient.Connect(endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);
        }
        private void TcpListen()
        {
            Task.Run(() =>
            {
                using (stream = tcpClient.GetStream())
                {
                    int position;

                    while (Running)
                    {
                        while (Running && (position = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            var args = new DataReceivedArgs()
                            {
                                Message = buffer.Take(position).ToArray()
                            };

                            OnDataIn(args);
                        }
                    }
                }
            });
        }

        private Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => DataReceived?.Invoke(this, e));
        }

        public void Dispose()
        {
            Close().Wait();
        }
    }
}