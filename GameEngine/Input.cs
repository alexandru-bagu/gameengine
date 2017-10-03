using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GameEngine
{
    public class Input
    {
        private HashSet<Key> _keysDown, _keysDownLast;
        private HashSet<MouseButton> _mouseDown, _mouseDownLast;
        private int _capsLock, _alt, _control;

        public Key[] KeyPresses => _keysDown.Where(p => !_keysDownLast.Contains(p)).ToArray();
        public Key[] KeysUp => _keysDownLast.Where(p => !_keysDown.Contains(p)).ToArray();
        public Key[] KeysDown => _keysDown.ToArray();

        public bool CapsLock => _capsLock > 0;
        public bool Alt => _alt > 0;
        public bool Control => _control > 0;

        public Input()
        {
            Clear();
        }

        private void Clear()
        {
            _keysDown = new HashSet<Key>();
            _keysDownLast = new HashSet<Key>();

            _mouseDown = new HashSet<MouseButton>();
            _mouseDownLast = new HashSet<MouseButton>();
        }

        public void SetWindow(IGameWindow game)
        {
            game.KeyUp += _window_KeyUp;
			game.KeyDown += _window_KeyDown;
			game.MouseUp += _window_MouseUp;
			game.MouseDown += _window_MouseDown;
		}

        public void ClearKey(Key key)
        {
            _keysDown.Remove(key);
        }

        private void _window_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.CapsLock) _capsLock ^= 1;
            if (e.Key == Key.ControlLeft || e.Key == Key.ControlRight) _control++;
            if (e.Key == Key.AltLeft || e.Key == Key.AltRight) _alt++;

            _keysDown.Add(e.Key);
        }

        private void _window_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.ControlLeft || e.Key == Key.ControlRight) _control--;
            if (e.Key == Key.AltLeft || e.Key == Key.AltRight) _alt--;

            _keysDown.Remove(e.Key);
        }

        private void _window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown.Add(e.Button);
        }

        private void _window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseDownLast.Contains(e.Button)) _mouseDownLast.Add(e.Button);
            _mouseDown.Remove(e.Button);
        }

        public void Update()
        {
            _keysDownLast = new HashSet<Key>(_keysDown);
            _mouseDownLast = new HashSet<MouseButton>(_mouseDown);
        }

        public bool KeyPress(Key key)
        {
            return _keysDown.Contains(key) && !_keysDownLast.Contains(key);
        }

        public bool KeyRelease(Key key)
        {
            return !_keysDown.Contains(key) && _keysDownLast.Contains(key);
        }

        public bool KeyDown(Key key)
        {
            return _keysDown.Contains(key);
        }

        public bool MousePress(MouseButton button)
        {
            return _mouseDown.Contains(button) && !_mouseDownLast.Contains(button);
        }

        public bool MouseRelease(MouseButton button)
        {
            return !_mouseDown.Contains(button) && _mouseDownLast.Contains(button);
        }

        public bool MouseDown(MouseButton button)
        {
            return _mouseDown.Contains(button);
        }

        internal bool HasMouseInput()
        {
            return _mouseDown.Count > 0 || _mouseDownLast.Count > 0;
        }
    }
}
