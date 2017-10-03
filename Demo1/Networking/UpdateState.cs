using System;
using Demo1.Networking.Primitives;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class UpdateState : Processor<StatePrimitive>
	{
		public static uint LastId = 0;
		public override void Process(IThreadContext context, GenericNetworkClient client, StatePrimitive data)
		{
			if (LastId - data.Id > int.MaxValue) LastId = data.Id;
			if (data.Id < LastId) return;
			LastId = data.Id;
			MultiplayerInstance.Instance.UpdateState(data);
		}
	}
}
