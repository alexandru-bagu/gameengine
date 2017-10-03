using GameEngine.Assets;
using GameEngine.Collision;
using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System;
using System.Diagnostics;

namespace Demo3
{
    public class GameScene : GraphicsScene
    {
        private TextureAsset _circle;
        private CollisionPlane _collision;
        private Stopwatch _stopwatch;
        private Random _random;
        private BackgroundColor _background;

        public GameScene(int width, int height) : base(width, height)
        {
            _circle = TextureAsset.LoadRelativePath("circle.png");
            _collision = new CollisionPlane(8, 0.99f, 0.9f, new Vector2(0f, .1f));
            _stopwatch = Stopwatch.StartNew();
            _random = new Random();
            _background = new BackgroundColor() { Size = Size };
            Add(_background);

            _collision.Add(new RectangleShape(null, new Vector2(0, 0), new Vector2(Width, 0)) { Static = true });
            _collision.Add(new RectangleShape(null, new Vector2(0, Height), new Vector2(Width, 10)) { Static = true });
            _collision.Add(new RectangleShape(null, new Vector2(0, 0), new Vector2(10, Height)) { Static = true });
            _collision.Add(new RectangleShape(null, new Vector2(Width - 10, 0), new Vector2(10, Height)) { Static = true });
        }

        public override void Tick()
        {
            base.Tick();

            if (_stopwatch.ElapsedMilliseconds >= 500 && Objects.Count < 7)
            {
                _stopwatch.Restart();

                Texture texture = new Texture();
                texture.Texture = _circle;
                texture.Size = new Vector2(60, 60);
                Add(texture);
                CircleShape shape = new CircleShape(texture, texture.Size / 2, texture.Width / 2);
                _collision.Add(shape);

                shape.Offset(new Vector2(Width / 2 + _random.Next(-5, 5), 0));
            }

            foreach (var obj in _collision.Objects)
                foreach (var collision in _collision.TickFreeMovement(obj))
                    if (collision.Static && obj.Velocity.LengthSquared <= 1)
                        obj.Velocity = new Vector2(0, 0);
            _collision.TickGravity();
        }
    }
}
