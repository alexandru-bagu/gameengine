using System;
using System.Net;
using System.Net.Sockets;
using GameEngine.Threading;

namespace GameEngine.Networking.TCP
{
    public class NetworkServer<T> : Context<GenericNetworkClient>
		where T : NetworkClient, new()
	{
		private Socket _listener;
		private bool _closed;

		public NetworkServer(IPEndPoint bindAddress, int backLog)
		{
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_listener.Bind(bindAddress);
			_listener.Listen(backLog);

			beginListening();
		}

		public NetworkServer(int port)
			: this(new IPEndPoint(IPAddress.Any, port), 4) { }

		public NetworkServer(string ip, int port)
			: this(new IPEndPoint(IPAddress.Parse(ip), port), 4) { }

		public NetworkServer(int port, int backLog)
				: this(new IPEndPoint(IPAddress.Any, port), backLog) { }

		public NetworkServer(string ip, int port, int backLog)
				: this(new IPEndPoint(IPAddress.Parse(ip), port), backLog) { }

		private void beginListening()
		{
			_listener.BeginAccept(beginAcceptCallback, null);
		}

		private void beginAcceptCallback(IAsyncResult result)
		{
			try
			{
				var socket = _listener.EndAccept(result);
                var client = new T();
                client.Init(socket);
				Register(client);
				client.BeginReceive();
				beginListening();
			}
			catch { return; }
		}

		public override void Tick(IThreadContext context)
		{
			base.Tick(context);

			foreach (var client in Tickables)
				if (!client.Online)
					Unregister(client);
		}

		public void Close()
		{
			if (!_closed)
			{
				_closed = true;
				_listener.Close();
			}
		}
	}
}
