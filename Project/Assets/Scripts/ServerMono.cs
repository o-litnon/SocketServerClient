using NetSockets.Server;
using System.Text;
using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public ServerSocket Server;
    // Start is called before the first frame update
    void Start()
    {
        Server = new ServerSocket(ip, port, bufferSize);

        Server.DataReceived += server_OnDataIn;
        Server.ClientConnected += server_OnClientIn;
        Server.ClientDisconnected += server_OnClientOut;

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

        foreach (var item in Server.ConnectedChannels.OpenChannels)
            item.Value.Send(data);
    }

    private void server_OnClientIn(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client connected with Id: {e.Id}");

        e.Channel.Send(Encoding.UTF8.GetBytes($"Server assigned Id is '{e.Id}'"));
    }

    private void server_OnClientOut(object sender, ClientDataArgs e)
    {
        Debug.Log($"Client disconnected with Id: {e.Id}");
    }

    private void server_OnDataIn(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

        Debug.Log($"Server received message: {data}");

        if (data == "CLOSE")
        {
            e.Channel.Close();
        }
    }
}
