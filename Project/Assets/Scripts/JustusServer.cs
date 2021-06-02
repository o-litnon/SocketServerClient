using NetSockets;
using NetSockets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JustusServer : ServerSocket
{
    private Dictionary<string, int> IdMap = new Dictionary<string, int>();
    public JustusServer(string ip, int port, int bufferSize, int maxPlayers) : base(ip, port, bufferSize, maxPlayers) { }

    public Task SendAll(Packet packet, ConnectionType type)
    {
        return Send(packet.ToArray(), type);
    }
    public Task SendTo(Packet packet, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        return Send(packet.ToArray(), type, guid);
    }
    public Task SendAllExcept(Packet packet, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        return SendExcept(packet.ToArray(), type, guid);
    }

    public override async Task OnClientConnected(ClientDataArgs e)
    {
        int id = NewId(e.Id);

        Debugging.Log($"Server: {IdMap[e.Id]} connected");

        using (var packet = new Packet(IdMap[e.Id]))
        {
            packet.Write("Welcome to the server");

            await e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    public override async Task OnClientActivated(ClientDataArgs e)
    {
        Debugging.Log($"Server: {IdMap[e.Id]} entered the game");

        using (var packet = new Packet())
        {
            packet.Write("You are now in-game");

            await e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    public override Task OnClientDisconnected(ClientDataArgs e)
    {
        Debugging.Log($"Server: {IdMap[e.Id]} disconnected");

        IdMap.Remove(e.Id);

        return Task.CompletedTask;
    }

    public override Task OnDataIn(DataReceivedArgs e)
    {
        if (e.Data.Length > 0)
            using (var packet = new Packet(e.Data))
            {
                var data = packet.ReadString();

                Debugging.Log($"Server: From client {IdMap[e.Id]}: {data}");
            }

        return Task.CompletedTask;
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