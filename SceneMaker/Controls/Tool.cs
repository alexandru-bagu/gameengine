using System;
using GameEngine;
using OpenTK;
using GameEngine.Graphics.Drawables;
using GameEngine.Graphics;
using System.Drawing;
using GameEngine.Assets;
using System.Collections.Generic;
using System.Globalization;

namespace SceneMaker.Controls
{
    public class Tool : Drawable
    {
        private Panel _background;
        private Label _label;
        private Func<string, Drawable> _generator;

        public Tool(string text, Func<string, Drawable> generator)
        {
            _generator = generator;

            _label = new Label();
            _label.Text = text;
            _label.BackColor = Color.Transparent;
            _label.ForeColor = Color.Black;
            _label.Font = FontAsset.Default;
            _background = new Panel();
            _background.BackColor = Color.FromArgb(100, Color.DarkGray);

            MouseEnter += Tool_MouseEnter;
            MouseLeave += Tool_MouseLeave;
        }

        // Convert the string to camel case.
        public string ToCamelCase(string value)
        {
            // If there are 0 or 1 characters, just return the string.
            if (value == null || value.Length < 2)
                return value;

            // Split the string into words.
            string[] words = value.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }

        public Drawable Generate(ICollection<string> usedNames)
        {
            var genericName = ToCamelCase(_label.Text);
            var index = 1;
            while (usedNames.Contains(genericName + index)) index++;
            usedNames.Add(genericName + index);
            return _generator(genericName + index);
        }

        private void Tool_MouseLeave(Drawable arg1, Vector2 arg2)
        {
            _background.BackColor = Color.FromArgb(100, Color.DarkGray);
        }

        private void Tool_MouseEnter(Drawable arg1, Vector2 arg2)
        {
            _background.BackColor = Color.FromArgb(200, Color.DarkGray);
        }
        
        public override Vector2 Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                _label.Position = new Vector2(Width / 5, (Size.Y - _label.Size.Y) / 2) + AbsolutePosition;
                _background.Position = AbsolutePosition;
                _background.Size = value;
            }
        }

        protected override void Draw()
        {
            _background.InvokeDraw();
            _label.InvokeDraw();
        }

        public override void Tick()
        {
        }
    }
}
