using System;
using Demo2.Levels;
using GameEngine.Graphics;
using OpenTK;
using OpenTK.Input;
using GameEngine.Collision;
using System.Linq;
using System.Diagnostics;
using GameEngine.Audio;
using System.Collections.Generic;
using Demo2.Controls;
using GameEngine.Assets;
using System.Drawing;
using GameEngine.Networking;
using Demo2.Networking;
using Demo2.Networking.Primitives;
using GameEngine.Networking.Primitives;
using GameEngine.Graphics.Drawables;

namespace Demo2.Scenes
{
    public class GameScene : GraphicsScene
    {
        private Level _level;
        private Vector2 _viewportSize;
        private CollisionPlane _plane;
        private GenericNetworkClient _client;
        
        private Player _player, _enemy;
        private PlayerCollision _playerObject, _enemyObject;
        private List<Polygon> _bulletPolygons;
        private Tracker _hpTracker, _bulletTracker;
        private BloodspatterGenerator _bloodSpatter;
        private bool _ended;

        public GameScene(int width, int height) : base(width, height)
        {
            _bulletPolygons = new List<Polygon>();

            PreviewKeys = true;
            PreviewKeyDown += GameScene_PreviewKeyDown;
            MouseClick += GameScene_MouseClick;

            _hpTracker = new Tracker("Hitpoints: ", TextureAsset.LoadRelativePath("heart.png"), 4, 0.5f, 100);
            _hpTracker.Position = new Vector2(10, 10);
            _bulletTracker = new Tracker("Bullets: ", Bullet.GetBulletAsset(), 5, 1f, 100);
            _bulletTracker.Position = new Vector2(10, 40);

            _bloodSpatter = new BloodspatterGenerator();
            Add(_bloodSpatter);

#if DEBUG
            _client = new UDPClient();
#endif
        }

        public void SetClient(GenericNetworkClient client)
        {
            _client = client;
        }

        private void createEnemy()
        {
            _enemy = new Player("Enemy", Color.Red);
            _enemy.Enemy = true;
            _enemy.Offset = Position;
            _enemy.Step += _player_Step;
            _enemy.Death += _player_Death;
            _enemyObject = new PlayerCollision(_enemy);
            _enemy.CollisionShape = _enemyObject;
        }

        private void createPlayer()
        {
            _player = new Player("You", Color.Green);
            _player.Enemy = false;
            _player.Offset = Position;
            _player.Step += _player_Step;
            _player.Death += _player_Death;
            _player.Shoot += _player_Shoot;
            _player.Reload += _player_Reload;
            _playerObject = new PlayerCollision(_player);
            _player.CollisionShape = _playerObject;
        }

        public void SetPlayerName(string text)
        {
            _player.Name = text;
        }

        public void SetEnemyName(string text)
        {
            _enemy.Name = text;
        }

        private void _player_Reload(Player obj)
        {
            _bulletTracker.Reload(5000);
            try
            {
                AudioSource src = Manager.Mixer.GenerateSource(AudioManager.Instance.Reload);
                src.Volume = Program.Volume;
                src.Play();
            }
            catch { }
        }

        private void _player_Shoot(Player player, float angle)
        {
            try
            {
                AudioSource src = Manager.Mixer.GenerateSource(AudioManager.Instance.Shoot);
                src.Volume = Program.Volume;
                src.Play();
            }
            catch { }
            var bulletSpeed = 10;
            var direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            direction *= bulletSpeed;
            angle *= (float)(180f / Math.PI);

            Bullet bullet = new Bullet(false);
            bullet.Offset = Position;
            bullet.Position = _player.Position + Vector2.UnitY * _player.Size / 2;
            if (direction.X < 0) bullet.Position -= Vector2.UnitX * _player.Size / 4;
            else bullet.Position += Vector2.UnitX * _player.Size;
            bullet.RotationAngle = angle;
            RectangleShape bulletShape = new BulletCollision(bullet, bullet.Position, bullet.Size);
            bulletShape.Velocity = direction;
            bulletShape.Mass = 0.01f;
            _bulletPolygons.Add(bulletShape);
            _plane.Add(bulletShape);
            _client.Send<ShootBullet>(new BulletState() { Position = bullet.Position, Velocity = direction, Angle = angle });
        }

        private void _player_Death(Player obj)
        {
            try
            {
                AudioSource src = Manager.Mixer.GenerateSource(AudioManager.Instance.Death);
                src.Volume = Program.Volume;
                src.Seek(0.3f);
                src.Play();
            }
            catch { }

            _client.Send<GameEnd>(new EmptyPrimitive());
            EndGame();
        }

        private void _player_Step(Player obj, bool left)
        {
            try
            {
                AudioSource src;
                if (left) src = Manager.Mixer.GenerateSource(AudioManager.Instance.StepL);
                else src = Manager.Mixer.GenerateSource(AudioManager.Instance.StepR);
                src.Volume = Program.Volume;
                src.Play();
            }
            catch { }
        }

        private void GameScene_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
        {
            if (arg3 == MouseButton.Middle) NewRound(false);
            _player.MousePress(arg3, arg2 + Manager.Camera.Position);
        }

        private bool GameScene_PreviewKeyDown(GraphicsScene source, Key key)
        {
            _player.DownKey(key);
            return false;
        }

        public void Load()
        {
            createPlayer();
            createEnemy();

            _viewportSize = new Vector2(Manager.Width, Manager.Height);
            _level = new MagicalCliffs();
            _level.Build(Width, Height, _viewportSize, Position);
            _plane = new CollisionPlane(4, 1f, 0, new Vector2(0, 0.4f));
            _plane.Add(_level.PlatformBlocks.Select(p =>
            {
                var parent = new Texture();
                parent.Position = new Vector2(p.X, p.Y);
                parent.Size = new Vector2(p.Width, p.Height);
                parent.Offset = Position;

                var shape = new RectangleShape(parent, p.X - Position.X, p.Y - Position.Y, p.Width, p.Height) { Static = true };
                return shape;
            }));
            _plane.Add(_playerObject);
            _plane.Add(_enemyObject);

#if DEBUG
            NewRound(false);
#endif
        }

        public void NewRound(bool isEnemy)
        {
            _ended = false;
            if (!isEnemy)
            {
                _playerObject.Offset(new Vector2(Width * 1 / 7, Height / 4) - _player.Position);
                _enemyObject.Offset(new Vector2(Width * 6 / 7, Height / 4) - _enemy.Position);
            }
            else
            {
                _playerObject.Offset(new Vector2(Width * 6 / 7, Height / 4) - _player.Position);
                _enemyObject.Offset(new Vector2(Width * 1 / 7, Height / 4) - _enemy.Position);
            }

            _player.Reset();
            _enemy.Reset();

            foreach (var bullet in _bulletPolygons)
            {
                _plane.Remove(bullet);
                Remove(bullet.Parent as Drawable);
            }
            _bulletPolygons.Clear();
        }

        public override void DrawObjects()
        {
            if (_level != null) _level.Draw();
            if (_player != null) _player.InvokeDraw();
            if (_enemy != null) _enemy.InvokeDraw();
            foreach (var bullet in _bulletPolygons) ((Drawable)bullet.Parent).InvokeDraw();

            _hpTracker.Position = new Vector2(10, 10) + Manager.Camera.Position;
            _hpTracker.InvokeDraw();

            _bulletTracker.Position = new Vector2(10, 40) + Manager.Camera.Position;
            _bulletTracker.InvokeDraw();

            base.DrawObjects();
        }

        public override void Tick()
        {
            base.Tick();

            _hpTracker.Items = _player.Hitpoints;
            _bulletTracker.Items = _player.Bullets;

            if (_level != null) _level.Tick(Manager.Camera.Position, _viewportSize);
            if (_player != null) _player.Tick();
            if (_enemy != null) _enemy.Tick();
            tickPlayerMovement();
            tickEnemyMovement();
            tickBulletMovement();
            _plane.TickGravity();
            var destination = Position + new Vector2(Math.Max(Math.Min(Width - Manager.Width, _player.Position.X - Manager.Width / 2), 0), 0);
            if (!_ended)
            {
                if (Manager.Camera.PositionDestination != destination)
                    Manager.Camera.SetPosition(destination, View.TweenType.Linear, 4);
            }
            _client.Send<UpdatePlayer>(new PlayerState(_player));
        }

        public void UpdateEnemy(PlayerState data)
        {
            _enemy.SetData(data);
        }

private void tickBulletMovement()
{
	List<Polygon> remove = new List<Polygon>();
	foreach (var bullet in _bulletPolygons)
	{
		bool hit = false;
		var pBullet = bullet.Parent as Bullet;
		var pPosition = bullet.Center + bullet.Velocity;
		foreach (var obj in _plane.TickFreeMovement(bullet))
		{
			if (obj != _playerObject)
			{
				if (!pBullet.Enemy && obj == _enemyObject)
				{
					_bloodSpatter.Generate(bullet.Center, (float)Math.Atan2(pPosition.Y - bullet.Center.Y, pPosition.X - bullet.Center.X));
					if (_enemy.ProcessHit(pBullet.Position + Position)) _enemy.Die();
				}
				hit = true;
			}
			else
			{
				if (pBullet.Enemy && obj == _playerObject)
				{
					_bloodSpatter.Generate(bullet.Center, (float)Math.Atan2(pPosition.Y - bullet.Center.Y, pPosition.X - bullet.Center.X));
					if (_player.ProcessHit(pBullet.Position + Position)) _player.Die();
					hit = true;
				}
			}
		}

		if (!ClientArea.IntersectsWith(pBullet.ClientArea) || hit)
		{
			remove.Add(bullet);
		}
	}
	foreach (var bullet in remove)
	{
		_bulletPolygons.Remove(bullet);
		_plane.Remove(bullet);
	}
}

        private void tickEnemyMovement()
        {
            foreach (var polygon in _plane.TickFreeMovement(_enemyObject))
            {
                var feet = _enemy.Position + _enemy.Size - Vector2.UnitX * _enemy.Size.X / 2;
                if (polygon.Parent.Position.Y > feet.Y)
                {
                    _enemy.UpdateVelocity(_enemy.Velocity * Vector2.UnitX);
                }
                else
                {
                    if (_enemyObject.Velocity.Y < 0)
                    {
                        _enemy.UpdateVelocity(_enemy.Velocity * Vector2.UnitX);
                    }
                }
            }
            _enemy.Floating = _plane.TestMovement(_enemyObject, _plane.Gravity * 4).Count() == 0;
            if (_enemy.Position.Y > Height && !_enemy.Dead) _enemy.Die();
        }

        private void tickPlayerMovement()
		{
            foreach (var polygon in _plane.TickFreeMovement(_playerObject))
            {
				var feet = _player.AbsolutePosition + _player.Size - Vector2.UnitX * _player.Size.X / 2;
				if (polygon.Parent.Position.Y > feet.Y)
				{
					_player.CanJump = true;
					_player.UpdateVelocity(_player.Velocity * Vector2.UnitX);
				}
				else if (Math.Abs(polygon.Parent.Position.Y + polygon.Parent.Size.Y -_player.AbsolutePosition.Y) < 5)
				{
					_player.UpdateVelocity(_player.Velocity * Vector2.UnitX);
				}
            }
            _player.Floating = _plane.TestMovement(_playerObject, _plane.Gravity * 4).Count() == 0;
            if (_player.Floating && _player.CanJump) _player.CanJump = false;
            if (_player.Position.Y > Height && !_player.Dead) _player.Die();
        }

        public void AddBullet(BulletState data)
        {
            try
            {
                AudioSource src = Manager.Mixer.GenerateSource(AudioManager.Instance.Shoot);
                src.Volume = Program.Volume;
                src.Play();
            }
            catch { }

            Bullet bullet = new Bullet(true);
            bullet.Offset = Position;
            bullet.Position = data.Position;
            bullet.RotationAngle = data.Angle;
            RectangleShape bulletShape = new BulletCollision(bullet, bullet.Position, bullet.Size);
            bulletShape.Velocity = data.Velocity;
            bulletShape.Mass = 0.01f;
            _bulletPolygons.Add(bulletShape);
            _plane.Add(bulletShape);
        }

        public void EndGame()
        {
            _ended = true;
            Program.Menu.SetView(Manager);
            Program.Menu.MoveCamera(Manager, View.TweenType.CubicInOut);

            _client.Disconnect();
            Manager.Unregister(_client);
            Multiplayer.Dispose();
        }
    }
}
