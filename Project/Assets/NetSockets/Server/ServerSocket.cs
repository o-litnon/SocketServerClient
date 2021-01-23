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
        public event EventHandler<ClientDataArgs> ClientConnected;
        public event EventHandler<ClientDataArgs> ClientDisconnected;
        public readonly Channels ConnectedChannels;
        public readonly int bufferSize;
        private readonly TcpListener Listener;

        public ServerSocket(string ip, int port, int bufferSize = 4096)
        {
            this.bufferSize = bufferSize;
            Listener = new TcpListener(IPAddress.Parse(ip), port);
            ConnectedChannels = new Channels(this);
        }

        public void Start()
        {
            Listener.Start();
            Running = true;
            Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);
        }

        private void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            Task.Run(() =>
            {
                var channel = new Channel(this, bufferSize);

                if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                    channel.Open(client);
            });
        }

        public void Stop()
        {
            Running = false;
            Channel current;

            foreach (var item in ConnectedChannels.OpenChannels.Keys)
                if (ConnectedChannels.OpenChannels.TryGetValue(item, out current))
                    current.Close();

            Listener.Stop();
        }

        internal Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { DataReceived?.Invoke(this, e); });
        }

        internal Task OnClientConnected(ClientDataArgs e)
        {
            return Task.Run(() => { ClientConnected?.Invoke(this, e); });
        }

        internal Task OnClientDisconnected(ClientDataArgs e)
        {
            return Task.Run(() => { ClientDisconnected?.Invoke(this, e); });
        }
    }
}