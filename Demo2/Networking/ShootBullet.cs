using Demo2.Networking.Primitives;
using GameEngine.Networking;
using GameEngine.Threading;

namespace Demo2.Networking
{
    public class ShootBullet : Processor<BulletState>
    {
        public override void Process(IThreadContext context, GenericNetworkClient client, BulletState data)
        {
            Program.Game.AddBullet(data);
        }
    }
}
