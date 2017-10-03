using System;
using System.Drawing;
using GameEngine.Assets;

namespace Demo1
{
	public class Fonts
	{
		public static FontAsset SeparatorFont, TitleFont, ButtonFont;
		public static FontAsset BeginFont, GoalFont;

		public static void Init(float ratio)
		{
			SeparatorFont = new FontAsset(new Font(FontFamily.GenericMonospace, 20 * ratio, FontStyle.Bold), ":0123456789");
			TitleFont = new FontAsset(new Font("Arial", 60 * ratio), FontAsset.BASIC_SET);
			ButtonFont = new FontAsset(new Font("Arial", 20 * ratio), FontAsset.BASIC_SET);

			BeginFont = new FontAsset(new Font(FontFamily.GenericMonospace, 72 * ratio), FontAsset.BASIC_SET);
			GoalFont = new FontAsset(new Font(FontFamily.GenericMonospace, 20 * ratio), FontAsset.BASIC_SET);
		}
	}
}
