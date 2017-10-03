using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Collections.Generic;
using System.Drawing;
using System;
using OpenTK.Graphics;
using OpenTK.Input;
using GameEngine.Graphics;
using GameEngine.Audio;
using GameEngine.Threading;
using System.Threading.Tasks;

namespace GameEngine
{
	public class GameManager : Context<ITickable>
    {
        private int _width, _height;
		private int _updatesPerSecond, _framesPerSecond;
        private float _ticksPerSecond;
        private GameWindow _window;
        private AudioMixer _mixer;

		private Vector2 _aspectRatio = new Vector2(1, 1);
        private bool _justResized;
		private Vector3 _translation;

        public int Width => _width;
		public int Height => _height;
		public string Title { get; set; } = "Game Window";
        public float TicksPerSecond => _ticksPerSecond;

        public MouseDevice Mouse => _window.Mouse;
        public AudioMixer Mixer => _mixer;
		public Vector3 Translation => _translation;
        public GameWindow Window => _window;
        public Vector2 AspectRatio => _aspectRatio;

        public Scissor Scissors { get; set; }
        public View Camera { get; private set; }
        public Input Input { get; private set; }
        public GraphicsScene CurrentScene { get; private set; }
        public Color BackColor { get; set; } = Color.CornflowerBlue;
        public bool AutoScale { get; set; } = true;
		public bool CursorVisible { get { return _window.CursorVisible; } set { _window.CursorVisible = value; } }
        public List<GraphicsScene> Scenes { get; private set; }

		public event Action<GameManager> Loaded, Prerender;

        public GameManager(int width, int height, string title, bool fullscreen)
        {
            //initialize Parallel
            Parallel.Invoke(() => { });

            _mixer = new AudioMixer();
			_translation = new Vector3(-0.25f, -0.25f, 0);
            Title = title;

            Camera = new View(Vector2.Zero, 0, 1f);
            Scenes = new List<GraphicsScene>();
            Input = new Input();
            Scissors = new Scissor(this);

            Resize(width, height, fullscreen);
        }

		public void Resize(int width, int height, bool fullscreen)
		{
			_width = width;
			_height = height;

			GraphicsMode mode = new GraphicsMode();
			var flag = GameWindowFlags.FixedWindow;
			if (fullscreen) flag = GameWindowFlags.Fullscreen;
            _justResized = true;
            var newWindow = new GameWindow(width, height, mode, Title, flag);
			if (_window != null) _window.Close();

			_window = newWindow;
			_aspectRatio = new Vector2((float)_window.Width / _width, (float)_window.Height / _height);
			Input.SetWindow(_window);

            _window.Load += _window_Load;
			_window.RenderFrame += _window_RenderFrame;
			_window.UpdateFrame += _window_UpdateFrame; 
		}

        public void Run(int updatesPerSecond, int framesPerSecond)
        {
            Loaded?.Invoke(this);
            
            _updatesPerSecond = updatesPerSecond;
            _framesPerSecond = framesPerSecond;
            _ticksPerSecond = 1000f / _updatesPerSecond;
            while (_justResized)
            {
                _justResized = false;
                _window.Run(_updatesPerSecond, _framesPerSecond);
            }
        }

        public void AddScene(GraphicsScene scene, int xPadding = 100, int yPadding = 100)
        {
            scene.Manager = this;
            if (Scenes.Count == 0)
            {
                scene.Position = new Vector2(0, 0);
                CurrentScene = scene;
                scene.Unpause();
            }
            else
            {
                var lastFrame = Scenes[Scenes.Count - 1];
                scene.Position = new Vector2(xPadding, lastFrame.Position.Y + lastFrame.Size.Y + yPadding);
            }
            Scenes.Add(scene);
        }

		public void SetScene(GraphicsScene frame)
		{
			if (CurrentScene != null && CurrentScene != frame)
				CurrentScene.InvokeLostFocus();
			CurrentScene = frame;
			frame.InvokeGotFocus();
		}

        private void _window_Load(object sender, System.EventArgs e)
        {
            GL.Viewport(_window.ClientRectangle.X, _window.ClientRectangle.Y, _window.ClientRectangle.Width, _window.ClientRectangle.Height);

            GL.ClearColor(BackColor);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void _window_UpdateFrame(object sender, FrameEventArgs e)
        {
			Tick(this);

            processInput();

            foreach (var scene in Scenes)
                if(!scene.Paused)
                    scene.Tick();

            Camera.Update();
            Input.Update();
        }

        private void processInput()
        {
			var mouse = _window.Mouse;
            if (CurrentScene != null)
            {
                CurrentScene.ProcessMouseInput(mouse, Input, _aspectRatio);

                foreach (var key in Input.KeysDown)
                    CurrentScene.OnKeyDown(key);
                foreach (var key in Input.KeysUp)
                    CurrentScene.OnKeyUp(key);
                foreach (var key in Input.KeyPresses)
                    CurrentScene.OnKeyPress(key);
            }
        }

		private void _window_RenderFrame(object sender, FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Painter.Begin(Width, Height, Translation);

			Camera.Apply();

			GL.Translate(Camera.Position.X, Camera.Position.Y, 0);
			Prerender?.Invoke(this);
			GL.Translate(-Camera.Position.X, -Camera.Position.Y, 0);

			Scissors.Clip(Camera.Position, Width, Height);
			foreach (var scene in Scenes) scene.InvokeDraw();
			Scissors.Unclip();

			_window.SwapBuffers();
		}
    }
}
