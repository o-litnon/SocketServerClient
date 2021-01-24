using UnityEngine;
using NetSockets.Client;
using NetSockets;
using System.Threading.Tasks;

public class ClientMono : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public int bufferSize = 4096;
    public JustusClient Socket;
    public string Id;

    void Start()
    {
        Socket = new JustusClient(ip, port, bufferSize);
    }

    private void OnDestroy()
    {
        if (Socket != null)
        {
            Socket.Dispose();
        }
    }
}

public class JustusClient : ClientSocket
{
    public string Id;

    public JustusClient(string ip, int port, int bufferSize) : base(ip, port, bufferSize)
    {
        DataReceived += socket_DataReceived;
    }

    private void socket_DataReceived(object sender, DataReceivedArgs e)
    {
        using (var packet = new Packet(e.Message))
        {
            if (string.IsNullOrEmpty(Id))
                Id = packet.ReadString();

            var data = packet.ReadString();

            Debug.Log($"Client received message: {data}");
        }
    }

    public override Task Open()
    {
        Id = string.Empty;

        return base.Open();
    }

    public override Task Close()
    {
        Id = string.Empty;

        return base.Close();
    }
}
