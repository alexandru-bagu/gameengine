using Demo1.Networking.Primitives;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class UpdateScore : Processor<ScorePrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, ScorePrimitive data)
		{
			MultiplayerInstance.Instance.UpdateScore(data);
		}
	}
}
