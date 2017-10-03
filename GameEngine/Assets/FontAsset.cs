using GameEngine.Text;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;

namespace GameEngine.Assets
{
	using Graphics = System.Drawing.Graphics;

    public class FontAsset
    {
        private static FontAsset _default, _defaultBold;
        public static FontAsset Default
        {
            get
            {
                if (_default == null)
                {
                    var path = AssetManager.RelativeFont("generic12.atlas");
                    if (AssetManager.AssetExists(path))
                        _default = new FontAsset(path);
                    else
                        _default = new FontAsset(new Font(FontFamily.GenericSansSerif, 12), BASIC_SET, true, 0);
                }
                return _default;
            }
        }
        public static FontAsset DefaultBold
        {
            get
            {
                if (_defaultBold == null)
                {
                    var path = AssetManager.RelativeFont("generic12b.atlas");
                    if (AssetManager.AssetExists(path))
                        _defaultBold = new FontAsset(path);
                    else
                        _defaultBold = new FontAsset(new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold), BASIC_SET, true, 0);
                }
                return _defaultBold;
            }
        }

        public const string 
            BASIC_SET = " \tabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'`\"(!?)+-*/=_{}[]@~#\\<>|^%$£&€°µ",
	        FRENCH_QUOTES = "«»‹›",
	        SPANISH_QEST_EX = "¡¿",
	        CYRILLIC_SET = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяљњќћџЉЊЌЋЏ",
	        EXTENDED_LATIN = "ÀŠŽŸžÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ",
	        GREEK_ALPHABET = "ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ",
	        TURKISH_I = "ıİŞ",
	        HEBREW_ALPHABET = "אבגדהוזחטיכךלמםנןסעפףצץקרשת",
	        ARABIC_ALPHABET = "ںکگپچژڈ¯؛ہءآأؤإئابةتثجحخدذرزسشصض×طظعغـفقكàلâمنهوçèéêëىيîï؟",
	        THAI_KHMER_ALPHABET = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำิีึืฺุู฿เแโใไๅๆ็่้๊๋์ํ๎๏๐๑๒๓๔๕๖๗๘๙๚๛",
	        HIRAGANA = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ゗゘゙゛゜ゝゞゟ",
	        JAP_DIGITS = "㆐㆑㆒㆓㆔㆕㆖㆗㆘㆙㆚㆛㆜㆝㆞㆟",
	        ASIAN_QUOTES = "「」",
	        ESSENTIAL_KANJI = "⽇⽉",
	        KATAKANA = "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ・ーヽヾヿ";

		private const string genericCharSet =
			BASIC_SET + FRENCH_QUOTES + SPANISH_QEST_EX + CYRILLIC_SET +
			EXTENDED_LATIN + GREEK_ALPHABET + TURKISH_I + HEBREW_ALPHABET +
			ARABIC_ALPHABET + THAI_KHMER_ALPHABET + HIRAGANA + JAP_DIGITS +
			ASIAN_QUOTES + ESSENTIAL_KANJI + KATAKANA;

		private const int _genericFontSpacing = 8;
		private Font _font;
        private string _charSet;
        private TextureAsset _asset;
        private float _width, _height;
        private Dictionary<char, Glyph> _glyphMap;
        private bool _antialias;
        private Bitmap _bmp;
        private Graphics _bmp_g;
        private float _xSpacing, _fontSize, _fontHeight;
        
        public TextureAsset Asset => _asset;
        public float LineHeight => _fontHeight;
        public float Size => _fontSize;

		public FontAsset(Font font, string alphabet) : this(font, alphabet, true, 0) { }
        public FontAsset(Font font, string alphabet, bool antialias) : this(font, alphabet, antialias, 0) { }
        public FontAsset(Font font, string alphabet, bool antialias, float xSpacing)
		{
            _xSpacing = xSpacing;
            _glyphMap = new Dictionary<char, Glyph>();
            _font = font;
			var hset = new HashSet<char>(alphabet.ToCharArray());
			StringBuilder builder = new StringBuilder(hset.Count);
			foreach (var c in hset) builder.Append(c);
			_charSet = builder.ToString();
            _antialias = antialias;
            _fontHeight = _font.Height;
            _fontSize = _font.Size;

            computeDimensions();
            using (var bmp = buildTexture())
                _asset = TextureAsset.LoadFromImage(bmp);
        }

        public FontAsset(string atlas) : this(new Atlas(atlas)) { }
        public FontAsset(Atlas atlas)
        {
            _glyphMap = atlas.Glyphs;
            using (var bmp = atlas.Texture)
                _asset = TextureAsset.LoadFromImage(bmp);
            _fontSize = atlas.FontSize;
            _fontHeight = atlas.FontHeight;
        }

        public Atlas MakeAtlas()
        {
            if (_width == 0) throw new Exception("Font is already atlassed.");
            return new Atlas(_glyphMap, buildTexture(), _fontSize, _fontHeight);
        }

        internal bool FindGlyph(char character, out Glyph glyph)
        {
            return _glyphMap.TryGetValue(character, out glyph);
        }

        internal GlyphLocation[] ProcessString(string text, RectangleF layout, StringFormatFlags flags)
        {
            List<GlyphLocation> list = new List<GlyphLocation>();
            float x = 0, y = 0;
            var spacing = LineHeight;
            int ai = 0, lni = 0, ci = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '\r')
                {
                    continue;
                }
                else if (c == '\n')
                {
                    if (flags.HasFlag(StringFormatFlags.DirectionVertical))
                    {
                        x += spacing;
                        y = 0;
                    }
                    else
                    {
                        x = 0;
                        y += spacing;
                    }
                    lni++;
                    ci = 0;

                    Glyph g;

                    if (FindGlyph('\0', out g)) list.Add(new GlyphLocation(g, new Vector2(x, y), ai++, lni, ci));
                }
                else
                {
                    Glyph g;

                    if (FindGlyph(c, out g))
                    {
                        if (!flags.HasFlag(StringFormatFlags.NoWrap))
                        {
                            if (flags.HasFlag(StringFormatFlags.DirectionVertical))
                            {
                                if (y + g.Height > layout.Height)
                                {
                                    x += spacing;
                                    y = 0;

                                    lni++;
                                    ci = 0;
                                }
                            }
                            else
                            {
                                if (x + g.Width > layout.Width)
                                {
                                    x = 0;
                                    y += spacing;

                                    lni++;
                                    ci = 0;
                                }
                            }
                        }
                        list.Add(new GlyphLocation(g, new Vector2(x, y), ai++, lni, ci++));

                        if (flags.HasFlag(StringFormatFlags.DirectionVertical))
                            y += g.Height;
                        else
                            x += g.Width;
                    }
                }
            }
            return list.ToArray();
        }

        private void computeDimensions()
        {
            _width = 0;
            _height = 0;
            StringFormat format;

            _bmp = new Bitmap(1, 1);
            _bmp_g = Graphics.FromImage(_bmp);

            setupGraphics(_bmp_g);

			foreach (var c in _charSet)
			{
				if (c == ' ' || c == '\t') format = StringFormat.GenericDefault;
				else format = StringFormat.GenericTypographic;

				var size = _bmp_g.MeasureString(c.ToString(), _font, int.MaxValue, format);

				_width += size.Width + _xSpacing + _genericFontSpacing;
				_height = Math.Max(_height, size.Height);
			}
			_height += 2;
        }

        private Bitmap buildTexture()
        {
            int bmpWidth = (int)Math.Ceiling(_width), bmpHeight = (int)Math.Ceiling(_height);
            float x = 0, y = 0;
            StringFormat format;
            var bmp = new Bitmap(bmpWidth, bmpHeight);
            bmp.MakeTransparent();
            using (var g = Graphics.FromImage(bmp))
            {
                setupGraphics(g);

				foreach (var c in _charSet)
				{
					x += _genericFontSpacing / 2;
					var s = c.ToString();
					if (c == ' ' || c == '\t') format = StringFormat.GenericDefault;
					else format = StringFormat.GenericTypographic;

					var size = g.MeasureString(s, _font, 0, format);
					g.DrawString(s, _font, Brushes.White, new PointF(x, y), format);
					if (Helper.MicrosoftCLR)
						_glyphMap[c] = new Glyph(c, x - _xSpacing / 2, y, size.Width + _xSpacing / 2, size.Height + 2);
					else
						_glyphMap[c] = new Glyph(c, x - _xSpacing / 2 + 1, y, size.Width + _xSpacing / 2, size.Height + 2);
					x += _genericFontSpacing / 2 + _xSpacing;
					x += size.Width;
				}
                _glyphMap['\0'] = new Glyph('\0', 0, 0, 0, 0);
            }
            return bmp;
        }

        private void setupGraphics(Graphics g)
        {
            if (_antialias)
            {
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.None;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            }
        }
    }
}
