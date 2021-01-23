using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class Channel : IDisposable
    {
        private ServerSocket thisServer;
        public readonly string Id;
        private TcpClient thisClient;
        private readonly byte[] buffer;
        private NetworkStream stream;
        public bool isConnected => thisClient != null && thisClient.Client != null && thisClient.Client.Connected;

        public Channel(ServerSocket myServer, int bufferSize)
        {
            thisServer = myServer;
            buffer = new byte[bufferSize];
            Id = Guid.NewGuid().ToString();
        }

        public void Open(TcpClient client)
        {
            if (isConnected)
                return;

            thisClient = client;

            Task.Run(() =>
            {
                using (stream = thisClient.GetStream())
                {
                    thisServer.OnClientConnected(new ClientDataArgs
                    {
                        Id = Id,
                        Channel = this
                    });

                    int position;

                    while (isConnected && (position = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        var args = new DataReceivedArgs()
                        {
                            Message = buffer.Take(position).ToArray(),
                            Id = Id,
                            Channel = this
                        };

                        thisServer.OnDataIn(args);
                    }

                    Close();
                }
            });
        }

        public Task Send(byte[] data, ConnectionType type = ConnectionType.TCP)
        {
            if (!isConnected)
                return Task.CompletedTask;

            switch (type)
            {
                case ConnectionType.UDP:
                    return thisServer.udpClient.SendAsync(data, data.Length, (IPEndPoint)thisClient.Client.RemoteEndPoint);
                case ConnectionType.TCP:
                default:
                    return stream.WriteAsync(data, 0, data.Length);
            }
        }

        public void Close()
        {
            stream.Close();
            thisClient.Close();
            thisClient.Dispose();
            thisServer.ConnectedChannels.OpenChannels.TryRemove(Id, out Channel removedChannel);

            thisServer.OnClientDisconnected(new ClientDataArgs
            {
                Id = Id,
                Channel = this
            });
        }

        public void Dispose()
        {
            Close();

            GC.SuppressFinalize(this);
        }
    }
}
