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
        private bool isOpen;
        private bool disposed;

        public ClientSocket(string ip, int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpClient = new TcpClient(endpoint);
            udpClient = new UdpClient(endpoint);
        }

        public Task Open()
        {
            isOpen = true;

            return Task.Run(() =>
            {
                while (isOpen)
                {

                }
            });
        }

        public void Close()
        {
            Dispose(false);
            isOpen = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                tcpClient.Close();
                tcpClient.Dispose();

                udpClient.Close();
                udpClient.Dispose();

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}