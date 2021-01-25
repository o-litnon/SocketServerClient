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

        public ServerSocket(string ip, int port, int bufferSize = 4096, int maxPlayers = 0)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.bufferSize = bufferSize;
            ConnectedChannels = new Channels(this, maxPlayers);
            Listener = new TcpListener(endpoint);
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

            Task.Run(async () =>
            {
                using (udpClient = new UdpClient((IPEndPoint)Listener.LocalEndpoint))
                {
                    UdpReceiveResult data;
                    KeyValuePair<string, Channel> channel;
                    while (Running)
                    {
                        data = await udpClient.ReceiveAsync();
                        channel = ConnectedChannels.OpenChannels.FirstOrDefault(d => d.Value.RemoteEndpoint.Equals(data.RemoteEndPoint));

                        await OnDataIn(new DataReceivedArgs
                        {
                            Id = channel.Key,
                            Channel = channel.Value,
                            Data = data.Buffer
                        });
                    }
                }
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

        public virtual async Task Close()
        {
            if (Running)
            {
                Running = false;

                foreach (var item in ConnectedChannels.OpenChannels.Keys)
                    if (ConnectedChannels.OpenChannels.TryGetValue(item, out Channel current))
                        await current.Close();

                Listener.Stop();
                udpClient.Close();
            }
        }

        internal Task OnDataIn(DataReceivedArgs e)
        {
            lock (DataReceived)
                return Task.Run(() => { DataReceived?.Invoke(this, e); });
        }

        internal Task OnClientConnected(ClientDataArgs e)
        {
            lock (ClientConnected)
                return Task.Run(() => { ClientConnected?.Invoke(this, e); });
        }

        internal Task OnClientActivated(ClientDataArgs e)
        {
            lock (ClientActivated)
                return Task.Run(() => { ClientActivated?.Invoke(this, e); });
        }

        internal Task OnClientDisconnected(ClientDataArgs e)
        {
            lock (ClientDisconnected)
                return Task.Run(() => { ClientDisconnected?.Invoke(this, e); });
        }
    }
}