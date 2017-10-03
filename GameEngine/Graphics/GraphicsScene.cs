using OpenTK;
using System.Collections.Generic;
using OpenTK.Input;
using System;
using GameEngine.Graphics.Drawables;

namespace GameEngine.Graphics
{
	public class GraphicsScene : Container
	{
		public delegate bool KeyEvent(GraphicsScene source, Key key);

		private bool _paused;
		private Vector2 _mousePosition, _savedPosition;
		private Drawable _mouseOver;
		private Drawable _mouseFocus, _keyboardFocus;

		/// <summary>
		/// PreviewKeys must be true to cancel the current event.
		/// </summary>
		public event KeyEvent PreviewKeyDown, PreviewKeyUp, PreviewKeyPress;
		public bool PreviewKeys { get; set; }
		public override bool Focusable => false;
		public Drawable MouseFocus
		{
			get { return _mouseFocus; }
			set { _mouseFocus = value; }
		}
		public Drawable KeyboardFocus
		{
			get { return _keyboardFocus; }
			set
			{
				if (_keyboardFocus != value)
				{
					var prev = _keyboardFocus;
					_keyboardFocus = value;
					if (prev != null) prev.Unfocus();
					_keyboardFocus.Focus();
				}
			}
		}

		public bool Paused => _paused;

		public GraphicsScene(int width, int height) : this(new Vector2(width, height)) { }

		public GraphicsScene(Vector2 size)
		{
			Size = size;
			_paused = true;
		}

		public virtual void Pause()
		{
			_paused = true;
		}

		public virtual void Unpause()
		{
			_paused = false;
		}

		public void SetView(GameManager game)
		{
			game.CurrentScene.Pause();
			game.SetScene(this);
			Unpause();
		}

		public void MoveCamera(GameManager game, View.TweenType type = View.TweenType.Instant)
		{
            game.Camera.SetPosition(Position, type);
        }

		public void SavePosition()
		{
			_savedPosition = Position;
		}
		public void RestorePosition()
		{
			Position = _savedPosition;
		}

		public override bool Pick(Vector2 position)
		{
			return false;
		}

		public override Drawable PickElement(Vector2 position)
		{
			for (int i = Objects.Count - 1; i >= 0; i--)
			{
				var obj = Objects[i];
				if (obj.Pick(position))
				{
					if (obj is IContainer)
						return ((IContainer)obj).PickElement(position);
					return obj;
				}
			}
			return null;
		}

		internal void ProcessMouseInput(MouseDevice mouse, Input input, Vector2 aspectRatio)
		{
			var position = new Vector2(mouse.X / aspectRatio.X, mouse.Y / aspectRatio.Y);
			position = position / (float)Manager.Camera.Zoom;
			position += Position;
			position += Manager.Translation.Xy * 4;

			if (MouseFocus == null)
			{
				var target = PickElement(position);
				if (position != _mousePosition)
				{
					if (target != null || _mouseOver != target)
					{
						if (_mouseOver != null)
							_mouseOver.OnMouseLeave(_mouseOver, position);
						if (target != null)
							target.OnMouseEnter(target, position);
						_mouseOver = target;
					}
					if (_mouseOver != null)
						_mouseOver.OnMouseMove(_mouseOver, position);
				}

				if (input.HasMouseInput())
				{
					if (target != null)
					{
						for (MouseButton i = MouseButton.Left; i < MouseButton.LastButton; i++)
						{
							if (input.MousePress(i))
								target.OnMouseDown(target, position, i);
							if (input.MouseRelease(i) && target.ClientArea.Contains(position.X, position.Y))
								target.OnMouseClick(target, position, i);
							if (input.MouseRelease(i))
								target.OnMouseUp(target, position, i);
						}
					}
				}
			}
			else
			{
				if (position != _mousePosition)
					MouseFocus.OnMouseMove(MouseFocus, position);

				if (input.HasMouseInput())
				{
					for (MouseButton i = MouseButton.Left; i < MouseButton.LastButton; i++)
					{
						if (input.MousePress(i) && MouseFocus != null)
							MouseFocus.OnMouseDown(MouseFocus, position, i);
						if (input.MouseRelease(i) && MouseFocus != null && MouseFocus.ClientArea.Contains(position.X, position.Y))
							MouseFocus.OnMouseClick(MouseFocus, position, i);
						if (input.MouseRelease(i) && MouseFocus != null)
							MouseFocus.OnMouseUp(MouseFocus, position, i);
					}
				}
			}

			if (position != _mousePosition)
				OnMouseMove(this, position);

			if (input.HasMouseInput())
			{
				for (MouseButton i = MouseButton.Left; i < MouseButton.LastButton; i++)
				{
					if (input.MousePress(i))
						OnMouseDown(this, position, i);
					if (input.MouseRelease(i))
						OnMouseClick(this, position, i);
					if (input.MouseRelease(i))
						OnMouseUp(this, position, i);
				}
			}

			_mousePosition = position;
		}

		internal virtual void OnKeyPress(Key key)
		{
			if (!PreviewKeys || (PreviewKeys && !(processNullableBoolean(PreviewKeyPress?.Invoke(this, key)))))
			{
				if (KeyboardFocus != null)
				{
					KeyboardFocus.OnKeyPress(KeyboardFocus, key);
				}
			}
		}
		internal virtual void OnKeyUp(Key key)
		{
			if (!PreviewKeys || (PreviewKeys && !(processNullableBoolean(PreviewKeyUp?.Invoke(this, key)))))
			{
				if (KeyboardFocus != null)
				{
					KeyboardFocus.OnKeyUp(KeyboardFocus, key);
				}
			}
		}
		internal virtual void OnKeyDown(Key key)
		{
			if (!PreviewKeys || (PreviewKeys && !(processNullableBoolean(PreviewKeyDown?.Invoke(this, key)))))
			{
				if (KeyboardFocus != null)
				{
					KeyboardFocus.OnKeyDown(KeyboardFocus, key);
				}
			}
		}
		private bool processNullableBoolean(bool? value)
		{
			if (value.HasValue) return value.Value;
			return false;
		}

		public override void Tick()
		{
			foreach (var obj in Objects)
				obj.Tick();
		}

		public override void Focus(bool mouseFocus = true)
		{
			if (Manager.CurrentScene != this)
				MouseFocus = null;
		}
		internal override void Unfocus()
		{
			MouseFocus = null;
		}
	}
}