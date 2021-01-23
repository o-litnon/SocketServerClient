using System.Collections.Concurrent;

namespace NetSockets.Server
{
    public class Channels
    {
        public ConcurrentDictionary<string, Channel> OpenChannels;
        private readonly ServerSocket thisServer;

        public Channels(ServerSocket myServer)
        {
            OpenChannels = new ConcurrentDictionary<string, Channel>();
            thisServer = myServer;
        }
    }
}
