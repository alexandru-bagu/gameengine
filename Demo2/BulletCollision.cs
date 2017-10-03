using GameEngine.Collision;
using OpenTK;

namespace Demo2
{
    public class BulletCollision : RectangleShape
    {
        public BulletCollision(ICollider parent, Vector2 position, Vector2 size) : base(parent, position, size)
        {
        }

        public override bool ClipThrough(Polygon polygon)
        {
            if (polygon is BulletCollision) return true;
            return base.ClipThrough(polygon);
        }
    }
}
