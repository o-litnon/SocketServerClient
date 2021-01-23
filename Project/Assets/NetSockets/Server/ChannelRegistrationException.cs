using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class ChannelRegistrationException : Exception
    {
        public ChannelRegistrationException(string message) : base(message)
        {
        }
    }
}
