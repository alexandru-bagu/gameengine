using GameEngine.Assets;
using GameEngine.Graphics;
using OpenTK;
using System;
using System.Drawing;

namespace Demo2
{
    public class BloodspatterGenerator : ParticleGenerator
    {
        private Random _random;

        public BloodspatterGenerator()
        {
            Texture = TextureAsset.LoadRelativePath("blood.png");
            _random = new Random();
        }

        public void Generate(Vector2 position, float angle)
        {
            angle *= (float)(180f / Math.PI);

            Generate(4 + _random.Next(3), Texture, () => Color.White, 500, 0, 100, position, Texture.Size / 4, 0.3f, angle, 60, 2, 1, 20, 0);
        }
    }
}
