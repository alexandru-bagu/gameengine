using System;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class RequestGame : Processor<EmptyPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, EmptyPrimitive data)
		{
			var instance = MultiplayerInstance.Instance;
			if (instance.Server != null && !instance.GameInProgress)
			{
				MultiplayerInstance.Instance.IsClient = false;
				client.Send<AcceptGame>(data);
			}
		}
	}
}
