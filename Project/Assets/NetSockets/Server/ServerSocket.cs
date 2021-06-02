using NetSockets.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public abstract class ServerSocket : ISocket, ISender
    {
        public bool Running { get; private set; }
        public readonly Channels ConnectedChannels;
        internal UdpSocket udpSocket;
        internal readonly int bufferSize;
        private readonly TcpListener Listener;

        public ServerSocket(int port, int bufferSize = 8192, int maxPlayers = -1) : this(IPAddress.Any, port, bufferSize, maxPlayers) { }
        public ServerSocket(string ip, int port, int bufferSize = 8192, int maxPlayers = -1) : this(IPAddress.Parse(ip), port, bufferSize, maxPlayers) { }
        public ServerSocket(IPAddress ip, int port, int bufferSize = 8192, int maxPlayers = -1)
        {
            var endpoint = new IPEndPoint(ip, port);
            this.bufferSize = bufferSize;
            ConnectedChannels = new Channels(this, maxPlayers);
            Listener = new TcpListener(endpoint);
        }

        ~ServerSocket()
        {
            udpSocket?.Dispose();
        }

        public virtual Task Open()
        {
            if (!Running)
            {
                Running = true;
                Listener.Start();
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

                udpSocket = new UdpSocket((IPEndPoint)Listener.LocalEndpoint);
                udpSocket.Client.ReceiveBufferSize = bufferSize;
                udpSocket.Listen(UdpDataRecieved);
            }

            return Task.CompletedTask;
        }

        private async void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            var channel = new Channel(this);

            await channel.Open(client);
        }

        private async Task UdpDataRecieved(SocketDataReceived e)
        {
            var channel = ConnectedChannels.OpenChannels.FirstOrDefault(d => d.Value.RemoteEndpoint.Equals(e.RemoteEndpoint));

            await OnDataIn(new DataReceivedArgs
            {
                Type = e.Type,
                Id = channel.Key,
                Channel = channel.Value,
                Data = e.Data
            });
        }

        public virtual async Task Close()
        {
            if (Running)
            {
                Running = false;

                await CloseAllConnections();

                Listener.Stop();
                udpSocket.Close();
            }
        }


        public Task Send(byte[] data, ConnectionType type)
        {
            var jobs = new List<Task>();

            foreach (var item in ConnectedChannels.ActiveChannels)
                jobs.Add(item.Value.Send(data, type));

            return Task.WhenAll(jobs);
        }
        public async Task Send(byte[] data, ConnectionType type, string id)
        {
            if (ConnectedChannels.ActiveChannels.TryGetValue(id, out Channel channel))
                await channel.Send(data, type);
        }
        public Task SendExcept(byte[] data, ConnectionType type, string id)
        {
            var jobs = new List<Task>();

            foreach (var item in ConnectedChannels.ActiveChannels.Where(d => !d.Key.Equals(id)))
                jobs.Add(item.Value.Send(data, type));

            return Task.WhenAll(jobs);
        }

        public Task CloseAllConnections()
        {
            var jobs = new List<Task>();

            foreach (var item in ConnectedChannels.OpenChannels)
                jobs.Add(item.Value.Close());

            return Task.WhenAll(jobs);
        }

        public async Task CloseConnection(string id)
        {
            if (ConnectedChannels.OpenChannels.TryGetValue(id, out Channel channel))
                await channel.Close();
        }

        public abstract Task OnDataIn(DataReceivedArgs e);
        public abstract Task OnClientConnected(ClientDataArgs e);
        public abstract Task OnClientActivated(ClientDataArgs e);
        public abstract Task OnClientDisconnected(ClientDataArgs e);
    }
}
