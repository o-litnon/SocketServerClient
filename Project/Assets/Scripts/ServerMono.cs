using NetSockets;
using NetSockets.Server;
using System.Linq;
using System.Text;
using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public static JustusServer Server;

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
    public JustusServer(string ip, int port, int bufferSize) : base(ip, port, bufferSize)
    {
        DataReceived += server_OnDataIn;
        ClientConnected += server_OnClientIn;
        ClientDisconnected += server_OnClientOut;
    }

    public void SendAllString(string packet, ConnectionType type)
    {
        foreach (var item in ConnectedChannels.OpenChannels)
            item.Value.Send(Encoding.UTF8.GetBytes(packet), type);
    }
    public void SendAll(Packet packet, ConnectionType type)
    {
        foreach (var item in ConnectedChannels.OpenChannels)
            item.Value.Send(packet.ToArray(), type);
    }
    public void SendTo(Packet data, ConnectionType type, string id)
    {
        if (ConnectedChannels.OpenChannels.TryGetValue(id.ToString(), out Channel channel))
            channel.Send(data.ToArray(), type);
    }
    public void SendAllExcept(Packet packet, ConnectionType type, string id)
    {
        foreach (var item in ConnectedChannels.OpenChannels.Where(d => !d.Key.Equals(id)))
            item.Value.Send(packet.ToArray(), type);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client connected with Id: {e.Id}");

        e.Channel.Send(Encoding.UTF8.GetBytes($"ID:{e.Id}"));
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client disconnected with Id: {e.Id}");
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

        Debug.Log($"Server received message from {e.Id}: {data}");
    }
}