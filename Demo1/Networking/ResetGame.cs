using System;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo1.Networking
{
	public class ResetGame : Processor<EmptyPrimitive>
	{
		public override void Process(IThreadContext context, GenericNetworkClient client, EmptyPrimitive data)
		{
			MultiplayerInstance.Instance.Client.Send<ResetGame>(new EmptyPrimitive());

			var gameScene = Program.GameScene as GameScene;

			Program.MainScene.SetView(gameScene.Manager);
			Program.MainScene.MoveCamera(gameScene.Manager);
			gameScene.Reset();

			MultiplayerInstance.Instance.CleanUp(gameScene.Manager);
		}
	}
}
