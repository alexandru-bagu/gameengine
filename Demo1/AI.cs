using GameEngine.Collision;
using OpenTK;
using System;
using GameEngine;
using System.Diagnostics;

namespace Demo1
{
	public class AI
	{
		private float _speed;
		private Polygon _mallet, _puck, _goal;
		private bool _topSide;
		private Stopwatch _tickWatch;
		public bool TopSide => _topSide;

		public AI(float speed)
		{
			_speed = speed;
			_tickWatch = Stopwatch.StartNew();
		}

		public void SetParameters(Polygon mallet, Polygon puck, Polygon goal, bool topSide)
		{
			_mallet = mallet;
			_puck = puck;
			_goal = goal;
			_topSide = topSide;
		}

        public void Tick(Vector2 position, float maxY, float center, float minX, float maxX)
        {
            if (_tickWatch.ElapsedMilliseconds < 100) return;
            _tickWatch.Restart();

            var mallet = _mallet.Center;
            var puck = _puck.Center;
            var goal = _goal.Center;

            double target = 0;
            var speed = _speed;
            if (_topSide && mallet.Y > maxY)
            {
                target = Math.Atan2(mallet.Y - goal.Y, mallet.X - goal.X);
            }
            else if (!_topSide && mallet.Y < maxY)
            {
                target = Math.Atan2(goal.Y - mallet.Y, goal.X - mallet.X);
            }
            else if (_topSide && puck.Y - ((CircleShape)_puck).Radius < mallet.Y)
            {
                target = Math.Atan2(mallet.Y - goal.Y, mallet.X - goal.X);
            }
            else if (!_topSide && puck.Y + ((CircleShape)_puck).Radius > mallet.Y)
            {
                target = Math.Atan2(goal.Y - mallet.Y, goal.X - mallet.X);
            }
            else
            {
                var pTarget = Math.Atan2(goal.X - puck.X, goal.Y - puck.Y);
                var nTarget = puck - new Vector2((float)Math.Cos(pTarget), (float)Math.Cos(pTarget)) * 2;
                target = Math.Atan2(nTarget.Y - mallet.Y, nTarget.X - mallet.X);
            }
            _mallet.Velocity = new Vector2((float)Math.Cos(target), (float)Math.Sin(target)) * speed;
            if (maxY != center)
            {
                if (_topSide && _mallet.Velocity.Y > 0) _mallet.Velocity = new Vector2(_mallet.Velocity.X, 0);
                if (!_topSide && _mallet.Velocity.Y < 0) _mallet.Velocity = new Vector2(_mallet.Velocity.X, 0);
            }
            if (position.X + _mallet.Velocity.X < minX && position.X + _mallet.Velocity.X < position.X) _mallet.Velocity = new Vector2(0, _mallet.Velocity.Y);
            if (position.X + _mallet.Velocity.X > maxX && position.X + _mallet.Velocity.X > position.X) _mallet.Velocity = new Vector2(0, _mallet.Velocity.Y);
        }
    }
}
