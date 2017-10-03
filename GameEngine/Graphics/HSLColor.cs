using System;
using System.Drawing;

namespace GameEngine.Graphics
{
	public struct HSLColor
	{
		private float _h, _sl, _l;

		public float H => _h;
		public float S => _sl;
		public float L => _l;

		public HSLColor(Color value)
		{
			float r = value.R / 255.0f;
			float g = value.G / 255.0f;
			float b = value.B / 255.0f;
			float v;
			float m;
			float vm;
			float r2, g2, b2;

			_h = 0; // default to black
			_sl = 0;
			_l = 0;
			v = Math.Max(r, g);
			v = Math.Max(v, b);
			m = Math.Min(r, g);
			m = Math.Min(m, b);
			_l = (m + v) / 2.0f;
			if (_l <= 0.0)
			{
				return;
			}
			vm = v - m;
			_sl = vm;
			if (_sl > 0.0)
			{
				_sl /= (_l <= 0.5f) ? (v + m) : (2.0f - v - m);
			}
			else
			{
				return;
			}
			r2 = (v - r) / vm;
			g2 = (v - g) / vm;
			b2 = (v - b) / vm;
			if (Equals(r, v))
			{
				_h = (Equals(g, m) ? 5.0f + b2 : 1.0f - g2);
			}
			else if (Equals(g, v))
			{
				_h = (Equals(b, m) ? 1.0f + r2 : 3.0f - b2);
			}
			else
			{
				_h = (Equals(r, m) ? 3.0f + g2 : 5.0f - r2);
			}
			_h /= 6.0f;
		}

		public HSLColor(float h, float sl, float l)
		{
			_h = h;
			_sl = sl;
			_l = l;
		}

		public Color ToRGB()
		{
			double v;
			double r, g, b;

			r = _l;   // default to gray
			g = _l;
			b = _l;
			v = (_l <= 0.5) ? (_l * (1.0 + _sl)) : (_l + _sl - _l * _sl);
			if (v > 0)
			{
				double m;
				double sv;
				int sextant;
				double fract, vsf, mid1, mid2;

				m = _l + _l - v;
				sv = (v - m) / v;
				_h *= 6.0f;
				sextant = (int)_h;
				fract = _h - sextant;
				vsf = v * sv * fract;
				mid1 = m + vsf;
				mid2 = v - vsf;
				switch (sextant)
				{
					case 0:
						r = v;
						g = mid1;
						b = m;
						break;
					case 1:
						r = mid2;
						g = v;
						b = m;
						break;
					case 2:
						r = m;
						g = v;
						b = mid1;
						break;
					case 3:
						r = m;
						g = mid2;
						b = v;
						break;
					case 4:
						r = mid1;
						g = m;
						b = v;
						break;
					case 5:
						r = v;
						g = m;
						b = mid2;
						break;
				}
			}
			return Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
		}

		private bool Equals(float a, float b, float epsilon = 0.001f)
		{
			return Math.Abs(a - b) <= epsilon;
		}

		public HSLColor AddHue(float value)
		{
			var newH = _h + value;
			while (newH > 1) newH -= 1f;
			while (newH < 0) newH += 1f;
			return new HSLColor(newH, _sl, _l);
		}

		public HSLColor AddSaturation(float value)
		{
			var newSL = _sl + value;
			if (newSL > 1) newSL -= 1f;
			if (newSL < 0) newSL -= 1f;
			return new HSLColor(_h, newSL, _l);
		}

		public HSLColor AddLighting(float value)
		{
			var newL = _l + value;
			if (newL > 1) newL = 1f;
			if (newL < 0) newL = 0f;
			return new HSLColor(_h, _sl, newL);
		}

		public static implicit operator HSLColor(Color rgb)
		{
			return new HSLColor(rgb);
		}

		public static implicit operator Color(HSLColor rgb)
		{
			return rgb.ToRGB();
		}
	}
}