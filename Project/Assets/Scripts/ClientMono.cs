using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetSockets.Client;
using System.Text;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public static ClientSocket Socket;

    void Start()
    {
        Socket = new ClientSocket(ip, port, bufferSize);

        Socket.DataReceived += socket_DataReceived;

        Socket.Open();
    }

    private void socket_DataReceived(object sender, DataReceivedArgs e)
    {
        var data = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

        Debug.Log($"Client received message: {data}");
    }

    public void SendTest(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);

            //Socket.Send(data);
    }

    private void OnDestroy()
    {
        if (Socket != null)
        {
            Socket.Close();
        }
    }
}
