using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace GameEngine.Graphics
{
    public class View
    {
        public enum TweenType
        {
            Instant, 
            Linear, 
            QuadraticInOut,
            CubicInOut,
            QuarticOut
        }

        private Vector2 _position, _positionSrc, _positionDst;
        private int _currentStep, _tweenSteps;
        private TweenType _tweenType;
        private double _rotation, _zoom;

        public Vector2 Position { get { return _position; } }
        public Vector2 PositionDestination { get { return _positionDst; } }
        public double Rotation { get { return _rotation; } }
        public double Zoom { get { return _zoom; } set { _zoom = value; } }
        
        public View(Vector2 position, double rotation = 0, double zoom = 1)
        {
            _position = position;
            _rotation = rotation;
            _zoom = zoom;
        }

        public Vector2 ToWorld(Vector2 input)
        {
            input /= (float)_zoom;
            Vector2 dX = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
            Vector2 dY = new Vector2((float)Math.Cos(_rotation + MathHelper.PiOver2), (float)Math.Sin(_rotation + MathHelper.PiOver2));
            return _position + dX * input.X + dY * input.Y;
        }
        
        public void Update()
        {
            if (_currentStep < _tweenSteps)
            {
                _currentStep++;
                switch (_tweenType)
                {
                    case TweenType.Linear:
                        _position = _positionSrc + (_positionDst - _positionSrc) * GetLinear((float)_currentStep / _tweenSteps);
                        break;
                    case TweenType.QuadraticInOut:
                        _position = _positionSrc + (_positionDst - _positionSrc) * GetQuadraticInOut((float)_currentStep / _tweenSteps);
                        break;
                    case TweenType.CubicInOut:
                        _position = _positionSrc + (_positionDst - _positionSrc) * GetCubicInOut((float)_currentStep / _tweenSteps);
                        break;
                    case TweenType.QuarticOut:
                        _position = _positionSrc + (_positionDst - _positionSrc) * GetQuarticOut((float)_currentStep / _tweenSteps);
                        break;
                    default:
                        _position = _positionDst;
                        break;
                }
            }
            else
            {
                _position = _positionDst;
            }
        }

        public float GetLinear(float t)
        {
            return t;
        }

        public float GetQuadraticInOut(float t)
        {
            return (t * t) / ((2 * t * t) - (2 * t) + 1);
        }

        public float GetCubicInOut(float t)
        {
            return (t * t * t) / ((3 * t * t) - (3 * t) + 1);
        }

        public float GetQuarticOut(float t)
        {
            return -((t - 1) * (t - 1) * (t - 1) * (t - 1)) + 1;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            _positionSrc = position;
            _positionDst = position;
            _tweenType = TweenType.Instant;
            _tweenSteps = 0;
            _currentStep = 0;
        }

        public void SetPosition(Vector2 position, TweenType type, int steps = 30)
        {
            if (type == TweenType.Instant)
            {
                SetPosition(position);
            }
            else
            {
                _positionSrc = _position;
                _positionDst = position;
                _tweenType = type;
                _tweenSteps = steps;
                _currentStep = 0;
            }
        }
        public void Apply()
        {
            Matrix4 transform = Matrix4.Identity;

            transform = Matrix4.Mult(transform, Matrix4.CreateTranslation(-Position.X, -Position.Y, 0));
            transform = Matrix4.Mult(transform, Matrix4.CreateRotationZ(-(float)Rotation));
            transform = Matrix4.Mult(transform, Matrix4.CreateScale((float)Zoom, (float)Zoom, 1));

            GL.MultMatrix(ref transform);
        }
    }
}
