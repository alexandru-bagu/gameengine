using System.Net.Sockets;

namespace GameEngine.Networking.TCP
{
    public interface ITCPNetworkClient : INetworkClient
    {
        void Init(Socket socket);
    }
}
