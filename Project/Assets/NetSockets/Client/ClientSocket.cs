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
        private TcpClient tcpClient;
        private UdpClient udpClient;
        private readonly byte[] buffer;
        private IPEndPoint endpoint;

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
                throw new Exception("Client is already connected");

            tcpClient = new TcpClient
            {
                SendBufferSize = buffer.Length,
                ReceiveBufferSize = buffer.Length
            };
            udpClient = new UdpClient(endpoint);

            var tcpConnecting = tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);

            tcpConnecting.ContinueWith((task, sender) =>
            {
                _ = TcpListen();
            }, tcpConnecting);

            udpClient.Connect(endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            return tcpConnecting;
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

        private Task TcpListen()
        {
            return Task.Run(() =>
            {
                using (var stream = tcpClient.GetStream())
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

        public void OnDataIn(DataReceivedArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public void Close()
        {
            if (isConnected)
            {
                tcpClient.Close();
                udpClient.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}