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
    public int? Id;

    public JustusClient(string ip, int port, int bufferSize) : base(ip, port, bufferSize)
    {
        DataReceived += socket_DataReceived;
    }

    private void socket_DataReceived(object sender, DataReceivedArgs e)
    {
        using (var packet = new Packet(e.Message))
        {
            if (!Id.HasValue)
                Id = packet.ReadInt();

            var data = packet.ReadString();

            Debug.Log($"Client received message: {data}");
        }
    }

    public override Task Open()
    {
        if (Running)
            return Task.CompletedTask;

        Id = null;

        return base.Open();
    }

    public override Task Close()
    {
        Id = null;

        return base.Close();
    }
}
