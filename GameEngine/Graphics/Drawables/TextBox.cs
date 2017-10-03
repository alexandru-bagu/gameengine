using System;
using OpenTK;
using GameEngine.Assets;
using System.Drawing;
using OpenTK.Input;
using GameEngine.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GameEngine.Graphics.Drawables
{
    public class TextBox : Drawable
    {
        private string _text = "";
        private bool _printCaret, _caretAtNewLine;
        private Stopwatch _caretWatch, _controlInputWatch, _inputWatch;
        private Vector2 _caretPositionTop, _caretPositionBottom;
        private int _caretIndex, _selectionDirection;
        private FontAsset _font;
        private StringFormatFlags _format = StringFormatFlags.DisplayFormatControl;
        private bool _multiLine = false;
        
        public Color BackColor { get; set; } = Color.White;
        public Color ForeColor { get; set; } = Color.Black;
        public Color SelectionColor { get; set; } = Color.FromArgb(200, Color.LightBlue);
        public bool Enabled { get; set; } = true;
        public override bool Focusable => true;

        private List<GlyphLocation> _selection;
        private GlyphLocation[] _glyphs;

        public FontAsset Font
        {
            get { return _font; }
            set
            {
                _font = value; updateGlyphs(_text);
                _caretPositionBottom = _caretPositionTop + new Vector2(0, Font.LineHeight);
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value == null) throw new Exception("Text cannot be null.");
                _text = value;
                updateGlyphs(value);
            }
        }

        public StringFormatFlags Format
        {
            get { return _format; }
            set { _format = value; updateGlyphs(_text); }
        }


        public TextBox()
        {
			Padding = new Vector4(3, 3, 3, 3);
            Draggable = true;

            _font = FontAsset.Default;

            _caretPositionTop = new Vector2(0, 0);
            _caretPositionBottom = new Vector2(0, 0);
            _selection = new List<GlyphLocation>();

            _caretWatch = Stopwatch.StartNew();
            _controlInputWatch = Stopwatch.StartNew();
            _inputWatch = Stopwatch.StartNew();

            Drag += TextBox_Drag;
            MouseDown += TextBox_MouseClick;
        }

		protected override void UpdateLayout()
		{
			base.UpdateLayout(); 
			updateGlyphs(_text);
		}

        private void TextBox_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
        {
            if (!Enabled) return;
            _selection.Clear();
            _selectionDirection = 0;

            if (_glyphs.Length > 0)
            {
                var closestBeginGlyph = _glyphs.SelectMin(p => p.Distance(arg2 + AbsolutePosition));

                _selectionDirection = -1;
				setCaretPosition(closestBeginGlyph, closestBeginGlyph.Distance(arg2 + AbsolutePosition) > closestBeginGlyph.Width / 2);
            }
        }

        private void updateGlyphs(string value)
        {
            if (_font == null) return;
            _glyphs = _font.ProcessString(value, ClientArea, _format);

			var offset = PaddedAbsolutePosition;
			foreach (var glyph in _glyphs)
				glyph.Offset(offset);
        }

        private void TextBox_Drag(Drawable arg1, DragInformation info, Vector2 current)
        {
            if (!Enabled) return;
            _selection.Clear();
            _selectionDirection = 0;
            bool _caretPositionBegin = false;

            if (_glyphs.Length > 0)
            {
				var closestBeginGlyph = _glyphs.SelectMin(p => p.Distance(info.RelativeMouseStart + AbsolutePosition));
                var closestEndGlyph = _glyphs.SelectMin(p => p.Distance(current + AbsolutePosition));

                if (closestBeginGlyph.ArrayIndex > closestEndGlyph.ArrayIndex)
                {
                    Helper.Swap(ref closestEndGlyph, ref closestBeginGlyph);
                    _caretPositionBegin = true;
                }

                bool found = false;
                for (int i = 0; i < _glyphs.Length; i++)
                {
                    var g = _glyphs[i];
                    if (closestBeginGlyph == g) found = true;
                    if (found) _selection.Add(g);
                    if (g == closestEndGlyph) break;
                }
                if (_caretPositionBegin)
                {
                    _selectionDirection = -1;
                    setCaretPosition(closestBeginGlyph);
                }
                else
                {
                    _selectionDirection = 1;
                    setCaretPosition(closestEndGlyph, true);
                }
                if (_selection.Count == 1)
                {
                    if (closestBeginGlyph.Width / 4 > current.X - info.RelativeMouseStart.X)
                        _selection.Clear();
                }
            }
        }

        private void setCaretPosition(GlyphLocation glyph, bool next = false)
        {
            _caretAtNewLine = false;
            if (next)
            {
                setCaretPosition(glyph.ArrayIndex + 1, false);
            }
            else
            {
                _caretIndex = glyph.ArrayIndex;
                if (glyph.ArrayIndex > 0 && _text[glyph.ArrayIndex] == '\n')
                {
                    _caretAtNewLine = true;
                    var g = _glyphs[glyph.ArrayIndex - 1];
                    _caretPositionTop = g.Position + new Vector2(g.Width, 0);
                    _caretPositionBottom = _caretPositionTop + new Vector2(0, Font.LineHeight);
                }
                else
                {
                    _caretPositionTop = glyph.Position;
                    _caretPositionBottom = _caretPositionTop + new Vector2(0, Font.LineHeight);
                }
            }
        }

        private void setCaretPosition(int index, bool clear = true)
        {
            if (clear)
            {
                _selectionDirection = 0;
                _selection.Clear();
            }
            if (_glyphs.Length == 0)
            {
                _caretIndex = 0;
                _caretPositionTop = new Vector2(0, 0);
                _caretPositionBottom = _caretPositionTop + new Vector2(0, Font.LineHeight);
            }
            else
            {
                if (index <= 0) setCaretPosition(_glyphs[0]);
                else if (index < _glyphs.Length) setCaretPosition(_glyphs[index]);
                else
                {
                    var glyph = _glyphs[_glyphs.Length - 1];
                    _caretIndex = glyph.ArrayIndex + 1;
                    _caretPositionTop = glyph.Position + new Vector2(glyph.Width, 0);
                    _caretPositionBottom = _caretPositionTop + new Vector2(0, Font.LineHeight);
                }
            }
        }

        protected override void Draw()
		{
			if (Texture == null)
				Painter.FillRectangle(AbsolutePosition, Size, BackColor);
			else
				Painter.Texture(Texture, ClientArea, Color.White);

			if (ClipContents)
				Manager?.Scissors.Clip(ClientArea);
			
			foreach (var selectedGlyph in _selection)
				Painter.FillRectangle(selectedGlyph.AbsolutePosition, new Vector2(selectedGlyph.Width, Font.LineHeight), SelectionColor);
			Painter.String(Size, ForeColor, Font, _glyphs);
			if (_printCaret && HasFocus) Painter.Line(_caretPositionTop + AbsolutePosition + Padding.Xy + new Vector2(1, 0), _caretPositionBottom + AbsolutePosition + Padding.Xy + new Vector2(1, 0), Color.Gray, 2);

			if (ClipContents)
				Manager?.Scissors.Unclip();
			
			Painter.Rectangle(AbsolutePosition, Size, SystemColors.ControlDark);
		}
        
        public override void Tick()
        {
            _printCaret = (_caretWatch.ElapsedMilliseconds / 500) % 2 == 0;

            processControlInput();
        }

        internal override void OnKeyPress(Drawable src, Key key)
        {
            base.OnKeyPress(src, key);

            processControlInput(true);
            if (key == Key.ShiftLeft || key == Key.ShiftRight) return;
            if (key == Key.AltLeft || key == Key.AltRight) return;
            if (key == Key.ControlLeft || key == Key.ControlRight) return;
            processBasicInput(key, true);
        }

        private void processControlInput(bool ignoreTime = false)
        {
            if (_controlInputWatch.ElapsedMilliseconds > 100 || ignoreTime)
            {
                _controlInputWatch.Restart();

                var input = Manager.Input;
                if (input.KeyDown(Key.LShift) || input.KeyDown(Key.RShift))
                {
                    if (input.KeyDown(Key.Left))
                    {
                        if (_selectionDirection == 0 || _selection.Count == 0) _selectionDirection = -1;
                        if (_selectionDirection == -1)
                        {
                            if (_caretIndex > 0)
                            {
                                var g = _glyphs[_caretIndex + _selectionDirection];
                                _selection.Insert(0, g);
                                setCaretPosition(g);
                            }
                        }
                        else
                        {
                            _selection.RemoveAt(_selection.Count - 1);
                            setCaretPosition(_glyphs[_caretIndex - _selectionDirection]);
                        }
                    }
                    else if (input.KeyDown(Key.Right))
                    {
                        if (_selectionDirection == 0 || _selection.Count == 0) _selectionDirection = 1;
                        if (_selectionDirection == 1)
                        {
                            if (_caretIndex < _glyphs.Length)
                            {
                                var g = _glyphs[_caretIndex];
                                _selection.Add(g);
                                setCaretPosition(g, true);
                            }
                        }
                        else
                        {
                            _selection.RemoveAt(0);
                            setCaretPosition(_glyphs[_caretIndex - _selectionDirection]);
                        }
                    }
                    else if (input.KeyDown(Key.Up))
                    {
                        if (_glyphs.Length == 0) return;
                        if (_selectionDirection == 0 || _selection.Count == 0) _selectionDirection = -1;
                        
                        GlyphLocation glyph = getCaretGlyph();
                        GlyphLocation upperGlyph = glyph;
                        foreach (var g in _glyphs)
                            if (g.LineIndex == glyph.LineIndex - 1 && g.ColumnIndex <= glyph.ColumnIndex)
                                upperGlyph = g;

                        if (upperGlyph != glyph)
                        {
                            if (_selection.Count > 0 && _selectionDirection != -1)
                            {
                                var g = _selection[0];
                                _selection.Clear();
                                setCaretPosition(g);
                            }

                            for (int i = _caretIndex - 1; i > upperGlyph.ArrayIndex; i--)
                                _selection.Insert(0, _glyphs[i]);
                            if (_selection.Count > 0) setCaretPosition(_selection[0]);
                        }
                    }
                    else if (input.KeyDown(Key.Down))
                    {
                        if (_glyphs.Length == 0) return;
                        if (_selectionDirection == 0 || _selection.Count == 0) _selectionDirection = 1;

                        GlyphLocation glyph = getCaretGlyph();
                        GlyphLocation lowerGlyph = glyph;
                        foreach (var g in _glyphs)
                            if (g.LineIndex == glyph.LineIndex + 1 && g.ColumnIndex <= glyph.ColumnIndex)
                                lowerGlyph = g;

                        if (lowerGlyph != glyph)
                        {
                            if (_selection.Count > 0 && _selectionDirection != 1)
                            {
                                var g = _selection[_selection.Count - 1];
                                _selection.Clear();
                                setCaretPosition(g);
                            }

                            for (int i = _caretIndex; i < lowerGlyph.ArrayIndex; i++)
                                _selection.Add(_glyphs[i]);
                            if (_selection.Count > 0) setCaretPosition(_selection[_selection.Count - 1], true);
                        }
                    }
                }
                else
                {
                    if (input.KeyDown(Key.Left))
                    {
                        if (_selection.Count != 0) setCaretPosition(_selection.First());
                        else setCaretPosition(_caretIndex - 1);
                    }
                    else if (input.KeyDown(Key.Right))
                    {
                        if (_selection.Count != 0) setCaretPosition(_selection.Last().ArrayIndex + 1);
                        else setCaretPosition(_caretIndex + 1);
                    }
                    else if (input.KeyDown(Key.Down))
                    {
                        if (_glyphs.Length == 0) return;
                        GlyphLocation glyph = getCaretGlyph();
                        GlyphLocation lowerGlyph = glyph;
                        foreach (var g in _glyphs)
                            if (g.LineIndex == glyph.LineIndex + 1 && g.ColumnIndex <= glyph.ColumnIndex)
                                lowerGlyph = g;
                        setCaretPosition(lowerGlyph.ArrayIndex);
                    }
                    else if (input.KeyDown(Key.Up))
                    {
                        if (_glyphs.Length == 0) return;
                        GlyphLocation glyph = getCaretGlyph();
                        GlyphLocation upperGlyph = glyph;
                        foreach (var g in _glyphs)
                            if (g.LineIndex == glyph.LineIndex - 1 && g.ColumnIndex <= glyph.ColumnIndex)
                                upperGlyph = g;
                        setCaretPosition(upperGlyph.ArrayIndex);
                    }
                }
            }
        }
        private void processBasicInput(Key key, bool ignoreTime = false)
        {
            if (_inputWatch.ElapsedMilliseconds > 250 || ignoreTime)
            {
                _inputWatch.Restart();

                var input = Manager.Input;

                var c = Converter.Convert(key, input);
                if (key == Key.A && input.Control)
                {
                    _selection.Clear();
                    _selection.AddRange(_glyphs);
                }
                else
                {
                    if (_selection.Count > 0 && (key == Key.BackSpace || key == Key.Delete || c != '\0'))
                    {
                        var sb = _selection[0];
                        var se = _selection[_selection.Count - 1];
                        var text = _text.Remove(sb.ArrayIndex, se.ArrayIndex - sb.ArrayIndex + 1);
                        var caret = sb.ArrayIndex;
                        if (caret > text.Length) caret = text.Length - 1;
                        Text = text;
                        setCaretPosition(caret);

                        if (key == Key.BackSpace || key == Key.Delete) return;
                    }
                    if (key == Key.BackSpace)
                    {
                        if (_caretIndex > 0 && _glyphs.Length > 0)
                        {
                            var g = _glyphs[_caretIndex - 1];

                            var text = _text.Remove(g.ArrayIndex, 1);
                            var caret = g.ArrayIndex;
                            if (caret > text.Length) caret = text.Length - 1;
                            Text = text;
                            setCaretPosition(caret);
                        }
                    }
                    else if (key == Key.Delete)
                    {
                        if (_caretIndex < _text.Length && _glyphs.Length > 0)
                        {
                            var g = _glyphs[_caretIndex];

                            var text = _text.Remove(g.ArrayIndex, 1);
                            var caret = g.ArrayIndex;
                            if (caret > text.Length) caret = text.Length - 1;
                            Text = text;
                            setCaretPosition(caret);
                        }
                    }
                    else
                    {
                        if (!_multiLine && c == '\n') return;
                        if (c != '\0')
                        {
                            var text = _text.Insert(_caretIndex, c.ToString());
                            var caret = _caretIndex + 1;
                            if (caret > text.Length) caret = text.Length - 1;
                            Text = text;
                            setCaretPosition(caret);
                        }
                    }
                }
            }
        }

        private GlyphLocation getCaretGlyph()
        {
            if (_caretIndex <= 0) return _glyphs[0];
            else if (_caretIndex < _glyphs.Length)
            {
                if (_caretAtNewLine) return _glyphs[_caretIndex - 1];
                else return _glyphs[_caretIndex];
            }
            else
            {
                return _glyphs[_glyphs.Length - 1];
            }
        }
    }
}
