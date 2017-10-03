using System;
using OpenTK;
using GameEngine.Assets;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
	public class ProgressBar : Container
    {
		private bool _sealed = false;
		public override bool Sealed => _sealed;

        private Label _label;
        private double _progress;
        private int _marqueePosition;
        private bool _horizontal = true;

        public virtual TextureAsset ProgressTexture { get; set; } = null;
        public Color BackColor { get; set; } = SystemColors.Control;
        public Color BarColor { get; set; } = Color.Green;
        public Color BorderColor { get; set; } = SystemColors.ActiveBorder;
        public float BorderWidth { get; set; } = 1f;

        public double Maximum { get; set; } = 100;
        public double Minimum { get; set; } = 0;
        public bool ShowTrueValue { get; set; } = false;
        public bool ShowText { get; set; } = true;
        public bool Marquee { get; set; }

        public bool Horizontal
        {
            get { return _horizontal; }
            set { _horizontal = value; UpdateLayout(); }
        }

        public FontAsset Font
        {
            get { return _label.Font; }
			set { _label.Font = value; UpdateLayout(); }
        }

        public Color ForeColor
        {
            get { return _label.ForeColor; }
            set { _label.ForeColor = value; }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                _progress = Math.Round(value, 2);
                UpdateLayout();
            }
        }

		public ProgressBar()
		{
			_sealed = false;
			_label = new Label();
			_label.ClipContents = false;
			Add(_label);
			_sealed = true;
			Padding = new Vector4(2, 2, 2, 2);
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();

			if (!ShowTrueValue)
			{
				_label.Text = _progress.ToString() + "%";
			}
			else
			{
				var total = Maximum - Minimum;
				var value = _progress * total / 100;
				_label.Text = $"{value}/{total}";
			}
			this.Center(_label);
			if (_horizontal) _label.RotationAngle = 0;
			else _label.RotationAngle = 90;
		}

        protected override void Draw()
        {
            if (Texture == null)
                Painter.FillRectangle(AbsolutePosition, Size, BackColor);
            else
                Painter.Texture(Texture, ClientArea, Color.White);

            base.Draw();

            if (BorderWidth > 0)
				Painter.Rectangle(AbsolutePosition, Size, BorderColor, BorderWidth);
        }

		public override void DrawObjects()
		{
			Manager?.Scissors.Clip(ClientArea);
			if (Horizontal)
				horizontalProgress();
			else
				verticalProgress();
			Manager?.Scissors.Unclip();

			if (ShowText && !Marquee)
				base.DrawObjects();
		}

		private void verticalProgress()
		{
			var pos = AbsolutePosition + Padding.Xy;
			if (Marquee)
			{
				var sz = new Vector2(Math.Max(Width - Padding.X - Padding.W - 1, 1), Math.Max(Height / 5 - Padding.Y - Padding.Z, 1));
				var y = _marqueePosition % Height;

				if (ProgressTexture == null)
				{
					Painter.FillRectangle(pos + new Vector2(0, y), sz, BarColor);
					Painter.FillRectangle(pos + new Vector2(0, y - Height), sz, BarColor);
				}
				else
				{
					Painter.Texture(ProgressTexture, Painter.AsRectangle(pos + new Vector2(0, y), sz), Color.White);
					Painter.Texture(ProgressTexture, Painter.AsRectangle(pos + new Vector2(0, y - Height), sz), Color.White);
				}
			}
			else
			{
				var sz = new Vector2(Math.Max(Width - Padding.X - Padding.W - 1, 1), Math.Max((float)(Height / 100 * Progress) - Padding.Y - Padding.Z, 1));

				if (sz.X > 0)
				{
					if (ProgressTexture == null)
						Painter.FillRectangle(pos, sz, BarColor);
					else
						Painter.Texture(ProgressTexture, Painter.AsRectangle(pos, sz), Color.White);
				}
			}
		}

		private void horizontalProgress()
		{
			if (Marquee)
			{
				var pos = AbsolutePosition + Padding.Xy;
				var sz = new Vector2(Math.Max(Width / 5 - Padding.X - Padding.W, 1), Math.Max(Height - Padding.Y - Padding.Z - 1, 1));
				var x = _marqueePosition % Width;

				if (ProgressTexture == null)
				{
					Painter.FillRectangle(pos + new Vector2(x, 0), sz, BarColor);
					Painter.FillRectangle(pos + new Vector2(x - Width, 0), sz, BarColor);
				}
				else
				{
					Painter.Texture(ProgressTexture, Painter.AsRectangle(pos + new Vector2(x, 0), sz), Color.White);
					Painter.Texture(ProgressTexture, Painter.AsRectangle(pos + new Vector2(x - Width, 0), sz), Color.White);
				}
			}
			else
			{
				var pos = AbsolutePosition + Padding.Xy;
				var sz = new Vector2(Math.Max((float)(Width / 100 * Progress) - Padding.X - Padding.W, 1), Math.Max(Height - Padding.Y - Padding.Z - 1, 1));

				if (sz.X > 0)
				{
					if (ProgressTexture == null)
						Painter.FillRectangle(pos, sz, BarColor);
					else
						Painter.Texture(ProgressTexture, Painter.AsRectangle(pos, sz), Color.White);
				}
			}
		}

        public override void Tick()
        {
            _marqueePosition++;
        }
    }
}
