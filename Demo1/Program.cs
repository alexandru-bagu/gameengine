using GameEngine;
using GameEngine.Graphics;
using GameEngine.Assets;
using GameEngine.Audio;
using System;
using System.Collections.Generic;
using GameEngine.Threading;
using OpenTK.Graphics.OpenGL;

namespace Demo1
{
    static class Program
    {
        public const int Width = 400, Height = (int)((Width / 0.6351097178683386f) * 1.04f);
		public static List<GraphicsScene> AlphaSharedScenes;
        public static GraphicsScene MainScene, SinglePlayerScene, MultiplayerScene, SettingsScene, PauseScene, GameScene;
        public static AudioAsset BackgroundMusicAsset, HockeyPuckAsset, HockeyStickAsset, BuzzerAsset;
        public static AudioSource BackgroundMusic;
        public static Settings AppSettings;
		public static GameManager Manager;

        public static float MusicVolume
        {
            get { return AppSettings.Get("music_volume", 1f); }
            set { AppSettings.Set("music_volume", value); }
        }

        public static float EffectsVolume
        {
            get { return AppSettings.Get("effects_volume", 1f); }
            set { AppSettings.Set("effects_volume", value); }
        }

		static void Main()
		{
            GameEngine.Networking.Reflection.Reflector.GetPair(typeof(float));

            AppSettings = new Settings("settings.cfg");

			Manager = new GameManager(Width, Height, "Air Hockey", false);
			Fonts.Init((float)Width / 500);
			Manager.AddScene(MainScene = new MainMenu(Width, Height));
			Manager.AddScene(SinglePlayerScene = new SinglePlayerMenu(Width, Height));
			Manager.AddScene(GameScene = new GameScene(Width, Height));
			Manager.AddScene(PauseScene = new PauseMenu(Width, Height));
			Manager.AddScene(SettingsScene = new SettingsMenu(Width, Height));
			Manager.AddScene(MultiplayerScene = new MultiplayerMenu(Width, Height));

			AlphaSharedScenes = new List<GraphicsScene>();
			AlphaSharedScenes.AddRange(new[] { MainScene, SinglePlayerScene, PauseScene, SettingsScene, MultiplayerScene });

			Manager.Loaded += Manager_Loaded;
			Manager.Prerender += Manager_Prerender;
			Manager.Register(new BackgroundTick(Manager));
			Manager.Run(60, 60);
		}

		private static void Manager_Prerender(GameManager obj)
		{
			var c = obj.BackColor;
			if (obj.CurrentScene != GameScene)
			{
				var c1 = ((HSLColor)c).AddLighting(-0.2f);//.AddSaturation(-0.5f);
				var c2 = ((HSLColor)c).AddLighting(-0.4f);//.AddSaturation(0);
				GL.Begin(PrimitiveType.Quads);
				GL.Color3(c1);
				GL.Vertex2(0, 0);
				GL.Color3(c1);
				GL.Vertex2(Width, 0);
				GL.Color3(c2);
				GL.Vertex2(Width, Height);
				GL.Color3(c2);
				GL.Vertex2(0, Height);
				GL.End();
			}
		}

		public class BackgroundTick : ITickable
		{
			private GameManager _manager;
			private HSLColor _color;

			public BackgroundTick(GameManager manager)
			{
				_manager = manager;
				_color = manager.BackColor;
			}

			public void Tick(IThreadContext context)
			{
				_color = _color.AddHue(0.001f);
				_manager.BackColor = _color;
			}
		}

		private static void Manager_Loaded(GameManager obj)
		{
			Game.LoadAssets();

			BackgroundMusicAsset = AudioAsset.LoadRelativePath("Happy-electronic-music.wav");
			HockeyPuckAsset = AudioAsset.LoadRelativePath("Hockey Puck.wav");
			HockeyStickAsset = AudioAsset.LoadRelativePath("Hockey Stick.wav");
			BuzzerAsset = AudioAsset.LoadRelativePath("buzzer.wav");

			BackgroundMusic = obj.Mixer.GenerateSource(BackgroundMusicAsset);
			BackgroundMusic.Loop = true;
			BackgroundMusic.Volume = MusicVolume;
			BackgroundMusic.Play();
		}

        public static void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
            BackgroundMusic.Volume = MusicVolume;
        }

        public static void SetEffectsVolume(float volume)
        {
            EffectsVolume = volume;
        }
    }
}
