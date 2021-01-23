using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class ServerSocket
    {
        public bool Running { get; private set; }
        public event EventHandler<DataReceivedArgs> DataReceived;
        public event EventHandler<ClientDataArgs> ClientReceived;
        private TcpListener Listener;
        public Channels ConnectedChannels { get; private set; }
        public readonly int bufferSize;

        public ServerSocket(string ip, int port, int bufferSize = 4096)
        {
            this.bufferSize = bufferSize;
            Listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            Listener.Start();
            Running = true;
            ConnectedChannels = new Channels(this);
            Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);
        }

        private async void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            await Task.Run(() =>
            {
                var channel = new Channel(this, bufferSize);

                if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                {
                    _ = channel.Open(client);

                    OnClientIn(new ClientDataArgs
                    {
                        ConnectionId = channel.Id,
                        ThisChannel = channel
                    });
                }
            }); ;
        }

        public void Stop()
        {
            Running = false;

            Listener.Stop();
        }

        public void OnDataIn(DataReceivedArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public void OnClientIn(ClientDataArgs e)
        {
            ClientReceived?.Invoke(this, e);
        }
    }
}