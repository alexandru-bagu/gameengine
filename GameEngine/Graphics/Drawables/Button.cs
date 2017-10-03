using System;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using OpenTK.Input;

namespace GameEngine.Graphics.Drawables
{
	public class Button : Container
    {
		private bool _sealed;
		public override bool Sealed => _sealed;

        private Label _label;
        private Color 
			_backColor = SystemColors.Control,
			_highlightColor = Color.FromArgb(224, 238, 251), 
			_pressColor = Color.FromArgb(208, 230, 251), _background;
        private Color _border;
        private ContentAlignment _align = ContentAlignment.MiddleCenter;
		private bool _autoSize;
        private bool _mouseDown, _mouseOver;

		public float BorderWidth { get; set; } = 1f;
		public MouseButton MainButton { get; set; } = MouseButton.Left;
		public bool Rounded { get; set; } = false;

        public string Text
        {
            get { return _label.Text; }
            set { _label.Text = value; UpdateLayout(); }
        }

        public StringFormatFlags Format
        {
            get { return _label.Format; }
            set { _label.Format = value; UpdateLayout(); }
        }

        public FontAsset Font
        {
            get { return _label.Font; }
            set { _label.Font = value; UpdateLayout(); }
        }

        public ContentAlignment TextAlign
        {
            get { return _align; }
            set { _align = value; UpdateLayout(); }
        }

		public bool AutoSize
		{
			get { return _autoSize; }
			set { _autoSize = value; UpdateLayout(); }
		}

        public Color ForeColor
        {
            get { return _label.ForeColor; }
            set { _label.ForeColor = value; }
        }

        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; updateColor(); }
        }

        public Color HighlightColor
        {
            get { return _highlightColor; }
            set { _highlightColor = value; updateColor(); }
        }

        public Color PressColor
        {
            get { return _pressColor; }
            set { _pressColor  = value; updateColor(); }
        }

        public Button()
		{
			_sealed = false;
            _label = new Label();
			Add(_label);
			_sealed = true;
			Padding = Vector4.One * 2;
			_autoSize = false;

            MouseEnter += Button_MouseEnter;
            MouseLeave += Button_MouseLeave;
            MouseDown += Button_MouseDown;
            MouseUp += Button_MouseUp;

            _background = BackColor;
            _border = darkerBlue(_background);
        }

        private void Button_MouseUp(Drawable arg1, Vector2 arg2, MouseButton arg3)
        {
            if (arg3 == MainButton)
            {
                _mouseDown = false;
                if (Size.X > arg2.X && Size.Y > arg2.Y && arg2.X >= 0 && arg2.Y >= 0)
                    _mouseOver = true;
                else
                    _mouseOver = false;
                updateColor();
            }
        }

        private void updateColor()
        {
            if (_mouseDown)
            {
                _background = PressColor;
            }
            else if (_mouseOver)
            {
                _background = HighlightColor;
            }
            else
            {
                _background = BackColor;
            }
            _border = darkerBlue(_background);
        }

        private void Button_MouseDown(Drawable arg1, Vector2 arg2, MouseButton arg3)
        {
            if (arg3 == MainButton)
            {
                _mouseDown = true;
                updateColor();
            }
        }

        private void Button_MouseLeave(Drawable arg1, Vector2 arg2)
        {
            _mouseOver = false;
            updateColor();
        }

        private void Button_MouseEnter(Drawable arg1, Vector2 arg2)
        {
            _mouseOver = true;
            updateColor();
        }

		protected override void UpdateLayout()
		{
			if (_autoSize)
				Size = _label.Size + Padding.PaddingVector() + Vector2.One * 4;
			base.UpdateLayout();
			_label.Position = computeOffset(_label.Size.X, _label.Size.Y);
        }

        private Vector2 computeOffset(float width, float height)
        {
            float offsetX = 0, offsetY = 0, paddingX = 0, paddingY = 0;
			if (_align == ContentAlignment.TopLeft || _align == ContentAlignment.MiddleLeft || _align == ContentAlignment.BottomLeft) { offsetX = 0; paddingX = Padding.X; }
			if (_align == ContentAlignment.TopCenter || _align == ContentAlignment.MiddleCenter || _align == ContentAlignment.BottomCenter) { offsetX = (Size.X - width) / 2; paddingX = -(Padding.X + Padding.W) / 2; }
            if (_align == ContentAlignment.TopRight || _align == ContentAlignment.MiddleRight || _align == ContentAlignment.BottomRight) { offsetX = Size.X - width; paddingX = -Padding.W - 1; }

			if (_align == ContentAlignment.TopLeft || _align == ContentAlignment.TopCenter || _align == ContentAlignment.TopRight) { offsetY = 0; paddingY = Padding.Y; }
			if (_align == ContentAlignment.MiddleLeft || _align == ContentAlignment.MiddleCenter || _align == ContentAlignment.MiddleRight) { offsetY = (Size.Y - height) / 2; paddingY = -(Padding.Y + Padding.Z) / 2; }
            if (_align == ContentAlignment.BottomLeft || _align == ContentAlignment.BottomCenter || _align == ContentAlignment.BottomRight) { offsetY = Size.Y - height; paddingY = -Padding.Z - 1; }

			return new Vector2(offsetX + Padding.X - Padding.W, offsetY + Padding.Y - Padding.Z);
        }

        private Color darkerBlue(Color color)
        {
            int k = 100;
            return Color.FromArgb(color.A, Math.Max(0, color.R - k), Math.Max(0, color.G - k / 2), color.B);
        }

        protected override void Draw()
        {
            var p = AbsolutePosition;

			if (Texture == null)
			{
				if (Rounded)
					Painter.FillRoundedRectangle(p.X, p.Y, Width, Height, 5, _background);
				else
					Painter.FillRectangle(AbsolutePosition, Size, _background);
			}
			else
			{
				Painter.Texture(Texture, ClientArea, Color.White);
			}

            base.Draw();

            if (Rounded)
            {
                Painter.RoundedRectangle(p.X, p.Y, Width, Height, BorderWidth, 5, _border);
            }
            else
            {
                Painter.Rectangle(AbsolutePosition, Size, _border, BorderWidth);
            }
        }

		public override Drawable PickElement(Vector2 position)
		{
			return this;
		}
    }
}