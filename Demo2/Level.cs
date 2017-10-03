using System;
using OpenTK;
using System.Collections.Generic;
using GameEngine.Graphics;
using System.Drawing;

namespace Demo2
{
    public abstract class Level : IDisposable
    {
        protected float _width, _height;
        public virtual float Width => _width;
        public virtual float Height => _height;
        public abstract IEnumerable<Drawable> Platforms { get; }
        public abstract IEnumerable<RectangleF> PlatformBlocks { get; }

        public abstract void Dispose();

        public abstract void Build(float width, float height, Vector2 viewportSize, Vector2 viewportOffset, float xRatio = 1, float yRatio = 1);
        public abstract void Draw();

        public abstract void Tick(Vector2 camera, Vector2 viewportSize);
    }
}
