using System;
using Demo1.Networking.Primitives;
using GameEngine.Networking.UDP;
using GameEngine.Threading;

namespace Demo1
{
	public class MultiplayerInstance
	{
		private static MultiplayerInstance _instance;
		public static MultiplayerInstance Instance
		{
			get
			{
				if (_instance == null)
					_instance = new MultiplayerInstance();
				return _instance;
			}
		}

		public const int Port = 1234;

		public NetworkServer<UDPClient> Server { get; private set; }
		public UDPClient Client { get; set; }
		public string Identifier { get; set; }
		public bool GameInProgress { get; set; }
		public bool IsClient { get; set; }

		public void StartServer(IContext<ITickable> context)
		{
			try
			{
				Server = new NetworkServer<UDPClient>(Port);
				context.Register(Server);
			}
			catch { }
		}

		public void StopServer(IContext<ITickable> context)
		{
			try
			{
				Server.Close();
				context.Unregister(Server);
			}
			catch { }
			Server = null;
		}

		public void BeginGame(string opponentName)
		{
			((GameScene)Program.GameScene).SetPlayerOpponent(opponentName);
			Program.GameScene.SetView(Program.Manager);
			Program.GameScene.MoveCamera(Program.Manager);
		}

		public void UpdateState(StatePrimitive data)
		{
			((GameScene)Program.GameScene).UpdateState(data);
		}

		public void UpdateScore(ScorePrimitive data)
		{
			((GameScene)Program.GameScene).UpdateScore(data);
		}

		public void CleanUp(IContext<ITickable> context)
		{
			if (Server != null)
				StopServer(context);
			if (Client != null)
			{
				Client.Disconnect();
				context.Unregister(Client);
				Client = null;
			}
		}
	}
}
