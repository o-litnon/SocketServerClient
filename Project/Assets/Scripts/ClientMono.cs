using UnityEngine;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public bool connectOnStart = false;
    public JustusClient Socket;

    void Awake()
    {
        Socket = new JustusClient(ip, port, bufferSize);
    }

    async void OnEnable()
    {
        if (connectOnStart)
            await Socket.Open();
    }

    async void OnDisable()
    {
        if (Socket != null)
        {
            await Socket.Close();
        }
    }
}