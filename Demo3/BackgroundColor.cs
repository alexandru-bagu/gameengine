using System;
using GameEngine.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Demo3
{
    public class BackgroundColor : Drawable
    {
		private HSLColor _color = Color.LimeGreen;

        public override void Tick()
        {
			_color = Color.Green;
            _color = _color.AddHue(0.001f);
        }

        protected override void Draw()
        {
            var c = (Color)_color;
            var c1 = ((HSLColor)c).AddLighting(0.2f);//.AddSaturation(-0.5f);
            var c2 = ((HSLColor)c).AddLighting(0.4f);//.AddSaturation(0);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(c1);
            GL.Vertex2(0, 0);
            GL.Color3(c1);
            GL.Vertex2(Width, 0);
            GL.Color3(c2);
            GL.Vertex2(Width, Height);
            GL.Color3(c2);
            GL.Vertex2(0, Height);
            GL.End();
        }
    }
}
