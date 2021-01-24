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

    void OnEnable()
    {
        if (connectOnStart)
            Socket.Open();
    }

    void OnDisable()
    {
        if (Socket != null)
        {
            Socket.Close();
        }
    }
}