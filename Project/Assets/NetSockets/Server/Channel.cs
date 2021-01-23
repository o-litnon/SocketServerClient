using System;
using System.Linq;
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
        private bool isOpen;

        public Channel(ServerSocket myServer, int bufferSize)
        {
            thisServer = myServer;
            buffer = new byte[bufferSize];
            Id = Guid.NewGuid().ToString();
        }

        public Task Open(TcpClient client)
        {
            if (isOpen)
                throw new Exception($"Channel {Id} is already open.");

            isOpen = true;
            thisClient = client;

            return Task.Run(() =>
            {
                using (stream = thisClient.GetStream())
                {
                    int position;

                    while (isOpen)
                    {
                        while (isOpen && (position = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            var args = new DataReceivedArgs()
                            {
                                Message = buffer.Take(position).ToArray(),
                                ConnectionId = Id,
                                ThisChannel = this
                            };

                            thisServer.OnDataIn(args);
                        }

                        Close();
                    }
                }
            });
        }

        public void Send(byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        public void Close()
        {
            isOpen = false;
            thisServer.ConnectedChannels.OpenChannels.TryRemove(Id, out Channel removedChannel);
            Dispose();
        }

        public void Dispose()
        {
            stream.Close();
            thisClient.Close();
            thisClient.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
