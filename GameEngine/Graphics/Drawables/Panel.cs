using OpenTK;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
	public class Panel : Container
	{
		public float BorderWidth { get; set; } = 0;
		public Color BackColor { get; set; }
		public Color BorderColor { get; set; }

		public Panel(float x1, float y1, float x2, float y2)
		{
			Position = new Vector2(x1, y1);
			Size = new Vector2(x2, y2) - Position;
		}

		public Panel(Vector2 position, float width, float height)
		{
			Position = position;
			Size = new Vector2(width, height);
		}

		public Panel(Vector2 position, Vector2 size)
		{
			Position = position;
			Size = size;
		}

		public Panel()
		{

		}

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRectangle(AbsolutePosition, Size, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);
			base.Draw();
			if (BorderWidth > 0) Painter.Rectangle(AbsolutePosition, Size, BorderColor, BorderWidth);
		}
	}
}
