using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GameEngine.Threading;
using GameEngine.Reflection;

#pragma warning disable CS0618
#pragma warning disable CS0675

namespace GameEngine.Networking.UDP
{
	public class NetworkServer<T> : ITickable
		where T : NetworkClient, new()
    {
		private Socket _listener;
		private EndPoint _remoteIEP;
		private Dictionary<long, NetworkClient> _clients;
		private byte[] _remoteBuffer;
		private Object _syncRoot;
		private Queue<Tuple<NetworkClient, byte[]>> _queue;
		private bool _closed;

		public NetworkServer(IPEndPoint bindAddress)
		{
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_listener.Bind(bindAddress);
			_remoteIEP = new IPEndPoint(IPAddress.Any, 0);
			_clients = new Dictionary<long, NetworkClient>();
			_remoteBuffer = new byte[576];//guarantee of deliverability
			_syncRoot = new object();
			_queue = new Queue<Tuple<NetworkClient, byte[]>>();

			beginListening();
		}

		public NetworkServer(int port) 
			: this(new IPEndPoint(IPAddress.Any, port)) { }
		public NetworkServer(string ip, int port) 
			: this(new IPEndPoint(IPAddress.Parse(ip), port)) { }

		private void beginListening()
		{
			_listener.BeginReceiveFrom(_remoteBuffer, 0, _remoteBuffer.Length, SocketFlags.None, ref _remoteIEP, receiveFromCallback, null);
		}

		private long fromEndPoint(EndPoint ep)
		{
			var iep = (IPEndPoint)ep;
			if (iep != null)
				return (iep.Address.Address << 32) | ((long)iep.Port);
			return 0;
		}

		private IPEndPoint clone(EndPoint ep)
		{
			var iep = (IPEndPoint)ep;
			if (iep != null)
				return new IPEndPoint(iep.Address, iep.Port);
			return null;
		}

		private void receiveFromCallback(IAsyncResult res)
		{
			try
			{
				int size = _listener.EndReceiveFrom(res, ref _remoteIEP);
				byte[] buffer = new byte[size];
				Buffer.BlockCopy(_remoteBuffer, 0, buffer, 0, size);
				var liep = fromEndPoint(_remoteIEP);

				NetworkClient client;
				lock (_syncRoot)
				{
                    if (!_clients.TryGetValue(liep, out client))
                    {
                        client = new T();
                        client.Init(clone(_remoteIEP));
                        _clients.Add(liep, client);
                    }
					_queue.Enqueue(Tuple.Create(client, buffer));
				}

				beginListening();
			}
			catch { return; }
		}

		public void Tick(IThreadContext context)
		{
			if (_queue.Count > 0)
			{
				lock (_syncRoot)
				{
					while (_queue.Count > 0)
					{
						Tuple<NetworkClient, byte[]> tuple = _queue.Dequeue();
						var client = tuple.Item1;
						var buffer = tuple.Item2;

						if (client.Online && !Factory.Process(context, client, buffer))
						{
							client.InternalSend(buffer);//for discovery
							client.Disconnect();
						}

						if (!client.Online)
							_clients.Remove(fromEndPoint(client.RemoteEndPoint));
					}
				}
			}
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
