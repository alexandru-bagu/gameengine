using System;
using GameEngine;
using GameEngine.Graphics;
using OpenTK.Graphics.OpenGL;
using GameEngine.Threading;

namespace Demo3
{
    class Program
    {
        static void Main(string[] args)
        {
            GameManager manager = new GameManager(400, 600, "Demo3", false);
            manager.AddScene(new GameScene(400, 600));
            manager.Run(60, 60);
        }
    }
}
