using System;
using GameEngine;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class ProcessIdentifier : Processor<IdentifierPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, IdentifierPrimitive data)
		{
			if (context is GameManager)
			{
				var scene = (Program.MultiplayerScene as MultiplayerMenu);
				if(MultiplayerInstance.Instance.Identifier != data.Name)
					scene.AddIdentifier(data.Name, data.IP);

				client.Disconnect();
				(context as GameManager).Unregister(client);
			}
		}
	}
}
