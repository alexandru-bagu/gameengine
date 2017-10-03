using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using GameEngine.Assets;
using System;
using GameEngine.Text;

namespace GameEngine.Graphics
{
    public class Painter
    {
        static float DegreesInRads = MathHelper.Pi / 180;

        public static RectangleF AsRectangle(Vector2 position, Vector2 size)
        {
            return new RectangleF(position.X, position.Y, size.X, size.Y);
        }

        public static RectangleF AsRectangle(float x, float y, float width, float height)
        {
            return new RectangleF(x, y, width, height);
        }

        public static void Texture(TextureAsset texture, RectangleF destinationRec, RectangleF? sourceRec = null)
        {
            Texture(texture, destinationRec, Color.White, sourceRec);
        }

        public static void Texture(TextureAsset texture, RectangleF destinationRec, Color color, RectangleF? sourceRec = null)
		{
			RectangleF source;
			if (sourceRec != null) source = sourceRec.Value;
			else source = texture.CropArea;

			Vector2[] vertices = new Vector2[] {
				new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0, 1)
			};

			float width = source.Width * (destinationRec.Width / source.Width);
			float height = source.Height * (destinationRec.Height / source.Height);
            
            if (texture.Scale != Vector3.One || Math.Abs(texture.RotationAngle) > 0)
			{
				GL.PushMatrix();
				GL.Translate(destinationRec.X + width / 2, destinationRec.Y + height / 2, 0f);
				GL.Scale(texture.Scale);
				GL.Rotate(texture.RotationAngle, texture.RotationAxis);
				GL.Translate(-(destinationRec.X + width / 2), -(destinationRec.Y + height / 2), 0f);
            }
            GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, texture.Id);
			GL.Begin(PrimitiveType.Quads);
            paintColor(color);

			for (int i = 0; i < vertices.Length; i++)
			{
				var vertex = vertices[i];
				GL.TexCoord2((source.Left + vertex.X * source.Width) / texture.Width, (source.Top + vertex.Y * source.Height) / texture.Height);

				vertex.X = vertex.X * width + destinationRec.X;
				vertex.Y = vertex.Y * height + destinationRec.Y;

				paintVertex(vertex);
			}

			GL.End();
            if (texture.Scale != Vector3.One || Math.Abs(texture.RotationAngle) > 0) GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);
        }

        private static void paintColor(Color color)
        {
            GL.Color4(PremultiplyAlpha(color));
        }

        public static Color PremultiplyAlpha(Color pixel)
        {
            return Color.FromArgb(
                pixel.A,
                PremultiplyAlpha_Component(pixel.R, pixel.A),
                PremultiplyAlpha_Component(pixel.G, pixel.A),
                PremultiplyAlpha_Component(pixel.B, pixel.A));
        }
        private static byte PremultiplyAlpha_Component(float source, float alpha)
        {
            return (byte)(source * alpha / 255f + 0.5f);
        }

        private static void paintVertex(Vector2 vertex)
        {
            GL.Vertex2(vertex);
        }

        private static void paintVertex(float x, float y)
        {
			paintVertex(new Vector2(x, y));
        }

        public static void Quad(Vector2[] vertices, Color color)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            paintColor(color);

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                paintVertex(vertex);
            }

            GL.End();
        }

        public static void TextureQuad(TextureAsset texture, Vector2[] vertices, Color color, RectangleF? sourceRec = null)
        {
            RectangleF source;
            if (sourceRec != null) source = sourceRec.Value;
            else source = texture.CropArea;

            Vector2[] texVertices = new Vector2[] {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0, 1)
            };
            
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            GL.Begin(PrimitiveType.Quads);
            paintColor(color);

            for (int i = 0; i < texVertices.Length; i++)
            {
                var vertex = texVertices[i];
                GL.TexCoord2((source.Left + vertex.X * source.Width) / texture.Width, (source.Top + vertex.Y * source.Height) / texture.Height);
                paintVertex(vertices[i]);
            }

            GL.End();
        }

        public static void FillCircle(float x, float y, float radius, Color fillcolor)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.TriangleFan);
            paintColor(fillcolor);
            float y1 = y;
            float x1 = x;

            for (int i = 0; i <= 360; i++)
            {
                float degInRad = i * DegreesInRads;
                float x2 = x + ((float)Math.Cos(degInRad) * radius);
                float y2 = y + ((float)Math.Sin(degInRad) * radius);
                paintVertex(x, y);
                paintVertex(x1, y1);
                paintVertex(x2, y2);
                y1 = y2;
                x1 = x2;
            }
            GL.End();
        }

        public static void Arc(float x, float y, float width, int start, int end, float radius, Color color)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(width);
            GL.Begin(PrimitiveType.TriangleFan);
            paintColor(color);

            for (int i = start; i <= end; i++)
            {
                float degInRad = i * DegreesInRads;
                paintVertex(x + ((float)Math.Cos(degInRad) * radius), y + ((float)Math.Sin(degInRad) * radius));
            }

            GL.End();
        }

        public static void Circle(float x, float y, float width, float radius, Color color)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(width);
            GL.Begin(PrimitiveType.LineLoop);
            paintColor(color);

            for (int i = 180; i <= 270; i++)
            {
                float degInRad = i * DegreesInRads;
                paintVertex(x + ((float)Math.Cos(degInRad) * radius), y + ((float)Math.Sin(degInRad) * radius));
            }

            GL.End();
        }

        public static void RoundedRectangle(float x, float y, float width, float height, float lineWidth, float radius, Color color)
        {
            //http://stackoverflow.com/a/33916344/1163478
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(lineWidth);
            GL.Begin(PrimitiveType.LineLoop);
            paintColor(color);
            y++; height--;
            // top-left corner
            roundedCorner(x, y + radius, 3 * MathHelper.Pi / 2, MathHelper.Pi / 2, radius, lineWidth);
            // top-right
            roundedCorner(x + width - radius, y, 0.0, MathHelper.Pi / 2, radius, lineWidth);
            // bottom-right
            roundedCorner(x + width - 1, y + height - radius, MathHelper.Pi / 2, MathHelper.Pi / 2, radius, lineWidth );
            // bottom-left
            roundedCorner(x + radius, y + height, MathHelper.Pi, MathHelper.Pi / 2, radius, lineWidth );

            GL.End();
        }

        public static void FillRoundedRectangle(float x, float y, float width, float height, float radius, Color color)
        {
            //http://stackoverflow.com/a/33916344/1163478
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.TriangleFan);
            paintColor(color);

            // top-left corner
            roundedCorner(x, y + radius, 3 * MathHelper.Pi / 2, MathHelper.Pi / 2, radius);
            // top-right
            roundedCorner(x + width - radius, y, 0.0, MathHelper.Pi / 2, radius);
            // bottom-right
            roundedCorner(x + width - 1, y + height - radius, MathHelper.Pi / 2, MathHelper.Pi / 2, radius);
            // bottom-left
            roundedCorner(x + radius, y + height, MathHelper.Pi, MathHelper.Pi / 2, radius);

            GL.End();
        }

        private static void roundedCorner(float x, float y, double sa, double arc, float r, float e = 0)
        {
            //http://stackoverflow.com/a/33916344/1163478
            const int rounding_pieces = 25;
            // centre of the arc, for clockwise sense
            float cent_x = (float)(x + r * Math.Cos(sa + MathHelper.PiOver2));
            float cent_y = (float)(y + r * Math.Sin(sa + MathHelper.PiOver2));
            float next_x, next_y;
            // build up piecemeal including end of the arc
            int n = (int)Math.Ceiling(rounding_pieces * arc / MathHelper.Pi * 2);
            for (int i = 0; i <= n; i++)
            {
                double ang = sa + arc * (double)i / (double)n;
                // compute the next point
                next_x = (float)(cent_x + r * Math.Sin(ang));
                next_y = (float)(cent_y - r * Math.Cos(ang));
                paintVertex(next_x, next_y);
            }
        }

        public static void String(Vector2 pos, Vector2 size, Color color, FontAsset font, GlyphLocation[] locations, float zRotation = 0)
        {
            foreach (var p in locations)
            {
                var g = p.Glyph;
                var position = p.Position + pos;
                var rotation = font.Asset.RotationAngle;
                font.Asset.RotateZ(zRotation);
                Texture(font.Asset, new RectangleF(position.X, position.Y, g.Width, g.Height), color, g.Source);
                font.Asset.RotateZ(rotation);
            }
        }


        public static void String(Vector2 size, Color color, FontAsset font, GlyphLocation[] locations, float zRotation = 0)
        {
            foreach (var p in locations)
            {
                var g = p.Glyph;
				var position = p.AbsolutePosition;
                var rotation = font.Asset.RotationAngle;
                font.Asset.RotateZ(zRotation);
                Texture(font.Asset, new RectangleF(position.X, position.Y, g.Width, g.Height), color, g.Source);
                font.Asset.RotateZ(rotation);
            }
        }

        public static void Rectangle(Vector2 position, Vector2 size, Color color, float lineWidth = 1)
        {
            Rectangle(new RectangleF(position.X, position.Y, size.X, size.Y), color, lineWidth);
        }
        
        public static void Rectangle(RectangleF destinationRec, Color color, float lineWidth = 1)
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0, 1)
            };

            float width = destinationRec.Width;
            float height = destinationRec.Height;
            
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(lineWidth);
            GL.Begin(PrimitiveType.LineLoop);
            paintColor(color);
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                vertex.X = vertex.X * width + destinationRec.X;
                vertex.Y = vertex.Y * height + destinationRec.Y;

                paintVertex(vertex);
            }

            GL.End();
        }

        public static void FillRectangle(Vector2 position, Vector2 size, Color color)
        {
            FillRectangle(new RectangleF(position.X, position.Y, size.X, size.Y), color);
        }

        public static void FillRectangle(RectangleF destinationRec, Color color)
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0, 1)
            };

            float width = destinationRec.Width;
            float height = destinationRec.Height;

            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            paintColor(color);

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                vertex.X = vertex.X * width + destinationRec.X;
                vertex.Y = vertex.Y * height + destinationRec.Y;

                paintVertex(vertex);
            }

            GL.End();
        }

        public static void Line(Vector2 point1, Vector2 point2, Color color, float width = 1)
        {
            Vector2[] vertices = new Vector2[] {
               point1, point2
            };
            
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(width);
            GL.Begin(PrimitiveType.Lines);
            paintColor(color);
            for (int i = 0; i < vertices.Length; i++)
                paintVertex(vertices[i]);

            GL.End();
        }

        public static void Begin(int viewWidth, int viewHeight, Vector3 translation)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0, viewWidth, viewHeight, 0, 0, 1f);
			GL.Translate(translation);
        }
    }
}
