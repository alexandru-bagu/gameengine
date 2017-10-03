using System;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class InitializeGame : Processor<StringPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, StringPrimitive data)
		{
			var instance = MultiplayerInstance.Instance;
			if (!instance.GameInProgress)
			{
				instance.GameInProgress = true;
				client.Send<InitializeGame>(new StringPrimitive() { Value = instance.Identifier });

				instance.Client = (UDPClient)client;
				instance.BeginGame(data.Value);
			}
		}
	}
}
