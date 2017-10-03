using GameEngine.Assets;
using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using System.Drawing;

namespace Demo2
{
    public class Bullet : Texture
    {
        private static TextureAsset _spriteSheet = loadAsset();
        private static TextureAsset loadAsset()
        {
            return TextureAsset.LoadRelativePath("bullets_spritesheet.png");
        }

        public static TextureAsset GetBulletAsset()
        {
            return _spriteSheet.Copy().Crop(100, 26, 13, 11).RotateZ(-90);
        }

        private bool _enemy;
        public bool Enemy => _enemy;
        private TextureAsset _texture;

        public Bullet(bool enemy)
        {
            BackColor = Color.White;

            _enemy = enemy;
            if (_enemy) _texture = _spriteSheet.Copy().Crop(42, 26, 13, 11);
            else _texture = _spriteSheet.Copy().Crop(100, 26, 13, 11);
            Texture = _texture;
            Size = _texture.CropSize;
        }

        protected override void Draw()
        {
            Painter.Texture(_texture, ClientArea, BackColor, Texture.CropArea);
        }
    }
}