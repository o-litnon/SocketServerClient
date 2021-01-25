using UnityEngine;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public bool autoConnect = false;
    public JustusClient Socket;

    void Awake()
    {
        Socket = new JustusClient(ip, port, bufferSize);
    }

    async void Update()
    {
        if (autoConnect)
        {
            autoConnect = false;
            await Socket.Open();
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