using NetSockets;
using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 24;
    public int maxPlayers;
    public JustusServer Server;

    async void OnEnable()
    {
        if (Server == null)
            Server = new JustusServer(ip, port, bufferSize, maxPlayers);

        if (!Server.Running)
        {
            await Server.Open();

            Debugging.Log("Server started...");
        }
    }

    private void Update()
    {
        if (Server.ConnectedChannels.MaxPlayers != maxPlayers)
            Server.ConnectedChannels.MaxPlayers = maxPlayers;

    }

    async void OnDisable()
    {
        if (Server != null)
        {
            await Server.Close();

            Debugging.Log("Server stopped...");
        }
    }
}