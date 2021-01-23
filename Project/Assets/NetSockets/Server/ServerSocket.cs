using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class ServerSocket: ISocket
    {
        public bool Running { get; private set; }
        public event EventHandler<DataReceivedArgs> DataReceived;
        public event EventHandler<ClientDataArgs> ClientConnected;
        public event EventHandler<ClientDataArgs> ClientDisconnected;
        public readonly Channels ConnectedChannels;
        public readonly int bufferSize;
        private readonly TcpListener Listener;
        internal readonly UdpClient udpClient;

        public ServerSocket(string ip, int port, int bufferSize = 4096)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.bufferSize = bufferSize;
            ConnectedChannels = new Channels(this);
            Listener = new TcpListener(endpoint);
            udpClient = new UdpClient(endpoint);
        }

        public Task Open()
        {
            return Task.Run(() => {
                Listener.Start();
                Running = true;
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);
                udpClient.BeginReceive(UdpReceiveCallback, udpClient);
            });
        }

        private void UdpReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(ar, ref clientEndPoint);

            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            OnDataIn(new DataReceivedArgs
            {
                Message = data
            });
        }

        private void TcpClientConnect(IAsyncResult ar)
        {
            TcpClient client = Listener.EndAcceptTcpClient(ar);

            if (Running)
                Listener.BeginAcceptTcpClient(TcpClientConnect, Listener);

            Task.Run(async () =>
            {
                var channel = new Channel(this, bufferSize);

                if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                    await channel.Open(client);
            });
        }

        public Task Close()
        {
            return Task.Run(async () => {
                Running = false;
                Channel current;

                foreach (var item in ConnectedChannels.OpenChannels.Keys)
                    if (ConnectedChannels.OpenChannels.TryGetValue(item, out current))
                        await current.Close();

                Listener.Stop();
            });
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