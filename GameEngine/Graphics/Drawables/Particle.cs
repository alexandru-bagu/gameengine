using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK;

namespace GameEngine.Graphics.Drawables
{
	public class Particle : Texture
	{
		private int _ttl;
		private Stopwatch _watch;
		private float _maxOpacity, _currentOpacity, _opacityStep;
		private Vector2 _velocity;
		private bool _scaling, _rotating;
		private float _scaleStep, _rotationStep;

		public int TimeToLive => _ttl;
		public bool Expired => _watch.ElapsedMilliseconds > TimeToLive;
		public int Opacity => (int)_currentOpacity;

		public Particle(int timeToLive, int opacity, Vector2 velocity)
		{
			_scaling = _rotating = false;
			_ttl = timeToLive;
			_currentOpacity = opacity;
			_maxOpacity = opacity;
			_opacityStep = _currentOpacity / _ttl;
			_watch = Stopwatch.StartNew();
			_velocity = velocity;
		}

		public override void Tick()
		{
			if (Math.Abs(_currentOpacity - _watch.ElapsedMilliseconds * _opacityStep) > 0.0001f)
			{
				_currentOpacity = _watch.ElapsedMilliseconds * _opacityStep;
				var opacity = _maxOpacity - _currentOpacity;
				if (opacity < 0) opacity = 0;
				var c = BackColor;
				BackColor = Color.FromArgb((int)opacity, c.R, c.G, c.B);
			}
			if (_rotating && Math.Abs(RotationAngle - _watch.ElapsedMilliseconds * _rotationStep) > 0.0001f)
			{
				RotationAngle = _watch.ElapsedMilliseconds * _rotationStep;
			}
			if (_scaling && Math.Abs(Scale.X - _watch.ElapsedMilliseconds * _scaleStep) > 0.0001f)
			{
				var scale = _watch.ElapsedMilliseconds * _scaleStep;
				Scale = new Vector3(scale, scale, 1f);
			}
			Position += _velocity;
			base.Tick();
		}

		public void EnableRotation(float rotationMin, float rotationMax)
		{
			_rotating = true;
			RotationAngle = rotationMin;
			_rotationStep = (rotationMax - rotationMin) / _ttl;
		}

		public void EnableScaling(float scaleMin, float scaleMax)
		{
			_scaling = true;
			Scale = new Vector3(scaleMin, scaleMin, 1f);
			_scaleStep = (scaleMax - scaleMin) / _ttl;
		}
	}
}
