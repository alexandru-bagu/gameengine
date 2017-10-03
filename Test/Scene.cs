using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using System.Drawing;

namespace Test
{
    public class Scene : GraphicsScene
    {
        int alpha = 0;

        public Scene(int width, int height) : base(width, height)
        {

        }

        protected override void Draw()
        {
            base.Draw();

            Painter.Rectangle(new OpenTK.Vector2(100, 100), new OpenTK.Vector2(200, 200), Color.FromArgb(alpha, Color.Black));
        }

        public override void Tick()
        {
            alpha = (alpha + 10) % 255;
            base.Tick();
        }
    }
}
