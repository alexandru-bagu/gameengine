using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GameEngine.Graphics
{
	public class Scissor
	{
		private Stack<Rectangle> _stack = new Stack<Rectangle>();
        private GameManager _manager;

        public Scissor(GameManager manager)
        {
            _manager = manager;
        }

        public void Clip(float x, float y, float width, float height)
        {
            Clip(new Rectangle((int)x, (int)y, (int)width, (int)height));
        }

        public void Clip(Vector2 position, float width, float height)
        {
            Clip(new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height));
        }

        public void Clip(RectangleF rect)
        {
            Clip(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
        }

        public void Clip(Rectangle rect)
		{
            if (_stack.Count == 0)
                GL.Enable(EnableCap.ScissorTest);
            else
                rect = Rectangle.Intersect(rect, _stack.Peek());

            _stack.Push(rect);
            ApplyClip(rect);
        }

        public void Unclip()
        {
            if (_stack.Count == 0) throw new Exception("Unblanaced stack.");
            _stack.Pop();

            if (_stack.Count == 0)
                GL.Disable(EnableCap.ScissorTest);
            else
                ApplyClip(_stack.Peek());
        }

        private void ApplyClip(Rectangle rect)
        {
            var x = (int)(Math.Floor((rect.X - _manager.Camera.Position.X) * _manager.Camera.Zoom));
            var y = (int)(Math.Floor(_manager.Height + (-rect.Height - rect.Y + _manager.Camera.Position.Y) * _manager.Camera.Zoom));
            var w = (int)(Math.Ceiling(rect.Width * _manager.Camera.Zoom));
            var h = (int)(Math.Ceiling(rect.Height * _manager.Camera.Zoom));
            x = (int)(x * _manager.AspectRatio.X);
            w = (int)(w * _manager.AspectRatio.X);
            y = (int)(y * _manager.AspectRatio.Y);
            h = (int)(h * _manager.AspectRatio.Y);

            GL.Scissor(x, y, w, h);
        }
	}
}
