using OpenTK;
using System;

namespace GameEngine.Graphics
{
    public static class DrawableExtension
    {
        public static void Center(this Drawable parent, Drawable child)
        {
            CenterHorizontally(parent, child);
            CenterVertically(parent, child);
        }

        public static void CenterVertically(this Drawable parent, Drawable child)
        {
			child.Position = new Vector2(child.Position.X, (parent.Size.Y - child.Size.Y - parent.Padding.Y + parent.Padding.Z) / 2);
        }

        public static void CenterHorizontally(this Drawable parent, Drawable child)
        {
            child.Position = new Vector2((parent.Size.X - child.Size.X - parent.Padding.X + parent.Padding.W) / 2, child.Position.Y);
        }


		public static Vector2 Center(this Drawable parent, Vector2 child)
        {
            return CenterVertically(parent, CenterHorizontally(parent, child));
        }

        public static Vector2 CenterVertically(this Drawable parent, Vector2 child)
        {
			return new Vector2(child.X, (parent.Size.Y - child.Y - parent.Padding.Y + parent.Padding.Z) / 2);
        }

        public static Vector2 CenterHorizontally(this Drawable parent, Vector2 child)
        {
            return new Vector2((parent.Size.X - child.X - parent.Padding.X + parent.Padding.W) / 2, child.Y);
        }

		internal static Vector2 PaddingVector(this Vector4 padding)
		{
			return padding.Xy + padding.Wz;
		}
    }
}
