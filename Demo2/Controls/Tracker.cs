using GameEngine.Assets;
using GameEngine.Graphics.Drawables;
using OpenTK;
using GameEngine.Graphics;
using System.Diagnostics;
using System.Drawing;

namespace Demo2.Controls
{
    public class Tracker : Container
    {
        private Label _text;
        private ProgressBar _reloadBar;
        private TextureAsset _itemTexture;
        private int _alignWidth, _maxItems, _reloadTime;
        private Stopwatch _reloadStopwatch;

        public Label TextLabel => _text;
        public TextureAsset ItemTexture => _itemTexture;
        public int Items { get; set; }
        public new float Scale { get; set; } = 1f;

        public Tracker(string text, TextureAsset itemTexture, int max, float scale = 1f, int alignWidth = 0)
        {
            Scale = scale;
            _itemTexture = itemTexture;
            _maxItems = max;
            _alignWidth = alignWidth;
            _reloadTime = 1;
            _reloadStopwatch = new Stopwatch();

            _text = new Label();
            _text.Text = text;
            _reloadBar = new ProgressBar();
            _reloadBar.Size = new Vector2(max * _itemTexture.CropSize.X * scale, _text.Height / 2);
            _reloadBar.Visible = false;
            _reloadBar.BackColor = Color.Transparent;
            _reloadBar.ShowText = false;
            _reloadBar.BarColor = Color.FromArgb(50, Color.Gray);
            _reloadBar.BorderWidth = 0;
            Add(_text);
            Add(_reloadBar);
            
            var position = _text.Position;
            if (_alignWidth == 0) position += _text.Size * Vector2.UnitX;
            else position += _alignWidth * Vector2.UnitX;
            var size = _itemTexture.CropSize * Scale;
            position += _text.CenterVertically(Vector2.UnitY * _reloadBar.Size);
            _reloadBar.Position = position;
        }

        public void Reload(int totalTime)
        {
            _reloadTime = totalTime;
            _reloadStopwatch.Restart();
        }

        public override void DrawObjects()
        {
            _text.InvokeDraw();
            var position = _text.AbsolutePosition;
            if (_alignWidth == 0) position += _text.Size * Vector2.UnitX;
            else position += _alignWidth * Vector2.UnitX;
            var size = _itemTexture.CropSize * Scale;
            position += _text.CenterVertically(Vector2.UnitY * size);

            if (Items == 0)
            {
                _reloadBar.Visible = true;
                _reloadBar.Progress = _reloadStopwatch.ElapsedMilliseconds / (float)_reloadTime * 100f;
                _reloadBar.InvokeDraw();
            }
            else
            {
                _reloadBar.Visible = false;
                for (int i = 0; i < Items; i++)
                {
                    var rect = Painter.AsRectangle(position, size);
                    Painter.Texture(_itemTexture, rect);
                    position += Vector2.UnitX * size + Vector2.UnitX * 2;
                }
            }
        }
    }
}