using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace GameEngine.Assets
{
	public class TextureAsset : Asset
	{
		private bool _disposed = false;
		private bool _canDisposeAsset;
		public int Width { get; protected set; }
		public int Height { get; protected set; }
        public Vector2 Size => new Vector2(Width, Height);
        public float CropWidth => CropArea.Width;
		public float CropHeight => CropArea.Height;
        public Vector2 CropSize => new Vector2(CropWidth, CropHeight);
		public RectangleF CropArea { get; protected set; }
		public Vector3 Scale { get; protected set; } = new Vector3(1, 1, 1);
		public float RotationAngle { get; protected set; } = 0;
		public Vector3 RotationAxis { get; protected set; } = new Vector3(0, 0, 1);

		public TextureAsset FlipX() { Scale = new Vector3(Scale.X * -1, Scale.Y, Scale.Z); return this; }
		public TextureAsset FlipY() { Scale = new Vector3(Scale.X, Scale.Y * -1, Scale.Z); return this; }
		public TextureAsset FlipZ() { Scale = new Vector3(Scale.X, Scale.Y, Scale.Z * -1); return this; }
		public TextureAsset RotateZ(float angle) { RotationAngle = angle; return this; }
		public TextureAsset Crop(float x, float y, float width, float height) { CropArea = new RectangleF(x, y, width, height); return this; }

		public TextureAsset()
		{
			_canDisposeAsset = false;
		}

		public TextureAsset Copy()
		{
			TextureAsset asset = new TextureAsset();
			asset.Width = Width;
			asset.Height = Height;
			asset.Scale = Scale;
			asset.RotationAngle = RotationAngle;
			asset.CropArea = CropArea;
			asset.Id = Id;
			return asset;
		}

		public static TextureAsset LoadRelativePath(params string[] pathPieces)
		{
			return LoadAbsolutePath(AssetManager.RelativeTexture(pathPieces));
		}

		public static TextureAsset LoadAbsolutePath(string path)
		{
			if (!AssetManager.AssetExists(path)) throw new Exception($"Texture {path} is missing.");
			return LoadFromImage(Image.FromFile(path));
		}

		public static TextureAsset LoadFromImage(Image image)
		{
			var glId = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, glId);

			var bmp = (Bitmap)image;
			var dim = new Rectangle(0, 0, bmp.Width, bmp.Height);
			var bits = bmp.LockBits(dim, ImageLockMode.ReadWrite, ImagingPixelFormat.Format32bppArgb);
            int bytesPerPixel = 4;
            int bytesPerLine = bytesPerPixel * bmp.Width;
            unsafe
            {
                byte* scan0 = (byte*)bits.Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    var line = scan0 + y * bits.Stride;
                    for (int x = 0; x < bytesPerLine; x += bytesPerPixel)
                    {
                        var blue = line[x] / 255.0;
                        var green = line[x + 1] / 255.0;
                        var red = line[x + 2] / 255.0;
                        var alpha = line[x + 3] / 255.0;
                        
                        blue *= alpha; green *= alpha; red *= alpha;

                        line[x] = (byte)(blue * 255.0);
                        line[x + 1] = (byte)(green * 255.0);
                        line[x + 2] = (byte)(red * 255.0);
                    }
                }
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, dim.Width, dim.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);
			bmp.UnlockBits(bits);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			return new TextureAsset() { Id = glId, Width = dim.Width, Height = dim.Height, CropArea = new RectangleF(0, 0, dim.Width, dim.Height), _canDisposeAsset = true };
		}

		public override void Dispose()
		{
			if (_canDisposeAsset && !_disposed)
			{
				_disposed = true;
				GL.DeleteTextures(1, new int[] { Id });
			}
		}
	}
}