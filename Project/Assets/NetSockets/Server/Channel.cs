using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class Channel : ISender
    {
        public readonly string Id;
        public IPEndPoint RemoteEndpoint { get; private set; }
        private readonly ServerSocket thisServer;
        private readonly byte[] buffer;
        private TcpClient thisClient;
        private NetworkStream stream;
        public bool Running => thisClient != null && thisClient.Client != null && thisClient.Client.Connected;

        public Channel(ServerSocket myServer, int bufferSize)
        {
            thisServer = myServer;
            buffer = new byte[bufferSize];
            Id = Guid.NewGuid().ToString();
        }

        ~Channel()
        {
            thisClient?.Dispose();
        }

        public async Task Open(TcpClient client)
        {
            if (!thisServer.ConnectedChannels.TryAdd(Id, this))
                return;

            if (Running)
                return;

            thisClient = client;
            RemoteEndpoint = (IPEndPoint)thisClient.Client.RemoteEndPoint;

            StartListeners();

            await thisServer.OnClientConnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });

            await thisServer.ConnectedChannels.ActivatePending();
        }

        private void StartListeners()
        {
            Task.Run(async () =>
            {
                using (stream = thisClient.GetStream())
                {
                    int position;

                    while (Running && (position = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var args = new DataReceivedArgs()
                        {
                            Data = buffer.Take(position).ToArray(),
                            Id = Id,
                            Channel = this
                        };

                        await thisServer.OnDataIn(args);
                    }

                    await Close();
                }
            });
        }

        public async Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return;

            switch (type)
            {
                case ConnectionType.UDP:
                    await thisServer.udpClient.SendAsync(data, data.Length, RemoteEndpoint);
                    break;
                case ConnectionType.TCP:
                default:
                    await stream.WriteAsync(data, 0, data.Length);
                    break;
            }
        }

        public async Task Close()
        {
            stream.Close();
            thisClient.Close();
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
