using System;
using GameEngine.Threading;
using System.Net.Sockets;

namespace GameEngine.Networking
{
	public abstract class GenericNetworkClient : ITickable
	{
		protected bool _online;
		public Socket Connection { get; protected set; }
		public bool Online => _online;

        protected GenericNetworkClient() { }

        protected void SetSocket(Socket socket)
        {
            Connection = socket;
            _online = true;
        }

		internal abstract void InternalSerialize(Type type, ISerializable data, bool validateTypes);
		internal abstract void InternalSend(byte[] buffer);
		internal abstract void BeginReceive();

		public abstract void Tick(IThreadContext context);
		public abstract void Disconnect();

		public virtual void Send<TProcessor, TData>(TData data)
			where TProcessor : Processor<TData>
			where TData : ISerializable, new()
		{
			var type = typeof(TProcessor);
			Send(type, data);
		}

		public virtual void Send<TProcessor>(ISerializable data)
		{
			var type = typeof(TProcessor);
			Send(type, data);
		}

		public virtual void Send(Type type, ISerializable data)
		{
			InternalSerialize(type, data, true);
		}
	}
}
