using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal class UdpSocket : UdpClient
    {
        private Func<SocketDataReceived, Task> dataReceived;
        public IPEndPoint LocalEndPoint => (IPEndPoint)Client.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)Client.RemoteEndPoint;

        public UdpSocket(IPEndPoint iPEndPoint) : base(iPEndPoint) { }

        public async Task SendAsync(byte[] data, IPEndPoint endPoint = null)
        {
            var packet = new Packet(data);
            packet.WriteLength();
            var bytes = packet.ToArray();

            if (endPoint == null)
                await base.SendAsync(bytes, bytes.Length);
            else
                await base.SendAsync(bytes, bytes.Length, endPoint);
        }
        public void Listen(Func<SocketDataReceived, Task> dataReceived)
        {
            this.dataReceived = dataReceived;

            base.BeginReceive(UdpReceive, this);
        }

        private async void UdpReceive(IAsyncResult ar)
        {
            var remoteEndpoint = default(IPEndPoint);
            byte[] data = base.EndReceive(ar, ref remoteEndpoint);

            await HandleData(data, remoteEndpoint);

            base.BeginReceive(UdpReceive, this);
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
    }
}
