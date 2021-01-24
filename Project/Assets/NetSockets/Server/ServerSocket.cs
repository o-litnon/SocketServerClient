using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class ServerSocket : ISocket
    {
        public bool Running { get; private set; }
        public event EventHandler<DataReceivedArgs> DataReceived;
        public event EventHandler<ClientDataArgs> ClientConnected;
        public event EventHandler<ClientDataArgs> ClientActivated;
        public event EventHandler<ClientDataArgs> ClientDisconnected;
        public readonly Channels ConnectedChannels;
        public readonly int bufferSize;
        private readonly TcpListener Listener;
        internal readonly UdpClient udpClient;

        public ServerSocket(string ip, int port, int bufferSize = 4096, int maxPlayers = 0)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.bufferSize = bufferSize;
            ConnectedChannels = new Channels(this, maxPlayers);
            Listener = new TcpListener(endpoint);
            udpClient = new UdpClient(endpoint);
        }

        public virtual Task Open()
        {
            return Task.Run(() =>
            {
                Listener.Start();
                Running = true;
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);
                udpClient.BeginReceive(UdpReceiveCallback, udpClient);
            });
        }

        private async void UdpReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(ar, ref clientEndPoint);

            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            var channel = ConnectedChannels.OpenChannels.FirstOrDefault(d => d.Value.RemoteEndpoint.Equals(clientEndPoint));

            await OnDataIn(new DataReceivedArgs
            {
                Id = channel.Key,
                Channel = channel.Value,
                Message = data
            });
        }

        private async void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            var channel = new Channel(this, bufferSize);

            await channel.Open(client);
        }

        public virtual Task Close()
        {
            return Task.Run(async () =>
            {
                Running = false;

                foreach (var item in ConnectedChannels.OpenChannels.Keys)
                    if (ConnectedChannels.OpenChannels.TryGetValue(item, out Channel current))
                        await current.Close();

                Listener.Stop();
            });
        }

        internal virtual Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { DataReceived?.Invoke(this, e); });
        }

        internal virtual Task OnClientConnected(ClientDataArgs e)
        {
            return Task.Run(() => { ClientConnected?.Invoke(this, e); });
        }

        internal virtual Task OnClientActivated(ClientDataArgs e)
        {
            return Task.Run(() => { ClientActivated?.Invoke(this, e); });
        }

        internal virtual Task OnClientDisconnected(ClientDataArgs e)
        {
            return Task.Run(() => { ClientDisconnected?.Invoke(this, e); });
        }
    }
}