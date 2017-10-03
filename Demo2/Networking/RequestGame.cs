using GameEngine;
using GameEngine.Graphics;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo2.Networking
{
    public class RequestGame : Processor<StringPrimitive>
    {
        public override void Process(IThreadContext context, GenericNetworkClient client, StringPrimitive data)
        {
            if (Multiplayer.InGame) return;
			if (data.Value == Program.PlayerName) return;
            Multiplayer.InGame = true;
            Multiplayer.IsServer = true;
            client.Send<BeginGame>(new StringPrimitive() { Value = Program.PlayerName });

            Program.Game.NewRound(false);
            Program.Game.SetClient(client);
            Program.Game.SetView((GameManager)context);
            Program.Game.MoveCamera((GameManager)context, View.TweenType.CubicInOut);
            Program.Game.SetPlayerName(Program.PlayerName);
            Program.Game.SetEnemyName(data.Value);
        }
    }
}
