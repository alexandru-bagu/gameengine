using OpenTK;
using System.Drawing;

namespace GameEngine.Text
{
    public class GlyphLocation
    {
        public Glyph Glyph { get; set; }
		public Vector2 AbsolutePosition { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 CenterPosition { get; set; }
        public int ArrayIndex { get; private set; }
        public int LineIndex { get; private set; }
        public int ColumnIndex { get; private set; }
        public float Width => Glyph.Width;
        public float Height => Glyph.Height;

        private RectangleF _clientArea;

        public GlyphLocation(Glyph glyph, Vector2 position, int arrayIndex, int lineIndex, int columnIndex)
        {
            Glyph = glyph;
            Position = position;
            ArrayIndex = arrayIndex;
            LineIndex = lineIndex;
            ColumnIndex = columnIndex;
			Offset(Vector2.Zero);
        }

		public void Offset(Vector2 position)
		{
			AbsolutePosition = Position + position;
			_clientArea = new RectangleF(AbsolutePosition.X, AbsolutePosition.Y, Glyph.Width, Glyph.Height);
			CenterPosition = AbsolutePosition + new Vector2(Width / 2, Glyph.Height / 2);
		}

        public bool Pick(Vector2 position)
        {
            return _clientArea.Contains(position.X, position.Y);
        }

        public float Distance(Vector2 position)
        {
            return CenterPosition.Distance(position);
        }
    }
}
