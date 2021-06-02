using UnityEngine;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 24;
    public bool autoConnect = false;
    public JustusClient Socket;

    void Awake()
    {
        Socket = new JustusClient(ip, port, bufferSize);
    }

    void Update()
    {
        if (autoConnect)
        {
            autoConnect = false;
            _ = Socket.Open();
        }
    }

    async void OnDisable()
    {
        if (Socket != null)
        {
            await Socket.Close();
        }
    }
}