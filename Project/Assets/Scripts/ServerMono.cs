using NetSockets.Server;
using System.Text;
using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public static ServerSocket Server;
    // Start is called before the first frame update
    void Start()
    {
        Server = new ServerSocket(ip, port, bufferSize);

        Server.DataReceived += server_OnDataIn;
        Server.ClientReceived += server_OnClientIn;

        Server.Start();
        Debug.Log("Server has started...");
    }

    private void OnDestroy()
    {
        if (Server != null)
        {
            Server.Stop();
        }
    }

    public void SendTest(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);

        Debug.Log($"Sending to {Server.ConnectedChannels.OpenChannels.Count } clients");

        foreach (var item in Server.ConnectedChannels.OpenChannels)
            item.Value.Send(data);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client connected with Id: {e.ConnectionId}, from {Server.ConnectedChannels.OpenChannels.Count } clients");

        e.ThisChannel.Send(Encoding.UTF8.GetBytes("CONNECTION SUCCESS"));
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

        Debug.Log($"Server received message: {data}");

        e.ThisChannel.Send(Encoding.UTF8.GetBytes("MESSAGE RECEIVED"));

        if (data == "CLOSE")
        {
            e.ThisChannel.Close();
        }
    }
}
