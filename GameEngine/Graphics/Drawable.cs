using GameEngine.Assets;
using GameEngine.Collision;
using GameEngine.Graphics.Drawables;
using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace GameEngine.Graphics
{
    public abstract class Drawable : ICollider
    {
        private DragInformation _drag;
        private Vector2 _position, _size, _absolutePosition, _paddedAbsolutePosition, _offset;
        private Vector4 _padding;
        private RectangleF _clip, _padlessClip;
        private Container _parent;
        private GameManager _manager;

        public event Action<Drawable, Vector2> MouseEnter, MouseLeave, MouseMove;
        public event Action<Drawable, DragInformation, Vector2> DragStart, Drag, DragEnd;
        public event Action<Drawable, Vector2, MouseButton> MouseDown, MouseUp, MouseClick;
        public event Action<Drawable, Key> KeyDown, KeyUp, KeyPress;
        public event Action<Drawable> GotFocus, LostFocus, PerformLayout;

        public virtual TextureAsset Texture { get; set; } = null;
        public virtual ContextMenu ContextMenu { get; set; }
        public virtual string Name { get; set; } = "";
        public object Tag { get; set; }

        public MouseButton ContextMenuButton { get; set; } = MouseButton.Right;
        public MouseButton DragButton { get; set; } = MouseButton.Left;
        public bool Draggable { get; set; } = true;
        public bool ClipContents { get; set; } = true;
        public Vector3 RotationAxis { get; set; } = Vector3.UnitZ;
        public float RotationAngle { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public virtual bool Focusable { get; } = false;
        public bool Visible { get; set; } = true;

        public bool HasFocus => Manager?.CurrentScene.KeyboardFocus == this;
        public float Width => Size.X;
        public float Height => Size.Y;
        public virtual RectangleF WorkingArea => _clip;
        public virtual RectangleF ClientArea => _clip;
        public virtual RectangleF PadlessClientArea => _padlessClip;
        public virtual Vector2 AbsolutePosition => _absolutePosition;
        public virtual Vector2 PaddedAbsolutePosition => _paddedAbsolutePosition;

        public virtual Container Parent
        {
            get { return _parent; }
            set { _parent = value; UpdateLayout(); }
        }
        public virtual Vector2 Position
        {
            get { return _position; }
            set { if (!_position.Equals(value)) { _position = value; UpdateLayout(); } }
        }
        public virtual Vector2 Size
        {
            get { return _size; }
            set { if (!_size.Equals(value)) { _size = value; UpdateLayout(); } }
        }
        public virtual Vector2 Offset
        {
            get { return _offset; }
            set { if (!_offset.Equals(value)) { _offset = value; UpdateLayout(); } }
        }
        public virtual Vector4 Padding
        {
            get { return _padding; }
            set { if (!_padding.Equals(value)) { _padding = value; UpdateLayout(); } }
        }
        public virtual GameManager Manager
        {
            get { return _manager; }
            set { if (_manager == null || !_manager.Equals(value)) { _manager = value; UpdateLayout(); } }
        }

        protected abstract void Draw();
        public abstract void Tick();
        
        internal void InternalUpdateLayout()
		{
			UpdateLayout();
		}

        protected virtual void UpdateLayout()
        {
            _absolutePosition = Position + _offset;
            _paddedAbsolutePosition = Position + _offset + _padding.Xy;
            if (Parent != null)
            {
                _absolutePosition += Parent.AbsolutePosition;
                _paddedAbsolutePosition += Parent.AbsolutePosition;
            }
            _clip = new RectangleF(_absolutePosition.X, _absolutePosition.Y, Width, Height);
            _padlessClip = new RectangleF(_paddedAbsolutePosition.X, _paddedAbsolutePosition.Y, Width - _padding.X - _padding.W, Height - _padding.Y - _padding.Z);
            if (_parent != null) _manager = _parent._manager;
            PerformLayout?.Invoke(this);
        }

		public virtual bool Pick(Vector2 position)
        {
            if (!Visible) return false;
			if (_clip.Contains(position.X, position.Y)) return true;
            return false;
        }

        public virtual Drawable PickElement(Vector2 position)
        {
            if (!Visible) return null;
            if (Pick(position)) return this;
            return null;
        }

        public virtual void ProcessDrag(DragInformation information, Vector2 position)
        {
            Position = information.ReferencePoint - information.RelativeMouseStart + position;
        }

        internal virtual void OnMouseEnter(Drawable src, Vector2 position)
        {
			MouseEnter?.Invoke(src, position - _absolutePosition);
        }

        internal virtual void OnMouseLeave(Drawable src, Vector2 position)
        {
			MouseLeave?.Invoke(src, position - _absolutePosition);
        }

        internal virtual void OnMouseMove(Drawable src, Vector2 position)
        {
            if (Draggable && _drag != null) OnDrag(this, _drag.Compute(position));
			MouseMove?.Invoke(src, position - _absolutePosition);
        }

        internal virtual void OnDrag(Drawable src, Vector2 mouseDelta)
        {
            Drag?.Invoke(src, _drag, mouseDelta);
        }

        internal virtual void OnMouseClick(Drawable src, Vector2 position, MouseButton button)
        {
            Focus(false);
			MouseClick?.Invoke(src, position - _absolutePosition, button);
            if (ContextMenu != null && ContextMenuButton == button)
                ContextMenu.Show(this, position);
        }
        
        internal virtual void OnMouseUp(Drawable src, Vector2 position, MouseButton button)
        {
            if (Manager != null) Manager.CurrentScene.MouseFocus = null;
            if (Draggable && _drag != null) { DragEnd?.Invoke(src, _drag, position); _drag = null; }
			MouseUp?.Invoke(src, position - _absolutePosition, button);
        }

        internal virtual void OnMouseDown(Drawable src, Vector2 position, MouseButton button)
		{
            Focus();
			MouseDown?.Invoke(src, position - _absolutePosition, button);
            if (Draggable && button == DragButton)
            {
				_drag = new DragInformation(position, position - _absolutePosition, Position);
                DragStart?.Invoke(this, _drag, position);
            }
        }

        internal virtual void OnKeyPress(Drawable src, Key key)
        {
            KeyPress?.Invoke(src, key);
        }

        internal virtual void OnKeyUp(Drawable src, Key key)
        {
            KeyUp?.Invoke(src, key);
        }

        internal virtual void OnKeyDown(Drawable src, Key key)
        {
            KeyDown?.Invoke(src, key);
        }

        public virtual void Focus(bool mouseFocus = true)
        {
            if (Manager != null)
            {
                if (mouseFocus) Manager.CurrentScene.MouseFocus = this;
                if (Focusable)
                {
                    Manager.CurrentScene.KeyboardFocus = this;
					InvokeGotFocus();
                }
            }
        }

        internal virtual void Unfocus()
        {
            if (Focusable)
            {
				InvokeLostFocus();
            }
        }

		internal void InvokeGotFocus()
		{
			GotFocus?.Invoke(this);
		}

		internal void InvokeLostFocus()
		{
			LostFocus?.Invoke(this);
		}

		public void InvokeDraw()
		{
			if (Visible)
			{
				BeginTransformation();
				Draw();
				EndTransformation();
			}
		}

		public virtual void BeginTransformation()
		{
			var area = ClientArea;
			var translationX = area.X + area.Width / 2;
			var translationY = area.Y + area.Height / 2;
			if (Math.Abs(RotationAngle) > 0 || Scale != Vector3.One)
			{
				GL.PushMatrix();
				GL.Translate(translationX, translationY, 0f);
				GL.Scale(Scale);
				GL.Rotate(RotationAngle, RotationAxis);
				GL.Translate(-translationX, -translationY, 0f);
			}
		}

		public virtual void EndTransformation()
		{
			if (Math.Abs(RotationAngle) > 0 || Scale != Vector3.One)
			{
				GL.PopMatrix();
			}
		}
    }
}
