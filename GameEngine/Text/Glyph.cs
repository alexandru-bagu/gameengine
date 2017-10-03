using OpenTK;
using System.Drawing;

namespace GameEngine.Text
{
    public class Glyph
    {
        public char Char { get; private set; }
        public RectangleF Source { get; set; }
        public float Width => Source.Width;
        public float Height => Source.Height;

        public Glyph(char glyph, float x, float y, float width, float height)
        {
            Char = glyph;
            Source = new RectangleF(x, y, width, height);
        }
    }
}
