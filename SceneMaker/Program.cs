using SceneMaker.Scenes;
using GameEngine;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using System.IO;

namespace SceneMaker
{
    static class Program
    {
        public static int Width = 1024, Height = 736;
		public static float Ratio = Height / (float)Width;

        public static GameManager Manager;

        public static MainScene Background;

        static void Main()
        {
			Manager = new GameManager(Width, Height, "Scene Maker", false);
            Manager.Loaded += Kernel_Loaded;
			Manager.Run(60, 60);
        }

        private static void Kernel_Loaded(GameManager obj)
        {
            Manager.AutoScale = false;
            Manager.CursorVisible = true;
            Manager.BackColor = Color.PaleGreen;
			Manager.AddScene(Background = new MainScene(Width, Height));
        }
    }
}
