using NetSockets;
using NetSockets.Server;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public JustusServer Server;

    void Start()
    {
        if (Server != null)
            return;

        Server = new JustusServer(ip, port, bufferSize);
        Server.Open();

        Debug.Log("Server started...");
    }

    private void OnDestroy()
    {
        if (Server != null)
        {
            Server.Close();
        }
    }
}

public class JustusServer : ServerSocket
{
    private Dictionary<string, int> idMap = new Dictionary<string, int>();
    public JustusServer(string ip, int port, int bufferSize) : base(ip, port, bufferSize)
    {
        DataReceived += server_OnDataIn;
        ClientConnected += server_OnClientIn;
        ClientDisconnected += server_OnClientOut;
    }

    public void SendAll(Packet packet, ConnectionType type)
    {
        var bytes = packet.ToArray();

        foreach (var item in ConnectedChannels.OpenChannels)
            item.Value.Send(bytes, type);
    }
    public void SendTo(Packet data, ConnectionType type, string id)
    {
        if (ConnectedChannels.OpenChannels.TryGetValue(id, out Channel channel))
            channel.Send(data.ToArray(), type);
    }
    public void SendAllExcept(Packet packet, ConnectionType type, string id)
    {
        var bytes = packet.ToArray();

        foreach (var item in ConnectedChannels.OpenChannels.Where(d => !d.Key.Equals(id)))
            item.Value.Send(bytes, type);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        idMap[e.Id] = NewId();

        Debug.Log($"Client connected with Id: {idMap[e.Id]}");

        using (var packet = new Packet())
        {
            packet.Write(idMap[e.Id]);
            packet.Write("Welcome to the server");

            e.Channel.Send(packet.ToArray());
        }
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client disconnected with Id: {idMap[e.Id]}");

        idMap.Remove(e.Id);
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        using (var packet = new Packet(e.Message))
        {
            var data = packet.ReadString();

            Debug.Log($"Server received message from {idMap[e.Id]}: {data}");
        }
    }

    private int NewId()
    {
        lock (idMap)
            for (int i = 0; i < int.MaxValue; i++)
                if (!idMap.ContainsValue(i))
                    return i;

        throw new System.Exception("To many players are being hosted");
    }
}