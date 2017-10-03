using GameEngine.Threading;

namespace GameEngine.Networking
{
	public abstract class Processor<T> : IProcessor
		where T : ISerializable, new()
	{
		public abstract void Process(IThreadContext context, GenericNetworkClient client, T data);
		void IProcessor.Process(IThreadContext context, GenericNetworkClient client, ISerializable data) { Process(context, client, (T)data); }
	}
}
