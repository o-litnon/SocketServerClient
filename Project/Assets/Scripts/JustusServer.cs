using NetSockets;
using NetSockets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JustusServer : ServerSocket
{
    private Dictionary<string, int> IdMap = new Dictionary<string, int>();
    public JustusServer(string ip, int port, int bufferSize, int maxPlayers) : base(ip, port, bufferSize, maxPlayers)
    {
        DataReceived += server_OnDataIn;
        ClientConnected += server_OnClientIn;
        ClientActivated += server_OnClientActivated;
        ClientDisconnected += server_OnClientOut;
    }

    public Task SendAll(Packet packet, ConnectionType type)
    {
        var bytes = packet.ToArray();
        var jobs = new List<Task>();

        foreach (var item in ConnectedChannels.ActiveChannels)
            jobs.Add(item.Value.Send(bytes, type));

        return Task.WhenAll(jobs);
    }
    public async Task SendTo(Packet data, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        if (ConnectedChannels.ActiveChannels.TryGetValue(guid, out Channel channel))
            await channel.Send(data.ToArray(), type);
    }
    public Task SendAllExcept(Packet packet, ConnectionType type, int id)
    {
        var bytes = packet.ToArray();
        var guid = ChannelId(id);
        var jobs = new List<Task>();

        foreach (var item in ConnectedChannels.ActiveChannels.Where(d => !d.Key.Equals(guid)))
            jobs.Add(item.Value.Send(bytes, type));

        return Task.WhenAll(jobs);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        int id = NewId(e.Id);

        Debugging.Log($"Server: {IdMap[e.Id]} connected");

        using (var packet = new Packet(IdMap[e.Id]))
        {
            packet.Write("Welcome to the server");

            _ = e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    private void server_OnClientActivated(object sender, ClientDataArgs e)
    {
        Debugging.Log($"Server: {IdMap[e.Id]} entered the game");

        using (var packet = new Packet())
        {
            packet.Write("You are now in-game");

            _ = e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debugging.Log($"Server: {IdMap[e.Id]} disconnected");

        IdMap.Remove(e.Id);
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        if (e.Data.Length == 0)
            return;

        using (var packet = new Packet(e.Data))
        {
            var data = packet.ReadString();

            Debugging.Log($"Server: From client {IdMap[e.Id]}: {data}");
        }
    }

    private string ChannelId(int id)
    {
        return IdMap.First(d => d.Value.Equals(id)).Key;
    }

    private int NewId(string input)
    {
        for (int i = 0; i < int.MaxValue; i++)
            if (!IdMap.ContainsValue(i))
            {
                IdMap[input] = i;
                return i;
            }

        throw new Exception("To many players are being hosted");
    }
}