using Demo2.Networking.Primitives;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo2.Networking
{
    public class UpdatePlayer : Processor<PlayerState>
    {
        public static int LastUpdateId = 0;

        public override void Process(IThreadContext context, GenericNetworkClient client, PlayerState data)
        {
            if (LastUpdateId < data.Id || data.Id / 2 > LastUpdateId)
            {
                LastUpdateId = data.Id;
                Program.Game.UpdateEnemy(data);
            }
        }
    }
}
