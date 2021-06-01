using NetSockets.Client;
using NetSockets;
using System.Threading.Tasks;
using System.Net;

public class JustusClient : ClientSocket
{
    public int? Id;

    public JustusClient(string ip, int port, int bufferSize) : base(string.IsNullOrEmpty(ip) ? IPAddress.Loopback: IPAddress.Parse(ip), port, bufferSize)
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
        else
        {
            await Send(new byte[0], ConnectionType.UDP);
        }
    }

    public override Task Close()
    {
        if (Id.HasValue)
            Debugging.Log($"Client {Id}: Disconnected");

        Id = null;

        return base.Close();
    }
}
