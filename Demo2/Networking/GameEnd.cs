using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Threading;

namespace Demo2.Networking
{
    public class GameEnd : Processor<EmptyPrimitive>
    {
        public override void Process(IThreadContext context, GenericNetworkClient client, EmptyPrimitive data)
        {
            Program.Game.EndGame();
        }
    }
}
