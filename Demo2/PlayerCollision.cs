using GameEngine.Collision;
using OpenTK;

namespace Demo2
{
    public class PlayerCollision : RectangleShape
    {
        public PlayerCollision(Player player) : base(player, player.Position, player.Size)
        {
            MovementSamples = 16;
            Reflect = false;
        }

        public override bool ClipThrough(Polygon polygon)
        {
            if (polygon.Parent is Bullet)
            {
                var bullet = (Bullet)polygon.Parent;
                return !bullet.Enemy;
            }
            if (polygon is PlayerCollision) return true;
            return base.ClipThrough(polygon);
        }
    }
}
