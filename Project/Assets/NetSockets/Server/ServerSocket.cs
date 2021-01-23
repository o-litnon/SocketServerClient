using System;
using System.Net;
using System.Net.Sockets;

namespace NetSockets.Server
{
    public class ServerSocket
    {
        public bool Running { get; set; }
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

        public async void Start()
        {
            try
            {
                Listener.Start();
                Running = true;
                ConnectedChannels = new Channels(this);
                while (Running)
                {
                    var client = await Listener.AcceptTcpClientAsync();
                    var channel = new Channel(this, bufferSize);

                    if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                    {
                        _ = channel.Open(client);

                        OnClientIn(new ClientDataArgs { 
                            ConnectionId = channel.Id,
                            ThisChannel = channel
                        });
                    }
                }

            }
            catch (SocketException)
            {
                throw;
            }
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