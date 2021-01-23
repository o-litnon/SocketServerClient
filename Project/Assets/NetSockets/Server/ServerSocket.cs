using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace NetSockets.Server
{
    public class ServerSocket
    {
        public bool Running { get; set; }
        public event EventHandler<DataReceivedArgs> DataReceived;
        private TcpListener Listener;
        public Channels ConnectedChannels;

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
                    await Task.Run(() =>
                    {
                        var channel = new Channel(this);

                        if (ConnectedChannels.OpenChannels.TryAdd(channel.Id, channel))
                            channel.Open(client);
                    });
                }

            }
            catch (SocketException)
            {
                throw;
            }
            catch (ChannelRegistrationException)
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