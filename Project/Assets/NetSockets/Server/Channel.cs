using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class Channel : ISender, IDisposable
    {
        public readonly string Id;
        public IPEndPoint RemoteEndpoint { get; private set; }
        private ServerSocket thisServer;
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

        public async Task Open(TcpClient client)
        {
            if (!thisServer.ConnectedChannels.TryAdd(Id, this))
                return;

            if (Running)
                return;

            thisClient = client;
            RemoteEndpoint = (IPEndPoint)thisClient.Client.RemoteEndPoint;
            stream = thisClient.GetStream();

            _ = Task.Run(async () =>
            {
                int position;

                while (Running && (position = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    var args = new DataReceivedArgs()
                    {
                        Message = buffer.Take(position).ToArray(),
                        Id = Id,
                        Channel = this
                    };

                    await thisServer.OnDataIn(args);
                }

                await Close();
            });

            await thisServer.OnClientConnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });

            await thisServer.ConnectedChannels.ActivatePending();
        }

        public Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!Running)
                return Task.CompletedTask;

            switch (type)
            {
                case ConnectionType.UDP:
                    return thisServer.udpClient.SendAsync(data, data.Length, RemoteEndpoint);
                case ConnectionType.TCP:
                default:
                    return stream.WriteAsync(data, 0, data.Length);
            }
        }

        public async Task Close()
        {
            stream.Close();
            thisClient.Close();
            thisClient.Dispose();
            thisServer.ConnectedChannels.TryRemove(Id, out Channel removedChannel);

            await thisServer.OnClientDisconnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });

            await thisServer.ConnectedChannels.ActivatePending();
        }

        public void Dispose()
        {
            Close().Wait();

            GC.SuppressFinalize(this);
        }
    }
}
