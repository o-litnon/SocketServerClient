﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Sockets
{
    internal class TcpSocket
    {
        private readonly TcpClient tcpClient;
        public bool Connected => tcpClient.Client != null && tcpClient.Client.Connected;

        public IPEndPoint RemoteEndPoint => (IPEndPoint)tcpClient.Client.RemoteEndPoint;
        public IPEndPoint LocalEndPoint => (IPEndPoint)tcpClient.Client.LocalEndPoint;

        private Func<SocketDataReceived, Task> dataReceived;
        private byte[] buffer;
        private NetworkStream stream;
        private Packet receivedData;
        public int ReceiveBufferSize
        {
            get => tcpClient.ReceiveBufferSize;
            set
            {
                tcpClient.ReceiveBufferSize = value;
                buffer = new byte[value];
            }
        }
        public int SendBufferSize
        {
            get => tcpClient.SendBufferSize;
            set => tcpClient.SendBufferSize = value;
        }

        public TcpSocket(TcpClient client) : base()
        {
            this.tcpClient = client;
            buffer = new byte[tcpClient.ReceiveBufferSize];
        }
        public TcpSocket() : base()
        {
            tcpClient = new TcpClient();
            buffer = new byte[tcpClient.ReceiveBufferSize];
        }
        public Task ConnectAsync(IPAddress address, int port)
        {
            return tcpClient.ConnectAsync(address, port);
        }

        public void Listen(Func<SocketDataReceived, Task> dataReceived)
        {
            receivedData?.Dispose();
            receivedData = new Packet();

            this.dataReceived = dataReceived;

            stream = tcpClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, this);
        }

        public async Task SendAsync(byte[] data)
        {
            if (this.Connected)
            {
                var packet = new Packet(data);
                packet.WriteLength();
                var bytes = packet.ToArray();

                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private async void TcpReceive(IAsyncResult ar)
        {
            int position = stream.EndRead(ar);

            var data = buffer.Take(position).ToArray();

            await HandleData(data);

            if (Connected)
                stream.BeginRead(buffer, 0, buffer.Length, TcpReceive, this);
        }

        private async Task HandleData(byte[] data)
        {
            receivedData.SetBytes(data);

            int length = 0;

            for (length = packetLength(receivedData);
                length > 0 && length <= receivedData.UnreadLength();
                length = packetLength(receivedData))
            {
                await dataReceived?.Invoke(new SocketDataReceived
                {
                    RemoteEndpoint = RemoteEndPoint,
                    Type = ConnectionType.TCP,
                    Data = receivedData.ReadBytes(length)
                });
            }

            receivedData.Reset(length <= 0);
        }

        private int packetLength(Packet packet)
        {
            if (packet.UnreadLength() >= 4)
                return packet.ReadInt();
            else
                return 0;
        }

        public void Close()
        {
            tcpClient.Close();
            stream?.Close();
        }

        public void Dispose()
        {
            tcpClient.Dispose();
            stream?.Dispose();
            receivedData?.Dispose();
        }
    }
}
