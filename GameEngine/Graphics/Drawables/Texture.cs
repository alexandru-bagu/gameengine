using GameEngine.Assets;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
    public class Texture : Drawable
    {
        public Color BackColor { get; set; } = Color.White;

        public Texture(TextureAsset asset = null)
        {
            Texture = asset;
        }

        protected override void Draw()
        {
            if (Texture == null) return;
            Painter.Texture(Texture, ClientArea, BackColor, Texture.CropArea);
        }

        public override void Tick()
        {

        }
    }
}
