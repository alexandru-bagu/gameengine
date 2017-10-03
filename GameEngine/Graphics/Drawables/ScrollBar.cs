using System;
using System.Drawing;
using OpenTK;

namespace GameEngine.Graphics.Drawables
{
	public class ScrollBar : Panel
	{
		private bool _sealed;
		public override bool Sealed => _sealed;
		private Button _button;
		private bool _vertical;
		private float _parentMeasurement, _scrollMeasurement;
		private float _scrollPercentage, _scrollOffset;

		public Button Button => _button;

		public bool Vertical
		{
			get { return _vertical; }
			set { _vertical = value; UpdateLayout(); }
		}

		public override Container Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				if(base.Parent != null)
					base.Parent.PerformLayout -= performLayout;
				base.Parent = value;
				if (base.Parent != null)
					base.Parent.PerformLayout += performLayout;
			}
		}

		public ScrollBar(int size)
		{
			Vertical = true;
			Size = new Vector2(size, size);
			_sealed = false;
			_button = new Button();
			_button.Draggable = true;
			_button.Drag += _button_Drag;
			Add(_button);
			_sealed = true;
			Padding = Vector4.One;
		}

		private void performLayout(Drawable obj)
		{
			InternalUpdateLayout();
		}

		private void _button_Drag(Drawable arg1, DragInformation information, Vector2 position)
		{
			var oldPosition = _button.Position;
			_button.ProcessDrag(information, position);
			var newPosition = _button.Position;

			if (Vertical)
			{
				newPosition.X = oldPosition.X;
				if (newPosition.Y < Padding.Y) newPosition.Y = Padding.Y;
				if (newPosition.Y > Height - _button.Height - Padding.Y - Padding.Z) newPosition.Y = Height - _button.Height - Padding.Y - Padding.Z;

				_scrollPercentage = (newPosition.Y - Padding.Y) / (_scrollMeasurement - Padding.Z);
				if (_scrollPercentage < 0) _scrollPercentage = 0;
				if (_scrollPercentage > 1) _scrollPercentage = 1;
				_scrollOffset = _parentMeasurement * _scrollPercentage;
			}
			else
			{
				newPosition.Y = oldPosition.Y;
				if (newPosition.X < Padding.X) newPosition.X = Padding.X;
				if (newPosition.X > Width - _button.Width - Padding.X - Padding.W) newPosition.X = Width - _button.Width - Padding.X - Padding.W;

				_scrollPercentage = (newPosition.X - Padding.X) / (_scrollMeasurement - Padding.W);
				if (_scrollPercentage < 0) _scrollPercentage = 0;
				if (_scrollPercentage > 1) _scrollPercentage = 1;
				_scrollOffset = _parentMeasurement * _scrollPercentage;
			}
			_button.Position = newPosition;

			if (!oldPosition.Equals(newPosition))
				updateParentOffset();
		}

		private void refreshOffset()
		{
			var oldPosition = _button.Position;
			var newPosition = _button.Position;

			if (Vertical)
			{
				newPosition.X = oldPosition.X;
				if (newPosition.Y < Padding.Y) newPosition.Y = Padding.Y;
				if (newPosition.Y > Height - _button.Height - Padding.Y - Padding.Z) newPosition.Y = Height - _button.Height - Padding.Y - Padding.Z;

				_scrollPercentage = (newPosition.Y - Padding.Y) / (_scrollMeasurement - Padding.Z);
				if (_scrollPercentage < 0) _scrollPercentage = 0;
				if (_scrollPercentage > 1) _scrollPercentage = 1;
				_scrollOffset = _parentMeasurement * _scrollPercentage;
			}
			else
			{
				newPosition.Y = oldPosition.Y;
				if (newPosition.X < Padding.X) newPosition.X = Padding.X;
				if (newPosition.X > Width - _button.Width - Padding.X - Padding.W) newPosition.X = Width - _button.Width - Padding.X - Padding.W;

				_scrollPercentage = (newPosition.X - Padding.X) / (_scrollMeasurement - Padding.W);
				if (_scrollPercentage < 0) _scrollPercentage = 0;
				if (_scrollPercentage > 1) _scrollPercentage = 1;
				_scrollOffset = _parentMeasurement * _scrollPercentage;
			}
			_button.Position = newPosition;

			updateParentOffset();
		}

		private void updateParentOffset()
		{
			if (Parent == null) return;
			foreach (var obj in Parent.Objects)
			{
				if (!(obj is ScrollBar))
				{
					if (Vertical)
						obj.Offset = -new Vector2(obj.Offset.X, _scrollOffset);
					else 
						obj.Offset = -new Vector2(_scrollOffset, obj.Offset.Y);
				}
			}
		}

		private float getParentMaxWidth()
		{
			if (Parent == null) return 0;
			float max = 0;
			foreach (var obj in Parent.Objects)
				if (!(obj is ScrollBar))
					if (max < obj.Position.X + obj.Width)
						max = obj.Position.X + obj.Width;
			return max;
		}

		private float getParentMaxHeight()
		{
			if (Parent == null) return 0;
			float max = 0;
			foreach (var obj in Parent.Objects)
				if (!(obj is ScrollBar))
					if (max < obj.Position.Y + obj.Height)
						max = obj.Position.Y + obj.Height;
			return max;
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();

			if (Parent == null) return;

			if (Vertical)
			{
				Position = new Vector2(Parent.Width - Width, 0);
				Size = new Vector2(Width, Parent.Height);
			}
			else
			{
				Position = new Vector2(0, Parent.Height - Height);
				Size = new Vector2(Parent.Width, Height);
			}

			if (Vertical)
			{
				_button.Size = new Vector2(Width - Padding.X - Padding.Z, _button.Height);

				_parentMeasurement = getParentMaxHeight() - Parent.Height;

				if (_parentMeasurement < 0)
				{
					Visible = false;
				}
				else
				{
					Visible = true;

					var trueHeight = Height - Padding.Y - Padding.W;

					var ratio = _parentMeasurement / trueHeight;
					_button.Size = new Vector2(_button.Width, Math.Min(trueHeight / ratio / 2, Height * 0.8f));
					if (_button.Height < 10) _button.Size = new Vector2(_button.Width, 10);
					this.CenterHorizontally(_button);

					_scrollMeasurement = trueHeight - _button.Height;
					refreshOffset();
				}
			}
			else
			{
				_button.Size = new Vector2(Button.Width, Height - Padding.Y - Padding.W);

				_parentMeasurement = getParentMaxWidth() - Parent.Width;
				if (_parentMeasurement < 0)
				{
					Visible = false;
				}
				else
				{
					Visible = true;

					var trueWidth = Width - Padding.X - Padding.Z;

					var ratio = _parentMeasurement / trueWidth;
					_button.Size = new Vector2(Math.Min(trueWidth / ratio / 2, Width * 0.8f), _button.Height);
					if (_button.Width < 10) _button.Size = new Vector2(10, _button.Height);
					this.CenterHorizontally(_button);

					_scrollMeasurement = trueWidth - _button.Width;
					refreshOffset();
				}
			}
		}
	}
}
