using OpenTK;
using System.Drawing;
using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace GameEngine.Graphics.Drawables
{
	public class ContextMenu : Panel
    {
        private Vector2 _minimumSize = new Vector2();
        private Drawable _src;

        public event Action<Drawable, Drawable, MouseButton> SelectOption;
        public event Action<Drawable, Drawable> PreShow;

        public override Container Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                if (base.Parent != null)
                    base.Parent.MouseDown -= Parent_MouseDown;
                base.Parent = value;
                if (base.Parent != null)
                    base.Parent.MouseDown += Parent_MouseDown;
            }
        }

        private void Parent_MouseDown(Drawable arg1, Vector2 arg2, MouseButton arg3)
        {
			if (!PadlessClientArea.Contains(arg2.X, arg2.Y))
                hide();
        }

		public ContextMenu()
		{
			Visible = false;
			BorderWidth = 1;
			BorderColor = Color.Black;
			BackColor = Color.Transparent;
		}

        private float getLastToolY()
        {
            if (Objects.Count == 0) return 0;
            var obj = Objects[Objects.Count - 1];
			if (obj is Line) return obj.Position.Y;
			return obj.Position.Y + obj.Height;
        }

        public void Option(string option)
        {
            Label label = new Label();
            label.Text = option;
            label.Name = option;
            var size = label.Size;
            if (_minimumSize.X < size.X) _minimumSize = size;
            label.AutoSize = false;
            label.Size = new Vector2(Width, 30);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.ForeColor = Color.Black;
            label.BackColor = Color.White;
            label.Position = new Vector2(0, getLastToolY());
            label.MouseEnter += Label_MouseEnter;
            label.MouseLeave += Label_MouseLeave;
            label.BorderWidth = 1;
            label.MouseClick += Label_MouseClick;
            Add(label);
            resetSizes();
        }

        public void Separate()
        {
            var line = new Line();
            line.Point1 = new Vector2(0, getLastToolY());
            line.Point2 = new Vector2(Width, getLastToolY());
			line.Color = Color.Black;
			line.LineWidth = 5;
            Add(line);
        }

		public override void Clear()
        {
            _minimumSize = new Vector2();
			base.Clear();
        }

        private void Label_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
        {
            SelectOption?.Invoke(_src, arg1, arg3);
            hide();
        }

        private void hide()
        {
            _src = null;
            Visible = false;
        }

        private void Label_MouseLeave(Drawable arg1, Vector2 arg2)
        {
            var label = (Label)arg1;
            label.BackColor = Color.White;
        }

        private void Label_MouseEnter(Drawable arg1, Vector2 arg2)
        {
            var label = (Label)arg1;
            label.BackColor = Color.LightBlue;
        }

        private void resetSizes()
        {
            foreach (var obj in Objects)
            {
                if (obj is Line)
                    (obj as Line).Point2 = new Vector2(_minimumSize.X + 4, (obj as Line).Point2.Y);
                else
                    obj.Size = new Vector2(_minimumSize.X + 4, obj.Height);
            }
            Size = new Vector2(_minimumSize.X + 4, getLastToolY() - 1);
        }

        public void Show(Drawable src, Vector2 position)
        {
            _src = src;
            Visible = true;
            PreShow?.Invoke(this, src);
            var d = Parent.Size - position;

            if (d.X < Width && d.Y >= Height) d = new Vector2(Width - d.X, 0);
            else if (d.X >= Width && d.Y < Height) d = new Vector2(0, Height - d.Y);
            else if (d.X < Width && d.Y < Height) d = new Vector2(Width - d.X, Height - d.Y);
            else d = new Vector2(0, 0);

            Position = position - d;
            Parent.BringToFront(this);
        }

        public override bool Pick(Vector2 position)
        {
            if (!Visible) return false;
            return base.Pick(position);
        }

        public override Drawable PickElement(Vector2 position)
        {
            if (!Visible) return null;
            return base.PickElement(position);
        }

		public override IContainer PickContainer(Vector2 position, Drawable exclude)
		{
			if (!Visible) return null;
			return base.PickContainer(position, exclude);
		}
    }
}