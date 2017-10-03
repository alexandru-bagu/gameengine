using GameEngine.Assets;
using GameEngine.Collision;
using GameEngine.Graphics.Drawables;
using OpenTK;
using GameEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using GameEngine.Graphics;
using System.Drawing;
using System.Diagnostics;
using Demo1.Networking.Primitives;
using Demo1.Networking;

namespace Demo1
{
	public class MultiPlayer : Game
	{
		private TextureAsset _tableAsset, _puckAsset, _malletAsset;
		private Panel _background;
		private Texture _puck, _mallet1, _mallet2;
		private float _widthRatio, _heightRatio, _marginX, _marginY, _centerLineY, _areaWidth, _areaHeight, _friction = 0.98f;
		private Vector2 _lastMousePosition = Vector2.Zero, _lastDelta = Vector2.Zero;
		private CollisionPlane _collisionPlane;
		private Polygon _puckPolygon, _mallet1Polygon, _mallet2Polygon, _goal1, _goal2;
		private Vector2[] _previousMousePositions;
		private Vector2 _puck1fp, _mallet1fp, _mallet2fp;
		private bool _mousePositionChanged = false;
		private GameScene _scene;
		private ParticleGenerator _particleGenerator;
		private Stopwatch _stickWatch, _puckWatch;

		public MultiPlayer(GameScene scene, int width, int height)
		{
			_stickWatch = Stopwatch.StartNew();
			_puckWatch = Stopwatch.StartNew();
			Size = new Vector2(width, height);
			_scene = scene;
			_widthRatio = width / 1013f;
			_heightRatio = height / 1595f;
			_marginX = 32 * _widthRatio;
			_marginY = 32 * _heightRatio;
			_areaWidth = width - 2 * _marginX;
			_areaHeight = height - 2 * _marginY;
			_centerLineY = height / 2 + 3 * _heightRatio - 2 * _marginY;

			_tableAsset = _sprite.Copy().Crop(1683, 458, 1013, 1595);
			_puckAsset = _sprite.Copy().Crop(2900, 785, 112, 112);
			_malletAsset = _sprite.Copy().Crop(3084, 757, 187, 187);

			_background = new Panel();
			_background.Texture = _tableAsset;
			_background.Size = new Vector2(width, height);
			Add(_background);

			float sizeFactor = 3f / 4f;

			_puck = new Texture();
			_puck.Texture = _puckAsset;
			_puck.Size = new Vector2(_widthRatio * _puckAsset.CropArea.Width * sizeFactor, _heightRatio * _puckAsset.CropArea.Height * sizeFactor);
			_background.Add(_puck);
			_background.Center(_puck);

			_mallet1 = new Texture();
			_mallet1.Texture = _blueMallet;
			_mallet1.Size = new Vector2(_widthRatio * _malletAsset.CropArea.Width * sizeFactor, _heightRatio * _malletAsset.CropArea.Height * sizeFactor);
			_background.Add(_mallet1);
			_background.CenterHorizontally(_mallet1);
			_mallet1.Position = new Vector2(_mallet1.Position.X, _areaHeight - _mallet1.Size.Y);

			_mallet2 = new Texture();
			_mallet2.Texture = _greenMallet;
			_mallet2.Size = new Vector2(_widthRatio * _malletAsset.CropArea.Width * sizeFactor, _heightRatio * _malletAsset.CropArea.Height * sizeFactor);
			_background.Add(_mallet2);
			_background.CenterHorizontally(_mallet2);
			_mallet2.Position = new Vector2(_mallet2.Position.X, _mallet2.Size.Y / 2);

			_collisionPlane = new CollisionPlane(10, 0.2f, 1f, 4, 32);

			_puckPolygon = new CircleShape(_puck, _puck.Position + _puck.Size / 2, _puck.Width / 2) { MovementSamples = 32, Static = false };
			_mallet1Polygon = new CircleShape(_mallet1, _mallet1.Position + _mallet1.Size / 2, _mallet1.Width / 2) { MovementSamples = 32, Static = true };
			_mallet2Polygon = new CircleShape(_mallet2, _mallet2.Position + _mallet2.Size / 2, _mallet2.Width / 2) { MovementSamples = 32, Static = true };
			_collisionPlane.Add(_puckPolygon, _mallet1Polygon, _mallet2Polygon);
			createMargins();

			_scene.MouseMove += scene_MouseMove;
			_previousMousePositions = new Vector2[5];

			_puck1fp = _puck.Position;
			_mallet1fp = _mallet1.Position;
			_mallet2fp = _mallet2.Position;

			_particleGenerator = new ParticleGenerator();
			Add(_particleGenerator);
			BringToFront(_particleGenerator);
		}

		public override void SetAI(AI ai)
		{
			throw new NotImplementedException();
		}

		public override void SetOpponent(string opponent)
		{

		}

		public override void ResetPositions()
		{
			_puckPolygon.Velocity = Vector2.Zero;

			_puckPolygon.Offset(_puck1fp - _puck.Position);
			_mallet1Polygon.Offset(_mallet1fp - _mallet1.Position);
			_mallet2Polygon.Offset(_mallet2fp - _mallet2.Position);
		}

		private void createMargins()
		{
			List<Texture> margins = new List<Texture>();
			Texture goal1, goal2;
			margins.Add(new Texture() { Position = new Vector2(0, 0), Size = new Vector2(329 * _widthRatio, _marginY) });
			margins.Add(goal1 = new Texture() { Position = new Vector2(329 * _widthRatio, -2), Size = new Vector2(Width - (329 * _widthRatio) * 2, 3) });
			margins.Add(new Texture() { Position = new Vector2(Width - 329 * _widthRatio, 0), Size = new Vector2(329 * _widthRatio, _marginY) });

			margins.Add(new Texture() { Position = new Vector2(0, _marginY), Size = new Vector2(_marginX, Height - 2 * _marginY) });
			margins.Add(new Texture() { Position = new Vector2(Width - _marginX, _marginY), Size = new Vector2(_marginX, Height - 2 * _marginY) });

			margins.Add(new Texture() { Position = new Vector2(0, Height - _marginY), Size = new Vector2(329 * _widthRatio, _marginY) });
			margins.Add(goal2 = new Texture() { Position = new Vector2(329 * _widthRatio, Height), Size = new Vector2(Width - (329 * _widthRatio) * 2, 3) });
			margins.Add(new Texture() { Position = new Vector2(Width - 329 * _widthRatio, Height - _marginY), Size = new Vector2(329 * _widthRatio, _marginY) });

			foreach (var polygon in margins.Select(p => new RectangleShape(p, p.Position, p.Size) { Static = true }))
			{
				if (polygon.Parent == goal1) _goal1 = polygon;
				if (polygon.Parent == goal2) _goal2 = polygon;
				_collisionPlane.Add(polygon);
			}
		}

		private void scene_MouseMove(Drawable arg1, Vector2 arg2)
		{
			if (Pick(arg2 + _scene.AbsolutePosition))
			{
				arg2 -= Position;
				if (_lastMousePosition != Vector2.Zero)
					_lastDelta = _lastMousePosition - arg2;
				_lastMousePosition = arg2;
				_mousePositionChanged = true;
			}
		}

		private Vector2 getMalletVelocity()
		{
			Vector2 vx = new Vector2(0, 0);
			foreach (var pos in _previousMousePositions) vx += pos;
			return vx / _previousMousePositions.Length;
		}

		private void processMouseMovement()
		{
			if (_scene.Starting) return;
			_previousMousePositions[_previousMousePositions.Length - 1] = _lastDelta;
			Array.Copy(_previousMousePositions, 1, _previousMousePositions, 0, _previousMousePositions.Length - 1);

			if (_mousePositionChanged)
			{
				var client = MultiplayerInstance.Instance.Client;

				_mousePositionChanged = false;
				var newPos = _lastMousePosition - _mallet1.Size / 2;
				newPos = newPos + limitMallet(newPos, _mallet1.Size / 2, false);
				var newCenter = newPos + _mallet1.Size / 2;
				_mallet1Polygon.Velocity = getMalletVelocity();
				foreach (var obj in _collisionPlane.TickMovement(_mallet1Polygon, newCenter))
				{
					if (obj == _puckPolygon)
					{
						playStickHit();
						client.Send<UpdateState>(new StatePrimitive() { Puck = true, Position = _puck.Position, Velocity = _puckPolygon.Velocity });
					}
				}
				_mallet1.Position = newCenter - _mallet1.Size / 2;
				client.Send<UpdateState>(new StatePrimitive() { Puck = false, Position = _mallet1.Position, Velocity = _mallet1Polygon.Velocity });
			}
		}

		public override void UpdateState(StatePrimitive data)
		{
			var mirrorPosition = Size - data.Position;
			if (!data.Puck)
			{
				var delta = _mallet2.Position - mirrorPosition + _mallet2.Size;
				_mallet2Polygon.Velocity = -data.Velocity;

				foreach (var p in _collisionPlane.TickMovement(_mallet2Polygon, _mallet2Polygon.Center - delta))
				{
					if (p == _puckPolygon)
					{
						playStickHit();
						var speed = p.Speed * 2;
						if (_collisionPlane.MaximumSpeed < speed)
							speed = _collisionPlane.MaximumSpeed;
						p.Speed = speed;
					}
				}
			}
			else
			{
				var delta = _puck.Position - mirrorPosition + _puck.Size;
				_puckPolygon.Offset(-delta);
				_puckPolygon.Velocity = -data.Velocity;
			}
		}

		protected override void InvokeGoal(bool arg)
		{
			if (MultiplayerInstance.Instance.Server != null)
			{
				var client = MultiplayerInstance.Instance.Client;
				client.Send<UpdateScore>(new ScorePrimitive() { GoalGreen = arg });

				base.InvokeGoal(arg);
			}
		}

		private void playStickHit()
		{
			if (_stickWatch.ElapsedMilliseconds < 100) return;
			_stickWatch.Restart();
			try
			{
				var source = Manager.Mixer.GenerateSource(Program.HockeyStickAsset);
				source.Volume = Program.EffectsVolume;
				source.Play();
			}
			catch { }
		}

		private void playPuckHit()
		{
			if (_puckWatch.ElapsedMilliseconds < 100) return;
			_puckWatch.Restart();
			try
			{
				var source = Manager.Mixer.GenerateSource(Program.HockeyPuckAsset);
				source.Volume = Program.EffectsVolume;
				source.Play();
			}
			catch { }
		}

		private void playGoal()
		{
			try
			{
				var source = Manager.Mixer.GenerateSource(Program.BuzzerAsset);
				source.Volume = Program.EffectsVolume;
				source.Play();
			}
			catch { }
		}

		private Vector2 limitMallet(Vector2 position, Vector2 size, bool topSide)
		{
			if (topSide)
				return limit(position, size, _marginX, _areaWidth - 2 * _marginX, _marginY, _centerLineY);
			else
				return limit(position, size, _marginX, _areaWidth - 2 * _marginX, _centerLineY, _areaHeight - 2 * _marginY);
		}

		private Vector2 limitPuck()
		{
			return limit(_puck.Position + _puck.Size / 2, _puck.Size, _marginX, _areaWidth + _marginX, _marginY, _areaHeight + 2 * _marginY);
		}

		private Vector2 limit(Vector2 position, Vector2 size, float left, float right, float top, float bottom)
		{
			var newPos = position;

			if (newPos.Y < top) newPos.Y = top;
			if (newPos.Y > bottom - size.Y / 2) newPos.Y = bottom - size.Y / 2;

			if (newPos.X < left) newPos.X = left;
			if (newPos.X > right - size.X / 2) newPos.X = right - size.X / 2;

			return newPos - position;
		}

		private float Range(float value, float min, float max)
		{
			if (min >= value && value <= max) return value;
			if (value <= min) return min;
			return max;
		}

		public override void Tick()
		{
			if (_scene.Starting) return;
			processMouseMovement();

			var p1Speed = _puckPolygon.Velocity.LengthSquared;
			if (p1Speed > 0)
			{
				if (p1Speed > 16)
					_puckPolygon.Velocity = new Vector2(_puckPolygon.Velocity.X * _friction, _puckPolygon.Velocity.Y * _friction);
				var prevCenter = _puckPolygon.Center - _puckPolygon.Velocity;
				foreach (var p in _collisionPlane.TickFreeMovement(_puckPolygon))
				{
					if (p == _goal1)
					{
						InvokeGoal(true);
						playGoal();
					}
					else if (p == _goal2)
					{
						InvokeGoal(false);
						playGoal();
					}
					else
					{
						playPuckHit();
						if (p != _mallet1Polygon && p != _mallet2Polygon)
						{
							_particleGenerator.Generate(
								_particleGenerator.Random(20, 30), _particleAsset,
								() => Color.FromArgb(_particleGenerator.Random(0, 255), _particleGenerator.Random(0, 255), _particleGenerator.Random(0, 255)),
								1000, 500, 255, _puckPolygon.Center, new Vector2(20, 20), 0,
								_particleGenerator.Angle(_puckPolygon.Center, prevCenter), 120, 4, 2, 0, 5);
						}
					}
				}
				_puckPolygon.Offset(limitPuck());
			}

			if (!MultiplayerInstance.Instance.IsClient)
			{
				var client = MultiplayerInstance.Instance.Client;
				client.Send<UpdateState>(new StatePrimitive() { Puck = true, Position = _puck.Position, Velocity = _puckPolygon.Velocity });
			}
			base.Tick();

		}
	}
}
