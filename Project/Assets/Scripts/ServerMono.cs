using UnityEngine;

public class ServerMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public int maxPlayers;
    public JustusServer Server;

    void OnEnable()
    {
        if (Server == null)
            Server = new JustusServer(ip, port, bufferSize, maxPlayers);

        if (!Server.Running)
        {
            var opening = Server.Open();

            opening.ContinueWith(d =>
            {
                Debug.Log("Server started...");
            });
        }
    }

    void OnDisable()
    {
        if (Server != null)
        {
            var closing = Server.Close();

            closing.ContinueWith(d =>
            {
                Debug.Log("Server stopped...");
            });
        }
    }
}