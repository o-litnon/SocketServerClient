using NetSockets.Client;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal class UdpSocket : UdpClient
    {

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool Connected => base.Client.Connected;

        public UdpSocket(IPEndPoint iPEndPoint) : base(iPEndPoint)
        {
            Connect(iPEndPoint);
            BeginReceive(UdpReceive, this);
        }

        public async Task Send(byte[] data)
        {
            if (this.Connected)
            {
                var packet = new Packet(data);
                packet.WriteLength();
                var bytes = packet.ToArray();

                await SendAsync(bytes, bytes.Length);
            }
        }

        private void UdpReceive(IAsyncResult ar)
        {
            var remoteEndpoint = default(IPEndPoint);
            byte[] data = EndReceive(ar, ref remoteEndpoint);

            if (Connected)
                BeginReceive(UdpReceive, this);

            HandleData(data);
        }

        private void HandleData(byte[] data)
        {
            using (Packet _packet = new Packet(data))
            {
                int length = packetLength(_packet);

                if (length > 0 && length <= _packet.UnreadLength())
                {
                    _ = OnDataIn(new DataReceivedArgs
                    {
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

        private Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { lock (DataReceived) DataReceived?.Invoke(this, e); });
        }
    }
}
