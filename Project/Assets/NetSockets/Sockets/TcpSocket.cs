using NetSockets.Client;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal class TcpSocket : TcpClient
    {
        public IPEndPoint RemoteEndpoint => (IPEndPoint)this.Client.RemoteEndPoint;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public event EventHandler OnClose;
        private byte[] buffer;
        private NetworkStream stream;
        private Packet receivedData;

        public TcpSocket(int bufferSize)
        {
            SendBufferSize = bufferSize;
            ReceiveBufferSize = bufferSize;

            buffer = new byte[ReceiveBufferSize];
            receivedData = new Packet();
        }

        public void Start()
        {
            stream = GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, this);
        }

        public void Stop()
        {
            TryCloseStream();
        }

        public async Task Send(byte[] data)
        {
            if (this.Connected)
            {
                var packet = new Packet(data);
                packet.WriteLength();
                var bytes = packet.ToArray();

                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private void TcpReceive(IAsyncResult ar)
        {
            int position = stream.EndRead(ar);

            if (position == 0)
            {
                Close();
                return;
            }

            var data = buffer.Take(position).ToArray();
            receivedData.Reset(HandleData(data));

            if (Connected)
                stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, this);
        }

        private bool HandleData(byte[] data)
        {
            receivedData.SetBytes(data);

            int length = 0;

            for (length = packetLength(receivedData);
                length > 0 && length <= receivedData.UnreadLength();
                length = packetLength(receivedData))
            {
                _ = OnDataIn(new DataReceivedArgs
                {
                    Type = ConnectionType.TCP,
                    Data = receivedData.ReadBytes(length)
                });
            }

            return length <= 0;
        }

        private int packetLength(Packet packet)
        {
            if (receivedData.UnreadLength() >= 4)
                return receivedData.ReadInt();
            else
                return 0;
        }

        private Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { lock (DataReceived) DataReceived?.Invoke(this, e); });
        }

        private Task onClose()
        {
            return Task.Run(() => { lock (OnClose) OnClose?.Invoke(this, null); });
        }

        public new void Close()
        {
            base.Close();
            TryCloseStream();
            _ = onClose();
        }

        private void TryCloseStream()
        {
            if (stream != null)
                stream.Close();
        }

        ~TcpSocket()
        {
            Close();
        }
    }
}
