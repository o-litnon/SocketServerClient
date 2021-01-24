using UnityEngine;
using NetSockets.Client;
using NetSockets;
using System.Threading.Tasks;

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

        var connecting = base.Open();

        return connecting.ContinueWith(d => {
            if (!Running)
                Debug.LogWarning("Unable to connect to the server.");
        });
    }

    public override Task Close()
    {
        Id = null;

        return base.Close();
    }
}
