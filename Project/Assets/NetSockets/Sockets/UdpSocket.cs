using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal class UdpSocket : IHttpSocket
    {
        private readonly UdpClient udpClient;
        private Func<SocketDataReceived, Task> dataReceived;
        public IPEndPoint LocalEndPoint => (IPEndPoint)udpClient.Client.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)udpClient.Client.RemoteEndPoint;
        public bool Connected => udpClient.Client != null && udpClient.Client.Connected;

        public int ReceiveBufferSize
        {
            get => udpClient.Client.ReceiveBufferSize;
            set => udpClient.Client.ReceiveBufferSize = value;
        }
        public int SendBufferSize
        {
            get => udpClient.Client.SendBufferSize;
            set => udpClient.Client.SendBufferSize = value;
        }
        public UdpSocket(IPEndPoint localEP)
        {
            udpClient = new UdpClient(localEP);
        }

        public Task ConnectAsync(IPEndPoint endpoint)
        {
            udpClient.Connect(endpoint);
            return Task.CompletedTask;
        }

        public Task SendAsync(byte[] data) => SendAsync(data, null);
        public async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            var packet = new Packet(data);
            packet.WriteLength();
            var bytes = packet.ToArray();

            if (endPoint == null)
                await udpClient.SendAsync(bytes, bytes.Length);
            else
                await udpClient.SendAsync(bytes, bytes.Length, endPoint);
        }
        public void Listen(Func<SocketDataReceived, Task> dataReceived)
        {
            this.dataReceived = dataReceived;

            udpClient.BeginReceive(UdpReceive, this);
        }

        private async void UdpReceive(IAsyncResult ar)
        {
            var remoteEndpoint = default(IPEndPoint);
            byte[] data = udpClient.EndReceive(ar, ref remoteEndpoint);

            await HandleData(data, remoteEndpoint);

            udpClient.BeginReceive(UdpReceive, this);
        }
        private async Task HandleData(byte[] data, IPEndPoint remoteEndpoint)
        {
            using (Packet _packet = new Packet(data))
            {
                int length = packetLength(_packet);

                if (length > 0 && length <= _packet.UnreadLength())
                {
                    await dataReceived.Invoke(new SocketDataReceived
                    {
                        RemoteEndpoint = remoteEndpoint,
                        Type = ConnectionType.UDP,
                        Data = _packet.ReadBytes(length)
                    });
                }
            }
        }
        private int packetLength(Packet packet)
        {
            if (packet.UnreadLength() >= 4)
                return packet.ReadInt();
            else
                return 0;
        }

        public void Dispose()
        {
            udpClient.Dispose();
        }

        public void Close()
        {
            udpClient.Close();
        }
    }
}
