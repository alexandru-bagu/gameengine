using Demo2.Networking;
using GameEngine;
using GameEngine.Networking.Primitives;
using GameEngine.Networking.UDP;
using System.Net;
using System;

namespace Demo2
{
    public class Multiplayer
    {
        public const int Port = 1234;
        public static NetworkServer<UDPClient> Server;
        public static UDPClient Client;
        public static bool InGame = false, IsServer = false;
		private static bool IsFake = false;

        public static void Init(GameManager manager, bool throwIfUsed = true)
        {
            Action action = () =>
            {
                try
                {
                    Server = new NetworkServer<UDPClient>(Port);
                    manager.Register(Server);
                }
                catch
                {
                    if (throwIfUsed) throw;
                }

                discoveryFinish(null);
            };
            action.BeginInvoke(action.EndInvoke, null);
        }

        public static void Preinit(GameManager manager)
        {
			IsFake = true;
            InGame = true;
            Init(manager, false); 
            Dispose();
            InGame = false;
			IsFake = false;
        }

        private static void discoveryFinish(IAsyncResult obj)
        {
            if (InGame) return;
            Discovery.Search(1234, callback, discoveryFinish, null);
        }

        private static void callback(IPAddress obj)
        {
            if (InGame) return;
			if (IsFake) return;
            Client = new UDPClient();
            Client.Init(obj, Port);
            Program.Manager.Register(Client);

            Client.Send<RequestGame>(new StringPrimitive() { Value = Program.PlayerName });
        }

        public static void Dispose()
        {
            InGame = false;
            Program.Manager.Unregister(Server);
            if (Server != null) Server.Close();
        }
    }
}
