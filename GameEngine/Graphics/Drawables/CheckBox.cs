using System;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using OpenTK.Input;

namespace GameEngine.Graphics.Drawables
{
	public class CheckBox : Container
	{
		private bool _sealed;
		public override bool Sealed => _sealed;

		private Panel _checkBox;
		private Label _label;
		private bool _checked;

		public event Action<Drawable> CheckedChanged;

		public Color BackColor { get; set; } = Color.Transparent;
		public Color HoverColor { get; set; } = Color.CornflowerBlue;
		public Color DefaultColor { get; set; } = Color.Gray;

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

		public bool Checked
		{
			get { return _checked; }
			set { _checked = value; }
		}

		public Color ForeColor
		{
			get { return _label.ForeColor; }
			set { _label.ForeColor = value; }
		}

		public CheckBox()
		{
			_sealed = false;
			_checkBox = new Panel();
			_checkBox.BackColor = Color.White;
			_checkBox.BorderColor = DefaultColor;
			_checkBox.BorderWidth = 1;
			_checkBox.Size = new Vector2(13, 13);
			_label = new Label();
			Add(_checkBox);
			Add(_label);
			_sealed = true;

			MouseEnter += CheckBox_MouseEnter;
			MouseLeave += CheckBox_MouseLeave;
			MouseClick += CheckBox_MouseClick;
		}

		private void CheckBox_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
		{
			if (arg3 == MouseButton.Left)
			{
				var absolutePosition = arg2 + AbsolutePosition;
				if (ClientArea.Contains(absolutePosition.X, absolutePosition.Y))
				{
					_checked ^= true;
					CheckedChanged?.Invoke(this);
				}
			}
		}

		private void CheckBox_MouseLeave(Drawable arg1, Vector2 arg2)
		{
			_checkBox.BorderColor = DefaultColor;
		}

		private void CheckBox_MouseEnter(Drawable arg1, Vector2 arg2)
		{
			_checkBox.BorderColor = HoverColor;
		}

		protected override void UpdateLayout()
		{
			Size = new Vector2(_label.Size.X + _checkBox.Size.X, _label.Height) + new Vector2(Font.Size, 0);
			base.UpdateLayout();
			_checkBox.Position = new Vector2(Font.Size / 3, (Size.Y - _checkBox.Size.Y) / 2) + Padding.Xy;
			_label.Position = new Vector2(Font.Size * 2 / 3 + _checkBox.Size.X, (Size.Y - _label.Size.Y) / 2) + Padding.Xy;
		}

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRectangle(AbsolutePosition, Size, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);

            base.Draw();

            if (_checked)
			{
				var p1 = _checkBox.AbsolutePosition + new Vector2(3, 5);
				var p2 = _checkBox.AbsolutePosition + new Vector2(_checkBox.Size.X / 2, _checkBox.Size.Y) - new Vector2(0, 3);
				var p3 = _checkBox.AbsolutePosition + new Vector2(_checkBox.Size.X, 0) - new Vector2(2, -2);

				Painter.Line(p1, p2, Color.Black);
				Painter.Line(p2, p3, Color.Black);
			}
		}

		public override Drawable PickElement(Vector2 position)
		{
			return this;
		}
	}
}
