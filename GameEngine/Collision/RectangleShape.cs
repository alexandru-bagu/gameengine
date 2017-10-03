using OpenTK;

namespace GameEngine.Collision
{
    public class RectangleShape : Polygon
    {
        public RectangleShape(ICollider parent, Vector2 position, Vector2 size) : this(parent, position.X, position.Y, size.X, size.Y) { }
        public RectangleShape(ICollider parent, float x, float y, float width, float height) : base(parent, 8, 1)
        {
            buildPolygon(new[]
            {
                new Vector2(x,y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y+ height)
            });
        }
    }
}
