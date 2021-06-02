using NetSockets.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class Channel : ISender
    {
        public readonly string Id;
        public IPEndPoint RemoteEndpoint => tcpSocket?.RemoteEndPoint;
        private readonly ServerSocket thisServer;
        private TcpSocket tcpSocket;
        public bool Running => tcpSocket != null && tcpSocket.Connected;

        internal Channel(ServerSocket myServer)
        {
            thisServer = myServer;
            Id = Guid.NewGuid().ToString();
        }

        ~Channel()
        {
            tcpSocket?.Dispose();
        }

        internal async Task Open(TcpClient client)
        {
            if (!thisServer.ConnectedChannels.TryAdd(Id, this))
                return;

            if (Running)
                return;

            tcpSocket = new TcpSocket(client);
            tcpSocket.ReceiveBufferSize = thisServer.bufferSize;

            tcpSocket.Listen(DataRecieved);

            await thisServer.OnClientConnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });

            await thisServer.ConnectedChannels.ActivatePending();
        }

        private async Task DataRecieved(SocketDataReceived e)
        {
            if (e.Data.Length > 0)
                await thisServer.OnDataIn(new DataReceivedArgs()
                {
                    Type = e.Type,
                    Data = e.Data,
                    Id = Id,
                    Channel = this
                });
            else if (e.Type == ConnectionType.TCP)
                await Close();
        }

        public async Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return;

            switch (type)
            {
                case ConnectionType.UDP:
                    await thisServer.udpSocket.SendAsync(data, RemoteEndpoint);
                    break;
                case ConnectionType.TCP:
                default:
                    await tcpSocket.SendAsync(data);
                    break;
            }
        }

        public async Task Close()
        {
            tcpSocket.Close();
            thisServer.ConnectedChannels.TryRemove(Id, out Channel removedChannel);

            await thisServer.OnClientDisconnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });

            await thisServer.ConnectedChannels.ActivatePending();
        }
    }
}
