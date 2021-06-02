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
        return SendAll(packet.ToArray(), type);
    }
    public Task SendTo(Packet packet, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        return SendTo(packet.ToArray(), type, guid);
    }
    public Task SendAllExcept(Packet packet, ConnectionType type, int id)
    {
        var guid = ChannelId(id);

        return SendAllExcept(packet.ToArray(), type, guid);
    }

    public override async Task OnClientConnected(ClientDataArgs e)
    {
        await base.OnClientActivated(e);

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
        await base.OnClientActivated(e);

        Debugging.Log($"Server: {IdMap[e.Id]} entered the game");

        using (var packet = new Packet())
        {
            packet.Write("You are now in-game");

            await e.Channel.Send(packet.ToArray(), ConnectionType.TCP);
        }
    }

    public override async Task OnClientDisconnected(ClientDataArgs e)
    {
        await base.OnClientDisconnected(e);

        Debugging.Log($"Server: {IdMap[e.Id]} disconnected");

        IdMap.Remove(e.Id);
    }

    public override async Task OnDataIn(DataReceivedArgs e)
    {
        await base.OnDataIn(e);

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