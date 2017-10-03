using System;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class AcceptGame : Processor<EmptyPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, EmptyPrimitive data)
		{
			var instance = MultiplayerInstance.Instance;
			if (!instance.GameInProgress)
			{
				MultiplayerInstance.Instance.IsClient = true;
				client.Send<InitializeGame>(new StringPrimitive() { Value = instance.Identifier });
			}
		}
	}
}
