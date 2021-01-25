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
        using (var packet = new Packet(e.Data))
        {
            if (!Id.HasValue)
            {
                Debugging.Log("Client: Receiving Id");
                Id = packet.ReadInt();
            }

            var data = packet.ReadString();

            Debugging.Log($"Client {Id}: {data}");
        }
    }

    public override async Task Open()
    {
        if (Running)
            return;

        Id = null;

        await base.Open();

        if (!Running)
            Debugging.LogWarning("Client: Unable to connect to the server.");
    }

    public override Task Close()//without 'async', you must return a Task
    {
        if (Id.HasValue)
            Debugging.Log($"Client {Id}: Disconnected");

        Id = null;

        return base.Close();
    }
}
