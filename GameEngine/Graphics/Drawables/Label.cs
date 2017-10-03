using System.Drawing;
using GameEngine.Assets;
using GameEngine.Text;
using OpenTK;
using System;

namespace GameEngine.Graphics.Drawables
{
	public class Label : Drawable
	{
		private string _text = "";
		private StringFormatFlags _format = StringFormatFlags.DisplayFormatControl;
		private FontAsset _font;
		private GlyphLocation[] _glyphs = new GlyphLocation[0];
		private ContentAlignment _align = ContentAlignment.TopLeft;
		private Vector2 _glyphOffset;
		private bool _autoSize = true;

		public Color BackColor { get; set; } = Color.Transparent;
		public Color ForeColor { get; set; } = Color.Black;
		public Color BorderColor { get; set; } = Color.Black;
		public float BorderWidth { get; set; } = 0;

		public override Vector2 Size
		{
			get { return base.Size; }
			set
			{
				if (!base.Size.Equals(value))
				{
					base.Size = value;
					updateGlyphs(_text);
				}
			}
		}

        public bool AutoSize
        {
            get { return _autoSize; }
            set { _autoSize = value; updateGlyphs(_text); }
        }

        public FontAsset Font
        {
            get { return _font; }
            set { _font = value; updateGlyphs(_text); }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if(value == null) throw new Exception("Text cannot be null.");
                _text = value;
                updateGlyphs(value);
            }
        }

        public StringFormatFlags Format
        {
            get { return _format; }
            set { _format = value; updateGlyphs(_text); }
        }

        public ContentAlignment TextAlign
        {
            get { return _align; }
            set { _align = value; updateGlyphs(_text); }
        }

		protected override void UpdateLayout()
		{
			base.UpdateLayout();

			var offset = PaddedAbsolutePosition + _glyphOffset;
			foreach (var glyph in _glyphs)
				glyph.Offset(offset);
		}

        private void updateGlyphs(string value)
        {
            if (_font == null) return;
            if (AutoSize) _format |= StringFormatFlags.NoWrap;
            else 
				_format = _format & ~StringFormatFlags.NoWrap;

            _glyphs = _font.ProcessString(value, ClientArea, _format);

            float xMax = 0, yMax = 0;
            foreach (var g in _glyphs)
            {
                if (xMax < g.Position.X + g.Width)
                    xMax = g.Position.X + g.Width;
                if (yMax < g.Position.Y + g.Height)
                    yMax = g.Position.Y + g.Height;
            }

            if (AutoSize)
            {
                Size = new Vector2(xMax + Padding.X + 2, yMax + Padding.Y + 2);
                _glyphOffset = new Vector2(0, 0);
            }
            else
            {
                _glyphOffset = computeOffset(xMax, yMax);
            }

			var offset = PaddedAbsolutePosition + _glyphOffset;
			foreach (var glyph in _glyphs)
				glyph.Offset(offset);
        }

        private Vector2 computeOffset(float width, float height)
        {
            float offsetX = 0, offsetY = 0, paddingX = 0, paddingY = 0;
            if (_align == ContentAlignment.TopLeft || _align == ContentAlignment.MiddleLeft || _align == ContentAlignment.BottomLeft) offsetX = 0;
            if (_align == ContentAlignment.TopCenter || _align == ContentAlignment.MiddleCenter || _align == ContentAlignment.BottomCenter) { offsetX = (Size.X - width) / 2; paddingX = -Padding.X / 2; }
            if (_align == ContentAlignment.TopRight || _align == ContentAlignment.MiddleRight || _align == ContentAlignment.BottomRight) { offsetX = Size.X - width; paddingX = -Padding.X * 2; }

            if (_align == ContentAlignment.TopLeft || _align == ContentAlignment.TopCenter || _align == ContentAlignment.TopRight) offsetY = 0;
            if (_align == ContentAlignment.MiddleLeft || _align == ContentAlignment.MiddleCenter || _align == ContentAlignment.MiddleRight) { offsetY = (Size.Y - height) / 2; paddingY = -Padding.Y / 2; }
            if (_align == ContentAlignment.BottomLeft || _align == ContentAlignment.BottomCenter || _align == ContentAlignment.BottomRight) { offsetY = Size.Y - height; paddingY = -Padding.Y * 2; }

            return new Vector2(offsetX + paddingX, offsetY + paddingY);
        }

        public Label() 
		{
            Font = FontAsset.Default;
			Padding = new Vector4(2, 2, 2, 2);
            _glyphOffset = new Vector2(0, 0);
        }

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRectangle(AbsolutePosition, Size, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);
			
			if (ClipContents)
				Manager?.Scissors.Clip(ClientArea);

			if (Format.HasFlag(StringFormatFlags.DirectionVertical)) Painter.String(Size, ForeColor, Font, _glyphs, 90);
			else Painter.String(Size, ForeColor, Font, _glyphs);

			if (ClipContents)
				Manager?.Scissors.Unclip();
			
			if (BorderWidth > 0)
				Painter.Rectangle(AbsolutePosition, Size, BorderColor, BorderWidth);
		}

        public override void Tick()
		{
			
		}
	}
}
