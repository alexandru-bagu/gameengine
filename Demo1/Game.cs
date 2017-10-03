using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using System;
using GameEngine.Assets;
using Demo1.Networking.Primitives;

namespace Demo1
{
    public abstract class Game : Panel
    {
		protected static TextureAsset _sprite, _greenMallet, _blueMallet, _particleAsset;

		public static void LoadAssets()
		{
			_sprite = TextureAsset.LoadRelativePath("Sprites air hockey classic.png");
			_greenMallet = TextureAsset.LoadRelativePath("green.png");
			_blueMallet = TextureAsset.LoadRelativePath("blue.png");
			_particleAsset = TextureAsset.LoadRelativePath("particle.png");
		}

		public event Action<bool> Goal;
        
        protected virtual void InvokeGoal(bool arg)
        {
            Goal?.Invoke(arg);
        }

		public abstract void SetAI(AI ai);
		public abstract void SetOpponent(string opponent);
        public abstract void ResetPositions();
		public abstract void UpdateState(StatePrimitive data);
	}
}
