using GameEngine.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GameEngine.Text
{
    public class Atlas
    {
        private Dictionary<char, Glyph> _glyphs;
        private Bitmap _texture;
        private float _size, _height;

        public Dictionary<char, Glyph> Glyphs => _glyphs;
        public Bitmap Texture => _texture;
        public float FontSize => _size;
        public float FontHeight => _height;

        public Atlas(Dictionary<char, Glyph> glyphs, Bitmap bmp, float size, float height)
        {
            _glyphs = glyphs;
            _texture = bmp;
            _size = size;
            _height = height;
        }

        public Atlas(string file)
        {
            if (!AssetManager.AssetExists(file)) throw new Exception($"Font {file} is missing.");
           _glyphs = new Dictionary<char, Glyph>();

            using (var stream = new FileStream(file, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                _size = reader.ReadSingle();
                _height = reader.ReadSingle();
                int count = reader.ReadInt32();
                while (count-- > 0)
                {
                    Glyph g = new Glyph(reader.ReadChar(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    _glyphs.Add(g.Char, g);
                }
                _texture = getBitmap(reader.ReadBytes(reader.ReadInt32()));
            }
        }
        
        public void Save(string file)
        {
            using (var stream = new FileStream(file, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                var bytes = getBytes(_texture);
                writer.Write(_size);
                writer.Write(_height);
                writer.Write(_glyphs.Count);
                foreach(var glyph in _glyphs.Values)
                {
                    writer.Write(glyph.Char);
                    writer.Write(glyph.Source.X);
                    writer.Write(glyph.Source.Y);
                    writer.Write(glyph.Source.Width);
                    writer.Write(glyph.Source.Height);
                }
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }

        private byte[] getBytes(Bitmap bmp)
        {
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private Bitmap getBitmap(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var bmp = new Bitmap(stream);
                var newBmp = new Bitmap(bmp.Width, bmp.Height);
                var g = System.Drawing.Graphics.FromImage(newBmp);
                g.DrawImage(bmp, new PointF(0, 0));
                return newBmp;
            }
        }
    }
}
