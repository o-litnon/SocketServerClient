using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSockets.Server
{
    public class Channels
    {
        public ConcurrentDictionary<string, Channel> OpenChannels { get; private set; }
        public ConcurrentDictionary<string, Channel> ActiveChannels { get; private set; }
        public ConcurrentQueue<Channel> PendingChannels { get; private set; }
        public bool IsFull => MaxPlayers >= 0 && ActiveChannels.Count >= MaxPlayers;
        private int _maxPlayers;
        public int MaxPlayers
        {
            get => _maxPlayers;
            set
            {
                bool hasChanged = value != _maxPlayers;
                _maxPlayers = value;

                if (hasChanged && !IsFull)
                    _ = ActivatePending();
            }
        }

        private readonly ServerSocket thisServer;

        public Channels(ServerSocket myServer, int maxPlayers)
        {
            this._maxPlayers = maxPlayers;
            OpenChannels = new ConcurrentDictionary<string, Channel>();
            ActiveChannels = new ConcurrentDictionary<string, Channel>();
            PendingChannels = new ConcurrentQueue<Channel>();
            thisServer = myServer;
        }

        private object ActivatingLock = new object();
        internal bool TryAdd(string id, Channel value)
        {
            lock (ActivatingLock)
            {
                var result = OpenChannels.TryAdd(id, value);

                if (result)
                    PendingChannels.Enqueue(value);

                return result;
            }
        }

        internal bool TryRemove(string id, out Channel value)
        {
            lock (ActivatingLock)
            {
                var result = OpenChannels.TryRemove(id, out value);

                if (result)
                {
                    ActiveChannels.TryRemove(id, out Channel channel);

                    if (PendingChannels.Contains(value))
                        PendingChannels = new ConcurrentQueue<Channel>(PendingChannels.Where(d => !d.Id.Equals(id)));
                }

                return result;
            }
        }

        public Task ActivatePending()
        {
            return Task.Run(() =>
            {
                lock (ActivatingLock)
                {
                    while (!IsFull && PendingChannels.Count > 0)
                        if (PendingChannels.TryDequeue(out Channel channel) && channel.Running)
                            if (ActiveChannels.TryAdd(channel.Id, channel))
                                thisServer.OnClientActivated(new ClientDataArgs
                                {
                                    Id = channel.Id,
                                    Channel = channel
                                });
                            else
                                PendingChannels.Enqueue(channel);

                }
            });
        }
    }
}
