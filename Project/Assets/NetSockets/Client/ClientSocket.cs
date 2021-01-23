using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Client
{
    public class ClientSocket : IDisposable
    {
        public bool Running { get; set; }
        private readonly byte[] buffer;
        private IPEndPoint endpoint;
        private TcpClient tcpClient;
        private UdpClient udpClient;
        private NetworkStream stream;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool isConnected => tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected
            && udpClient != null && udpClient.Client != null && udpClient.Client.Connected;

        public ClientSocket(string ip, int port, int bufferSize)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            buffer = new byte[bufferSize];
        }

        public Task Open()
        {
            if (isConnected)
                return Task.CompletedTask;

            tcpClient = new TcpClient
            {
                SendBufferSize = buffer.Length,
                ReceiveBufferSize = buffer.Length
            };
            udpClient = new UdpClient(endpoint);

            var tcpConnecting = tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);

            tcpConnecting.ContinueWith((task, sender) => TcpListen(), tcpConnecting);

            udpClient.Connect(endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            return tcpConnecting;
        }

        public void Close()
        {
            if (isConnected)
            {
                tcpClient.Close();
                udpClient.Close();
            }
        }

        public void Send(byte[] data)
        {
            if (isConnected)
                stream.Write(data, 0, data.Length);
        }

        private void UdpReceiveCallback(IAsyncResult ar)
        {
            byte[] data = udpClient.EndReceive(ar, ref endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            var result = new DataReceivedArgs
            {
                Message = data
            };

            OnDataIn(result);
        }

        private void TcpListen()
        {
            Task.Run(() =>
            {
                using (stream = tcpClient.GetStream())
                {
                    int position;

                    while (isConnected)
                    {
                        while (isConnected && (position = stream.Read(buffer, 0, buffer.Length)) != 0)
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
            Close();
        }
    }
}