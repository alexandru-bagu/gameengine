using System.Net;

namespace GameEngine.Networking
{
    public interface INetworkClient
    {
        void Init(string ip, int port);
        void Init(IPAddress ip, int port);
        void Init(IPEndPoint endPoint);
    }
}
