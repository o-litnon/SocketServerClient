using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class DataReceivedArgs : EventArgs, IDisposable
    {
        public string ConnectionId { get; set; }
        public string Message { get; set; }
        public Channel ThisChannel { get; set; }

        public void Dispose()
        {
            ((IDisposable)ThisChannel).Dispose();
        }
    }
}
