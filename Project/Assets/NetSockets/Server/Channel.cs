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
        private byte[] buffer;
        private NetworkStream stream;
        private bool isOpen;
        private bool disposed;

        public Channel(ServerSocket myServer)
        {
            thisServer = myServer;
            buffer = new byte[256];
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
                        while (buffer.Length < stream.Length)
                            buffer = new byte[buffer.Length * 2];

                        while (isOpen && (position = stream.Read(buffer, 0, (int)stream.Length)) != 0)
                        {
                            var args = new DataReceivedArgs()
                            {
                                Message = buffer.ToArray(),
                                ConnectionId = Id,
                                ThisChannel = this
                            };

                            thisServer.OnDataIn(args);
                        }
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
            Dispose(false);
            isOpen = false;
            thisServer.ConnectedChannels.OpenChannels.TryRemove(Id, out Channel removedChannel);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                stream.Close();
                thisClient.Close();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
