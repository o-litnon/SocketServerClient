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
        public bool Running => tcpClient != null && tcpClient.Connected;

        public ClientSocket(string ip, int port, int bufferSize)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            buffer = new byte[bufferSize];
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

        public virtual async Task Close()
        {
            if (Running)
                await Task.Run(() =>
                {
                    tcpClient.Close();
                    udpClient.Close();
                });
        }

        public virtual async Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return;

            switch (type)
            {
                case ConnectionType.UDP:
                    await udpClient.SendAsync(data, data.Length, endpoint);
                    break;
                case ConnectionType.TCP:
                default:
                    await stream.WriteAsync(data, 0, data.Length);
                    break;
            }
        }

        private void StartListeners()
        {
            udpClient = new UdpClient((IPEndPoint)tcpClient.Client.LocalEndPoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            Task.Run(async () =>
            {
                using (stream = tcpClient.GetStream())
                {
                    int position;

                    while (Running && (position = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var args = new DataReceivedArgs()
                        {
                            Message = buffer.Take(position).ToArray()
                        };

                        await OnDataIn(args);
                    }

                    await Close();
                }
            });
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

        private Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => DataReceived?.Invoke(this, e));
        }

        public virtual void Dispose()
        {
            Close().Wait();
            tcpClient.EndConnect(null);
        }
    }
}