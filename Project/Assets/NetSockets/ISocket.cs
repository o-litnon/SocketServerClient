
using System.Threading.Tasks;

namespace NetSockets
{
    public interface ISocket
    {
        bool Running { get; }
        Task Open();
        Task Close();
    }
}
