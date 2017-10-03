using GameEngine.Assets;
using GameEngine.Drawables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GameEngine
{
    public class Game
    {
        private string _title;
        private int _width, _height, _windowWidth, _windowHeight;
        private GameWindow _window;

        public string Title { get { return _title; } set { _title = value; } }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public int WindowWidth { get { return _windowWidth; } }
        public int WindowHeight { get { return _windowHeight; } }

        public View Camera { get; private set; }
        public Input Input { get; private set; }
        public Frame CurrentFrame { get; private set; }
        public Color BackColor { get; set; } = Color.CornflowerBlue;

        public List<Frame> Frames { get; private set; }

        public Game(int width, int height)
        {
            _width = _windowWidth = width;
            _height = _windowHeight = height;
            _window = new GameWindow(_width, _height);

            Camera = new View(Vector2.Zero, 0, 1f);
            Input = new Input(_window);
            Frames = new List<Frame>();
        }

        public void Run(int ticks, double fps)
        {
            _window.Load += _window_Load;
            _window.RenderFrame += _window_RenderFrame;
            _window.UpdateFrame += _window_UpdateFrame;
            _window.Resize += _window_Resize;
            
            _window.Run(ticks, fps);
        }

        public void AddFrame(Frame frame)
        {
            if (Frames.Count == 0)
            {
                frame.Position = new PointF(0, 0);
                CurrentFrame = frame;
            }
            else
            {
                var lastFrame = Frames[Frames.Count - 1];
                frame.Position = new PointF(0, lastFrame.Position.Y + lastFrame.Size.Height);
            }
            Frames.Add(frame);
        }

        public void SetFrame(Frame frame)
        {
            CurrentFrame = frame;
        }

        private void _window_Resize(object sender, System.EventArgs e)
        {
            _windowWidth = _window.ClientRectangle.Width;
            _windowHeight = _window.ClientRectangle.Height;

            GL.Viewport(_window.ClientRectangle.X, _window.ClientRectangle.Y, _window.ClientRectangle.Width, _window.ClientRectangle.Height);
        }

        private void _window_Load(object sender, System.EventArgs e)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _window.Title = _title;
        }

        private void _window_UpdateFrame(object sender, FrameEventArgs e)
        {
            if (CurrentFrame != null)
                CurrentFrame.Tick(this);

            Camera.Update();
            Input.Update();
        }

        private void _window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(BackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Painter.Begin(Width, Height);
            Camera.Apply();

            foreach (var frame in Frames) frame.Draw();
            
            _window.SwapBuffers();
        }
    }
}
