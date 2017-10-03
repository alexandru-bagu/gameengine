using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GameEngine.Threading;

namespace GameEngine.Networking.UDP
{
	public abstract class NetworkClient : GenericNetworkClient, IUDPNetworkClient
	{
		private bool _listenForMessages, _ownsSocket;
		private byte[] _remoteBuffer;
		private object _syncRoot;
		private Queue<byte[]> _queue;
		private EndPoint _remoteIEP;

		public IPEndPoint RemoteEndPoint { get; protected set; }

        public void Init(Socket socket, IPEndPoint endPoint)
        {
            SetSocket(socket);

            _listenForMessages = false;
            _ownsSocket = false;
            RemoteEndPoint = endPoint;
        }

        public void Init(string ip, int port)
        {
            Init(IPAddress.Parse(ip), port);
        }

        public void Init(IPAddress ip, int port)
        {
            Init(new IPEndPoint(ip, port));
        }

        public void Init(IPEndPoint endPoint)
        {
            Init(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), endPoint);

            _listenForMessages = true;
            _ownsSocket = true;
        }

		internal override void InternalSerialize(Type type, ISerializable data, bool validateTypes)
		{
			InternalSend(Factory.Serialize(type, data, validateTypes));
		}

		internal override void InternalSend(byte[] buffer)
		{
			if (!_online) return;
			if (buffer.Length > 576) throw new Exception("Buffer too big. Maximum allowed size is 576 for maximum deliverability.");
			try { Connection.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, RemoteEndPoint, sendToCallback, this); }
			catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (SocketException) { }
        }

		private static void sendToCallback(IAsyncResult res)
		{
			var client = res.AsyncState as NetworkClient;
			try
			{
				var sent = client.Connection.EndSendTo(res);
				if (sent == 0)
				{
					client.Disconnect();
				}
				else
				{
					if (client._listenForMessages)
					{
						client.BeginReceive();
					}
				}
			}
			catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch { client.Disconnect(); }
		}

		internal override sealed void BeginReceive()
		{
			_listenForMessages = false;
			_remoteBuffer = new byte[576];
			_syncRoot = new object();
			_queue = new Queue<byte[]>();
			_remoteIEP = new IPEndPoint(IPAddress.Any, 0);
			beginListening();
		}

		private void beginListening()
		{
			if (!_online) return;
			try { Connection.BeginReceiveFrom(_remoteBuffer, 0, _remoteBuffer.Length, SocketFlags.None, ref _remoteIEP, receiveFromCallback, null); }
			catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (SocketException) { }
        }

		private void receiveFromCallback(IAsyncResult res)
		{
			try
			{
				int size = Connection.EndReceiveFrom(res, ref _remoteIEP);
				byte[] buffer = new byte[size];
				Buffer.BlockCopy(_remoteBuffer, 0, buffer, 0, size);

				lock (_syncRoot)
					_queue.Enqueue(buffer);

				beginListening();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (SocketException) { }
        }

		public override void Tick(IThreadContext context)
		{
			if (!_online) return;
			if (_remoteIEP == null) return;
			if (_queue.Count > 0)
			{
				lock (_syncRoot)
				{
					while (_queue.Count > 0)
					{
						byte[] data = _queue.Dequeue();
						if (!Factory.Process(context, this, data))
							InternalSend(data);
					}
				}
			}
		}

		public override void Disconnect()
		{
			if (_ownsSocket && _online)
				Connection.Close(10);
			_online = false;
		}
    }
}
