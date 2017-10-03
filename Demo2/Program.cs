using System;
using Demo2.Scenes;
using GameEngine;
using GameEngine.Assets;

namespace Demo2
{
    class Program
    {
        public const int Width = 800, Height = (int)(Width * .6);
        public const int GameSceneWidth = 1600;
        public static GameManager Manager;
        public static GameScene Game;
        public static MenuScene Menu;
        public static LoadingScene Loading;
        public static Settings AppSettings;

		public static string PlayerName;

		public static string SettingsPlayerName
		{
			get { return AppSettings.Get("player_name", "None"); }
			set { AppSettings.Set("player_name", value); }
		}

        public static float Volume
        {
            get { return AppSettings.Get("volume", 1f); }
            set { AppSettings.Set("volume", value); }
        }

        public static void Main(string[] args)
        {
            AppSettings = new Settings("settings.cfg");
			PlayerName = SettingsPlayerName;
            Manager = new GameManager(Width, Height, "Demo2", false);

            Manager.AddScene(Game = new GameScene(GameSceneWidth, Height), 0, 0);
            Manager.AddScene(Loading = new LoadingScene(Width, Height), 0, 0);
			Manager.AddScene(Menu = new MenuScene(Width, Height), 0, 0);
            Manager.Loaded += ManagerLoaded;
            Manager.Run(60, 60);
        }

        public static void ManagerLoaded(GameManager manager)
        {
            Multiplayer.Preinit(manager);
            AudioManager.Instance.Load();
            Game.Load();

			Menu.SetView(Manager); Menu.MoveCamera(Manager);
        }
    }
}
