using System.Net;
using System.Net.Sockets;

namespace GameEngine.Networking.UDP
{
    public interface IUDPNetworkClient : INetworkClient
    {
        void Init(Socket socket, IPEndPoint endPoint);
    }
}
