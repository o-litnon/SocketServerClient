using UnityEngine;
using NetSockets.Client;
using System.Text;
using System.Net;
using NetSockets;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public ClientSocket Socket;
    public string Id;

    void Start()
    {
        Socket = new ClientSocket(ip, port, bufferSize);
        Socket.DataReceived += socket_DataReceived;
    }

    private void socket_DataReceived(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);


        if (data.Contains("ID:"))
        {
            Id = data.Replace("ID:", "");
        }

        Debug.Log($"Client received message: {data}");
    }

    public void SendTest(string message, ConnectionType type)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        Socket.Send(data, type);
    }

    public void Connect()
    {
        var connecting = Socket.Open();

        //connecting.ContinueWith(t => {
        //    Debug.Log($"{((IPEndPoint)Socket.udpClient.Client.LocalEndPoint).ToString()}");
        //});
    }

    public void Disconnect()
    {
        Socket.Close();
    }

    private void OnDestroy()
    {
        if (Socket != null)
        {
            Socket.Dispose();
        }
    }
}
