using NetSockets;
using NetSockets.Server;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public void SendAll(Packet packet, ConnectionType type)
    {
        var bytes = packet.ToArray();

        foreach (var item in ConnectedChannels.ActiveChannels)
            item.Value.Send(bytes, type);
    }
    public void SendTo(Packet data, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        if (ConnectedChannels.ActiveChannels.TryGetValue(guid, out Channel channel))
            channel.Send(data.ToArray(), type);
    }
    public void SendAllExcept(Packet packet, ConnectionType type, int id)
    {
        var bytes = packet.ToArray();
        var guid = ChannelId(id);

        foreach (var item in ConnectedChannels.ActiveChannels.Where(d => !d.Key.Equals(guid)))
            item.Value.Send(bytes, type);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        IdMap[e.Id] = NewId();

        Debugging.Log($"Client connected with Id: {IdMap[e.Id]}");

        using (var packet = new Packet(IdMap[e.Id]))
        {
            packet.Write("Welcome to the server");

            e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    private void server_OnClientActivated(object sender, ClientDataArgs e)
    {
        Debugging.Log($"Client entered the game with Id: {IdMap[e.Id]}");

        using (var packet = new Packet())
        {
            packet.Write("You are now in-game");

            e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debugging.Log($"Client disconnected with Id: {IdMap[e.Id]}");

        IdMap.Remove(e.Id);
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        using (var packet = new Packet(e.Data))
        {
            var data = packet.ReadString();

            Debugging.Log($"Server received message from {IdMap[e.Id]}: {data}");
        }
    }

    private string ChannelId(int id)
    {
        return IdMap.First(d => d.Value.Equals(id)).Key;
    }

    private int NewId()
    {
        lock (IdMap)
            for (int i = 0; i < int.MaxValue; i++)
                if (!IdMap.ContainsValue(i))
                    return i;

        throw new Exception("To many players are being hosted");
    }
}