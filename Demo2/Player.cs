using GameEngine.Assets;
using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Input;
using GameEngine.Collision;
using Demo2.Networking.Primitives;

namespace Demo2
{
    public class Player : Texture
    {
        public event Action<Player, bool> Step;
        public event Action<Player, float> Shoot;
        public event Action<Player> Death, Reload;

        private TextureAsset _spriteSheet;
        private Vector2[] _sheetPosition = new Vector2[] { new Vector2(10, 18), new Vector2(92, 18), new Vector2(174, 18), new Vector2(256, 18), new Vector2(338, 18), new Vector2(415, 18), new Vector2(494, 18), new Vector2(572, 18), new Vector2(19, 115), new Vector2(92, 114), new Vector2(176, 114), new Vector2(262, 114), new Vector2(341, 114), new Vector2(417, 114), new Vector2(22, 209), new Vector2(102, 207), new Vector2(176, 207), new Vector2(261, 207), new Vector2(340, 207), new Vector2(415, 207), new Vector2(10, 298), new Vector2(92, 298), new Vector2(173, 297), new Vector2(276, 290), new Vector2(343, 290), new Vector2(413, 315), new Vector2(504, 340), new Vector2(22, 397), new Vector2(97, 396) };
        private Vector2[] _sheetSize = new Vector2[] { new Vector2(57, 65), new Vector2(59, 65), new Vector2(58, 65), new Vector2(56, 65), new Vector2(54, 65), new Vector2(57, 65), new Vector2(58, 65), new Vector2(58, 65), new Vector2(49, 67), new Vector2(55, 67), new Vector2(51, 67), new Vector2(39, 69), new Vector2(36, 68), new Vector2(39, 67), new Vector2(54, 67), new Vector2(54, 67), new Vector2(59, 66), new Vector2(54, 67), new Vector2(55, 66), new Vector2(59, 65), new Vector2(65, 65), new Vector2(66, 64), new Vector2(66, 58), new Vector2(47, 73), new Vector2(52, 72), new Vector2(81, 39), new Vector2(81, 23), new Vector2(60, 56), new Vector2(61, 56) };
        private List<TextureAsset> _idleTextures, _walkingTextures, _shootingTextures, _deathTextures;
        private TextureAsset _floatUpwards, _floatDownwards;
        private bool _facingWest, _dead;
        private int _textureIndex, _bullets, _hitpoints;
        private Stopwatch _stopwatch, _stepStopwatch, _reloadStopwatch, _shootStopwatch;
        private Vector2 _offset, _textureSize;
        private float _velocitySquared;
        private Label _nameLabel;
        private Polygon _collisionShape;

        public bool Dead => _dead;
        public Vector2 Velocity => _collisionShape.Velocity;
        public bool CanJump { get; set; } = false;
        public bool Floating { get; set; }
        public int Bullets => _bullets;
        public int Hitpoints => _hitpoints;

        public override string Name
        {
            get { return _nameLabel.Text; }
            set { _nameLabel.Text = value; }
        }

        public Polygon CollisionShape
        {
            get { return _collisionShape; }
            set { _collisionShape = value; }
        }

        public bool Enemy { get; set; }

        public Player(string name, Color color)
        {
            _bullets = 5;
            _hitpoints = 4;
            _textureIndex = 0;
            _stopwatch = Stopwatch.StartNew();
            _stepStopwatch = Stopwatch.StartNew();
            _reloadStopwatch = new Stopwatch();
            _shootStopwatch = Stopwatch.StartNew();
            createTextures();
            _nameLabel = new Label();
            _nameLabel.Text = name;
            _nameLabel.Font = FontAsset.DefaultBold;
            _nameLabel.ForeColor = color;
            _nameLabel.BackColor = Color.FromArgb(50, Color.Gray);
            
            Texture = _idleTextures[0];
            Size = new Vector2(Texture.CropWidth, Texture.CropHeight);
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();

            _nameLabel.Position = AbsolutePosition + new Vector2((Width - _nameLabel.Width) / 2, -30);
        }

        public void Die()
        {
            _dead = true;
            _textureIndex = 0;
            Death?.Invoke(this);
        }

        public void DownKey(Key key)
        {
            if (_dead) return;
            var impulseSpeed = 4f;
            var jumpSpeed = 12f;
            Vector2 newVel = new Vector2(0, 0);
            float newXComp = 0, newYComp = 0;
            if (key == Key.X) Die();
            if (CanJump && key == Key.W) { newYComp = -jumpSpeed; CanJump = false; }
            if (key == Key.A) newXComp = -impulseSpeed;
            if (key == Key.D) newXComp = impulseSpeed;
            if (Math.Abs(newXComp) > 0.0001f || Math.Abs(newYComp) > 0.0001f)
            {
                if (Math.Abs(newXComp) > 0.0001f) newVel.X = newXComp; else newVel.X = Velocity.X;
                if (Math.Abs(newYComp) > 0.0001f) newVel.Y = newYComp; else newVel.Y = Velocity.Y;
                UpdateVelocity(newVel);
            }
            if (key == Key.S)
            {
                UpdateVelocity(new Vector2(Math.Sign(Velocity.X) * 0.000001f, Velocity.Y));
            }
        }

        public void MousePress(MouseButton button, Vector2 position)
        {
            if (_bullets == 0) return;
            if (_shootStopwatch.ElapsedMilliseconds < 150) return;
            _shootStopwatch.Restart();
            _bullets--;
            var center = AbsolutePosition + Size / 2;
            var angle = (float)Math.Atan2(position.Y - center.Y, position.X - center.X);
            Shoot?.Invoke(this, angle);
            if (_bullets == 0)
            {
                _reloadStopwatch.Restart();
                Reload?.Invoke(this);
            }
        }

        public void UpdateVelocity(Vector2 velocity)
        {
            _collisionShape.Velocity = velocity;
            _velocitySquared = Velocity.LengthSquared;
        }

        private void createTextures()
        {
            _spriteSheet = TextureAsset.LoadRelativePath("hero_spritesheet.png");
            _idleTextures = new List<TextureAsset>();
            for (int i = 0; i < 8; i++)
                _idleTextures.Add(extractTexture(i));
            _walkingTextures = new List<TextureAsset>();
            for (int i = 8; i < 14; i++)
                _walkingTextures.Add(extractTexture(i));
            _shootingTextures = new List<TextureAsset>();
            for (int i = 14; i < 20; i++)
                _shootingTextures.Add(extractTexture(i));
            _floatDownwards = extractTexture(21);
            _floatUpwards = extractTexture(22);
            _deathTextures = new List<TextureAsset>();
            for (int i = 23; i < 27; i++)
                _deathTextures.Add(extractTexture(i));
        }

        public void Reset()
        {
            _hitpoints = 4;
            _bullets = 5;
            _dead = false;
            UpdateVelocity(Vector2.Zero);
        }

        private TextureAsset extractTexture(int i)
        {
            var copy = _spriteSheet.Copy();
            var pos = _sheetPosition[i];
            var size = _sheetSize[i];
            copy.Crop(pos.X, pos.Y, size.X, size.Y);
            return copy;
        }

        public override void Tick()
        {
            base.Tick();
            if (_reloadStopwatch.ElapsedMilliseconds >= 5000)
            {
                _bullets = 5;
                _reloadStopwatch.Reset();
            }
            if (_stopwatch.ElapsedMilliseconds < 100) return;
            _stopwatch.Restart();
            _facingWest = Velocity.X <= 0;
            if (_dead)
            {
                _textureIndex = Math.Min(_deathTextures.Count - 1, _textureIndex + 1);
                Texture = _deathTextures[_textureIndex];
            }
            else
            {
                if (_velocitySquared <= 2)//idle
                {
                    _textureIndex = (_textureIndex + 1) % _idleTextures.Count;
                    Texture = _idleTextures[_textureIndex];
                }
                else
                {
                    if (Floating)
                    {
                        if (Velocity.Y < 0)
                        {
                            Texture = _floatUpwards;
                        }
                        else
                        {
                            Texture = _floatDownwards;
                        }
                    }
                    else
                    {
                        _textureIndex = (_textureIndex + 1) % _walkingTextures.Count;
                        Texture = _walkingTextures[_textureIndex];
                        if (_stepStopwatch.ElapsedMilliseconds >= 250)
                        {
                            _stepStopwatch.Restart();
                            Step?.Invoke(this, _textureIndex % 2 == 0);
                        }
                    }
                }

            }
            _textureSize = new Vector2(Texture.CropWidth, Texture.CropHeight);
            _offset = (_sheetSize[0] - _textureSize) / 2 * Vector2.UnitX + Vector2.UnitY * (_sheetSize[0].Y - _textureSize.Y);
            if (Math.Abs(Velocity.X) > 0.0000001f)
                UpdateVelocity(new Vector2(Velocity.X * 0.5f, Velocity.Y));
        }

        public void SetData(PlayerState data)
        {
            var position = data.Position - Position;
            _collisionShape.Offset(position);
            UpdateVelocity(data.Velocity);
            Floating = data.Floating;
            _dead = data.Dead;
        }

        public bool ProcessHit(Vector2 position)
        {
            var pos = position - AbsolutePosition;
            var y = Math.Abs(pos.Y);
            int hits = 1;
            if (y <= Size.Y / 5) // headshot
            { hits = 4; }
            else if (y <= Size.Y / 3) // chest shot
            {  hits = 2; }
            else
            {   hits = 1; }
            hits = 1;
            _hitpoints -= hits;
            if (_hitpoints < 0) _hitpoints = 0;
            return Hitpoints == 0;
        }

        protected override void Draw()
        {
            if (Texture != null)
            {
                if (Texture.Scale.X > 0 && _facingWest) Texture.FlipX();
                if (Texture.Scale.X < 0 && !_facingWest) Texture.FlipX();
                Painter.Texture(Texture, Painter.AsRectangle(AbsolutePosition + _offset, _textureSize), Color.White);
            }
            _nameLabel.InvokeDraw();
        }
    }
}
