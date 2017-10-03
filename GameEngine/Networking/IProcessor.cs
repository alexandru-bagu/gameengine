using GameEngine.Threading;

namespace GameEngine.Networking
{
	public interface IProcessor
	{
		void Process(IThreadContext context, GenericNetworkClient client, ISerializable data);
	}
}
