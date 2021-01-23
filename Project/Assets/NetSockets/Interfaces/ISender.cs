using System.Threading.Tasks;

namespace NetSockets
{
    public interface ISender
    {
        Task Send(byte[] data, ConnectionType type = ConnectionType.TCP);
    }
}
