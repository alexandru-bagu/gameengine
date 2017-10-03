using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GameEngine.Threading;

namespace GameEngine.Networking.TCP
{
	public abstract class NetworkClient : GenericNetworkClient, ITCPNetworkClient
	{
		private MemoryStream _bufferStream;
		private byte[] _buffer;
		private Queue<byte[]> _queue;
		private object _syncRoot;
		private int _lengthSize, _receivedSize;

        protected void Init()
        {
            _bufferStream = new MemoryStream();
            _buffer = new byte[2048];
            _queue = new Queue<byte[]>();
            _syncRoot = new object();
        }

        public virtual void Init(Socket socket)
        {
            Init();
            SetSocket(socket);
        }

        public virtual void Init(string ip, int port)
        {
            Init(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public virtual void Init(IPAddress ip, int port)
        {
            Init(new IPEndPoint(ip, port));
        }

        public virtual void Init(IPEndPoint iep)
        {
            Init();
            SetSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

            Connection.Connect(iep);
            BeginReceive();
        }
        
		internal override void InternalSend(byte[] buffer)
		{
			if (!_online) return;
			try { Connection.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, sendCallback, this); }
			catch (ObjectDisposedException) { }
		}

		private static void sendCallback(IAsyncResult res)
		{
			var client = (NetworkClient)res.AsyncState;

			try
			{
				if (client.Connection.EndSend(res) == 0)
					client.Disconnect();
			}
			catch
			{
				client.Disconnect();
			}
		}

		internal override void InternalSerialize(Type type, ISerializable data, bool validateTypes)
		{
			var buffer = Factory.Serialize(type, data, validateTypes);
			using (var serializer = new Serializer(buffer.Length + 4))
			{
				serializer.Write(buffer.Length);
				serializer.Write(buffer);
				InternalSend(serializer.ToArray());
			}
		}

		internal override sealed void BeginReceive()
		{
			if (!_online) return;
			_lengthSize = 4;
			_receivedSize = 0;

			try { Connection.BeginReceive(_buffer, _receivedSize, _lengthSize - _receivedSize, SocketFlags.None, headerCallback, this); }
			catch(ObjectDisposedException) { }
		}

		private static void headerCallback(IAsyncResult res)
		{
			var client = (NetworkClient)res.AsyncState;

			try
			{
				var size = client.Connection.EndReceive(res);
				if (size == 0)
				{
					client.Disconnect();
					return;
				}
				client._receivedSize += size;
				if (client._lengthSize != client._receivedSize)
				{
					client.Connection.BeginReceive(client._buffer, client._receivedSize, client._lengthSize - client._receivedSize, SocketFlags.None, headerCallback, client);
				}
				else
				{
					client._lengthSize = BitConverter.ToInt32(client._buffer, 0);
					client._receivedSize = 0;
					client.Connection.BeginReceive(
						client._buffer, 0,
						Math.Min(client._lengthSize - client._receivedSize, client._buffer.Length),
						SocketFlags.None, bodyCallback, client);
				}
			}
			catch
			{
				if (client._online)
					client.Disconnect();
			}
		}

		private static void bodyCallback(IAsyncResult res)
		{
			var client = (NetworkClient)res.AsyncState;

			try
			{
				var size = client.Connection.EndReceive(res);
				if (size == 0)
				{
					client.Disconnect();
					return;
				}
				client._receivedSize += size;
				client._bufferStream.Write(client._buffer, 0, size);
				if (client._lengthSize != client._receivedSize)
				{
					client.Connection.BeginReceive(
						client._buffer, 0,
						Math.Min(client._lengthSize - client._receivedSize, client._buffer.Length),
						SocketFlags.None, bodyCallback, client);
				}
				else
				{
					var buffer = client._bufferStream.ToArray();
					client._bufferStream.Position = 0;
					client._bufferStream.SetLength(0);
					lock (client._syncRoot)
						client._queue.Enqueue(buffer);

					client.BeginReceive();
				}
			}
			catch
			{
				if (client._online)
					client.Disconnect();
			}
		}

		public override void Tick(IThreadContext context)
		{
			if (!_online) return;
			if (_queue.Count > 0)
			{
				lock (_syncRoot)
				{
					while (_queue.Count > 0)
					{
						byte[] data = _queue.Dequeue();
						Factory.Process(context, this, data);
					}
				}
			}
		}

		public override void Disconnect()
		{
			if (_online)
			{
				_online = false;
				Connection.Close();
			}
		}
    }
}
