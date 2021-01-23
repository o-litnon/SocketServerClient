using System;
using System.Net;
using System.Net.Sockets;

namespace NetSockets.Server
{
    public class ServerSocket
    {
        public bool Running { get; set; }
        public event EventHandler<DataReceivedArgs> DataReceived;
        private TcpListener Listener;
        public Channels ConnectedChannels { get; private set; }

        public ServerSocket(string ip, int port)
        {
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
                    var channel = new Channel(this);

                    if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                        _ = channel.Open(client);
                }

            }
            catch (SocketException)
            {
                throw;
            }
        }

        public void Stop()
        {
            Listener.Stop();
            Running = false;
        }

        public void OnDataIn(DataReceivedArgs e)
        {
            DataReceived?.Invoke(this, e);
        }
    }
}