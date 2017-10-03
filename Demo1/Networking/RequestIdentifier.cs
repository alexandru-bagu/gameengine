using System;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class RequestIdentifier : Processor<IdentifierPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, IdentifierPrimitive data)
		{
			data.Name = MultiplayerInstance.Instance.Identifier;
			client.Send<ProcessIdentifier>(data);
			context.InvokeLazy(() =>
			{
				client.Disconnect();
			}, 2000);
		}
	}
}
