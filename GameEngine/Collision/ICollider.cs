using OpenTK;

namespace GameEngine.Collision
{
    public interface ICollider
    {
        Vector2 Position { get; set; }
        Vector2 Size { get; set; }
    }
}
