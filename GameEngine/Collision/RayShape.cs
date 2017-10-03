using OpenTK;

namespace GameEngine.Collision
{
    public class RayShape : Polygon
    {
        public RayShape(ICollider parent, Vector2 point1, Vector2 point2, float width) : base(parent, 8, 1)
        {
            buildPolygon(Helper.PolygonFromLine(point1, point2, width));
        }
    }
}
