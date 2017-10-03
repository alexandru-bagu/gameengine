using OpenTK;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
    public class RoundedPanel : Panel
    {
        public float Radius { get; set; } = 15;

        public RoundedPanel() : base()
        {
            Padding = new Vector4(10, 10, 10, 10);
        }

        public RoundedPanel(Vector2 position, float width, float height) : base(position, width, height)
		{
			Padding = new Vector4(10, 10, 10, 10);
        }

        public RoundedPanel(Vector2 position, Vector2 size) : base(position, size)
		{
			Padding = new Vector4(10, 10, 10, 10);
        }

        public RoundedPanel(float x1, float y1, float x2, float y2) : base(x1, y1, x2, y2)
		{
			Padding = new Vector4(10, 10, 10, 10);
        }

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRoundedRectangle(AbsolutePosition.X, AbsolutePosition.Y, Width, Height, Radius, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);

			base.DrawObjects();

            Painter.RoundedRectangle(AbsolutePosition.X, AbsolutePosition.Y, Width, Height, BorderWidth, Radius, BorderColor);
		}
    }
}
