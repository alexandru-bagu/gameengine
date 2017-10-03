using System;
using OpenTK;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
	public class TrackBar : Container
	{
		private bool _sealed;
		public override bool Sealed => _sealed;

		private float _max;

		public event Action<TrackBar, float> ValueChanged;

		public override Vector2 Size
		{
			get { return base.Size; }
			set
			{
				if (value.Y < 20) value.Y = 20;
				base.Size = value;
			}
		}

		public Button TrackingButton { get; protected set; }
		public float Minimum { get; set; }
		public float Maximum { get; set; }
		public Color BackColor { get; set; } = SystemColors.Control;
		public Color BorderColor { get; set; } = SystemColors.ActiveBorder;
        public float TrackerWidth { get; set; } = 1f;
        public float BorderWidth { get; set; } = 1f;

        public float Value
		{
			get
			{
				return (Maximum - Minimum) * ((TrackingButton.Position.X - Padding.X) / _max);
			}
            set
            {
				TrackingButton.Position = new Vector2(_max / (((Maximum - Minimum)) / value) + Padding.X, TrackingButton.Position.Y);
            }
		}

		public TrackBar()
		{
			Maximum = 10;
			_sealed = false;
			TrackingButton = new Button();
			TrackingButton.Drag += _button_Drag;
			Add(TrackingButton);
			_sealed = true;

			Padding = new Vector4(4, 4, 4, 4);
		}

		private void _button_Drag(Drawable arg1, DragInformation information, Vector2 position)
		{
			var oldPosition = TrackingButton.Position;
			TrackingButton.ProcessDrag(information, position);
			var newPosition = TrackingButton.Position;

			var x = newPosition.X;
			if (x < Padding.X) x = Padding.X;
			if (x > Width - Padding.X - TrackingButton.Width) x = Width - Padding.X - TrackingButton.Width;

			TrackingButton.Position = new Vector2(x, oldPosition.Y);
			if (TrackingButton.Position != oldPosition)
				ValueChanged?.Invoke(this, Value);
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();

			TrackingButton.Size = new Vector2(10, Height - 2 * Padding.Y);

			var oldPosition = TrackingButton.Position;
			var x = oldPosition.X;
			if (x < Padding.X) x = Padding.X;
			if (x > Width - Padding.X - TrackingButton.Width) x = Width - Padding.X - TrackingButton.Width;
			TrackingButton.Position = new Vector2(x, oldPosition.Y);
			this.CenterVertically(TrackingButton);

			_max = Width - Padding.X * 2 - TrackingButton.Width;
		}

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRectangle(AbsolutePosition, Size, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);

            base.Draw();

            if(BorderWidth > 0) Painter.Rectangle(AbsolutePosition, Size, BorderColor, BorderWidth);
		}

		public override void DrawObjects()
		{
			Painter.Line(AbsolutePosition + new Vector2(Padding.X, Height / 2), AbsolutePosition + new Vector2(Width - Padding.X, Height / 2), BorderColor, TrackerWidth);
			base.DrawObjects();
		}
    }
}
