using OpenTK;
using GameEngine.Collections;

namespace GameEngine.Graphics
{
	public interface IContainer
	{
		Vector2 Position { get; }
		Vector2 Size { get; }
		Vector4 Padding { get; }
		bool Sealed { get; }
        ReadOptimizedList<Drawable> Objects { get; set; }
        void Add(Drawable drawable);
        void Remove(Drawable drawable);
        void Clear();
        void BringToFront(Drawable drawable);
        void SendToBack(Drawable drawable);
        void DrawObjects();
        bool Pick(Vector2 position);
        Drawable PickElement(Vector2 position);
        IContainer PickContainer(Vector2 position, Drawable exclude);
    }
}
