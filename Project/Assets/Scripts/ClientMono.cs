using UnityEngine;
using NetSockets.Client;
using System.Text;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public ClientSocket Socket;

    void Start()
    {
        Socket = new ClientSocket(ip, port, bufferSize);

        Socket.DataReceived += socket_DataReceived;
    }

    private void socket_DataReceived(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

        Debug.Log($"Client received message: {data}");
    }

    public void SendTest(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);

        Socket.Send(data);
    }

    public void Connect()
    {
        Socket.Open();
    }

    public void Disconnect()
    {
        Socket.Close();
    }

    private void OnDestroy()
    {
        if (Socket != null)
        {
            Socket.Close();
            Socket.Dispose();
        }
    }
}
