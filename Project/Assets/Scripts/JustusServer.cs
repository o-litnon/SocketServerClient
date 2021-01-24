using NetSockets;
using NetSockets.Server;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        foreach (var item in ConnectedChannels.OpenChannels)
            _ = item.Value.Send(bytes, type);
    }
    public void SendTo(Packet data, ConnectionType type, string id)
    {
        if (ConnectedChannels.OpenChannels.TryGetValue(id, out Channel channel))
            _ = channel.Send(data.ToArray(), type);
    }
    public void SendAllExcept(Packet packet, ConnectionType type, string id)
    {
        var bytes = packet.ToArray();

        foreach (var item in ConnectedChannels.OpenChannels.Where(d => !d.Key.Equals(id)))
            _ = item.Value.Send(bytes, type);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        IdMap[e.Id] = NewId();

        Debug.Log($"Client connected with Id: {IdMap[e.Id]}");

        using (var packet = new Packet())
        {
            packet.Write(IdMap[e.Id]);
            packet.Write("Welcome to the server");

            _ = e.Channel.Send(packet.ToArray());
        }
    }

    private void server_OnClientActivated(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client entered the game with Id: {IdMap[e.Id]}");
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client disconnected with Id: {IdMap[e.Id]}");

        IdMap.Remove(e.Id);
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        using (var packet = new Packet(e.Message))
        {
            var data = packet.ReadString();

            Debug.Log($"Server received message from {IdMap[e.Id]}: {data}");
        }
    }

    private int NewId()
    {
        lock (IdMap)
            for (int i = 0; i < int.MaxValue; i++)
                if (!IdMap.ContainsValue(i))
                    return i;

        throw new System.Exception("To many players are being hosted");
    }
}