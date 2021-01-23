using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace NetSockets.Client
{
    public class ClientSocket : IDisposable
    {
        public bool Running { get; set; }
        private TcpClient tcpClient;
        private UdpClient udpClient;
        private readonly byte[] buffer;
        private IPEndPoint endpoint;

        public event EventHandler<DataReceivedArgs> DataReceived;
        public bool isConnected => tcpClient.Client.Connected;

        public ClientSocket(string ip, int port, int bufferSize)
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpClient = new TcpClient
            {
                SendBufferSize = bufferSize,
                ReceiveBufferSize = bufferSize
            };
            udpClient = new UdpClient(endpoint);
            buffer = new byte[bufferSize];
        }

        public Task Open()
        {
            var tcpConnecting = tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
            udpClient.Connect(endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            tcpConnecting.ContinueWith((task, state) => {
                Task.Run(() => {
                    
                //setup tcp listeners

                });
            }, tcpConnecting);

            return tcpConnecting;
        }

        private void UdpReceiveCallback(IAsyncResult ar)
        {
            byte[] data = udpClient.EndReceive(ar, ref endpoint);
            udpClient.BeginReceive(UdpReceiveCallback, udpClient);

            var result = new DataReceivedArgs {
                Message = data
            };

            DataReceived?.Invoke(this, result);
        }

        public void Close()
        {
            tcpClient.Client.Disconnect(true);
        }

        public void Dispose()
        {
            tcpClient.Close();
            tcpClient.Dispose();

            udpClient.Close();
            udpClient.Dispose();
        }
    }
}