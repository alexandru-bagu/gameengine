using GameEngine;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            GameManager manager = new GameManager(500, 500, "", false);
            manager.AddScene(new Scene(500, 500));
            manager.Run(60, 60);
        }
    }
}
