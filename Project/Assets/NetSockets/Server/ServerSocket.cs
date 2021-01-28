using System;
using System.Collections.Generic;
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
        internal UdpClient udpClient;

        public ServerSocket(int port, int bufferSize = 4096, int maxPlayers = -1) : this(IPAddress.Any, port, bufferSize, maxPlayers) { }
        public ServerSocket(string ip, int port, int bufferSize = 4096, int maxPlayers = -1) : this(IPAddress.Parse(ip), port, bufferSize, maxPlayers) { }
        public ServerSocket(IPAddress ip, int port, int bufferSize = 4096, int maxPlayers = -1)
        {
            var endpoint = new IPEndPoint(ip, port);
            this.bufferSize = bufferSize;
            ConnectedChannels = new Channels(this, maxPlayers);
            Listener = new TcpListener(endpoint);
        }

        ~ServerSocket()
        {
            udpClient?.Dispose();
        }

        public virtual Task Open()
        {
            if (!Running)
                StartListeners();

            return Task.CompletedTask;
        }
        private void StartListeners()
        {
            Running = true;
            Listener.Start();
            Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            udpClient = new UdpClient((IPEndPoint)Listener.LocalEndpoint);
            udpClient.BeginReceive(UdpReceive, udpClient);
        }

        private async void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            var channel = new Channel(this, bufferSize);

            await channel.Open(client);
        }

        private async void UdpReceive(IAsyncResult ar)
        {
            var remoteEndpoint = default(IPEndPoint);
            byte[] data = udpClient.EndReceive(ar, ref remoteEndpoint);

            if (Running)
                udpClient.BeginReceive(UdpReceive, udpClient);

            var channel = ConnectedChannels.OpenChannels.FirstOrDefault(d => d.Value.RemoteEndpoint.Equals(remoteEndpoint));

            await OnDataIn(new DataReceivedArgs
            {
                Type = ConnectionType.UDP,
                Id = channel.Key,
                Channel = channel.Value,
                Data = data
            });
        }

        public virtual async Task Close()
        {
            if (Running)
            {
                Running = false;

                var jobs = new List<Task>();

                foreach (var item in ConnectedChannels.OpenChannels.Keys)
                    if (ConnectedChannels.OpenChannels.TryGetValue(item, out Channel current))
                        jobs.Add(current.Close());

                await Task.WhenAll(jobs);

                Listener.Stop();
                udpClient.Close();
            }
        }

        internal Task OnDataIn(DataReceivedArgs e)
        {
            return Task.Run(() => { lock (DataReceived) DataReceived?.Invoke(this, e); });
        }

        internal Task OnClientConnected(ClientDataArgs e)
        {
            return Task.Run(() => { lock (ClientConnected) ClientConnected?.Invoke(this, e); });
        }

        internal Task OnClientActivated(ClientDataArgs e)
        {
            return Task.Run(() => { lock (ClientActivated) ClientActivated?.Invoke(this, e); });
        }

        internal Task OnClientDisconnected(ClientDataArgs e)
        {
            return Task.Run(() => { lock (ClientDisconnected) ClientDisconnected?.Invoke(this, e); });
        }
    }
}
