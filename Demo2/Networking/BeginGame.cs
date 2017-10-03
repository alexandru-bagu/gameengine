using GameEngine;
using GameEngine.Graphics;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo2.Networking
{
    public class BeginGame : Processor<StringPrimitive>
    {
        public override void Process(IThreadContext context, GenericNetworkClient client, StringPrimitive data)
        {
            Multiplayer.InGame = true;
            Multiplayer.IsServer = false;

            Program.Game.NewRound(true);
            Program.Game.SetView((GameManager)context);
            Program.Game.MoveCamera((GameManager)context, View.TweenType.CubicInOut);

            Program.Game.SetClient(client);
            Program.Game.SetPlayerName(Program.PlayerName);
            Program.Game.SetEnemyName(data.Value);
        }
    }
}
