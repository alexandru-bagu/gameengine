using System;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
	public class ScrollablePanel : Panel
	{
		private ScrollBar _hScrollBar, _vScrollBar;
		private RectangleF _clientArea;

		public override RectangleF WorkingArea => _clientArea;
		public ScrollBar HorizontalScrollBar => _hScrollBar;
		public ScrollBar VerticalScrollBar => _vScrollBar;

		public ScrollablePanel()
		{
			_hScrollBar = new ScrollBar(10) { Vertical = false };
			_vScrollBar = new ScrollBar(10) { Vertical = true };

			_hScrollBar.Parent = this;
			_vScrollBar.Parent = this;
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			_clientArea = base.ClientArea;
			float offsetX = 0, offsetY = 0;
			if (_hScrollBar.Visible) offsetY = _hScrollBar.Height;
			if (_vScrollBar.Visible) offsetX = _vScrollBar.Width;
			_clientArea = new RectangleF(_clientArea.X, _clientArea.Y, _clientArea.Width - offsetX, _clientArea.Height - offsetY);
		}

        protected override void Draw()
		{
			base.Draw();

			Manager?.Scissors.Clip(base.ClientArea);
			_hScrollBar.InvokeDraw();
			_vScrollBar.InvokeDraw();
			Manager?.Scissors.Unclip();
		}

		public override Drawable PickElement(OpenTK.Vector2 position)
		{
			if (_hScrollBar.Pick(position)) return _hScrollBar.Button;
			if (_vScrollBar.Pick(position)) return _vScrollBar.Button;
			return base.PickElement(position);
		}

		public override IContainer PickContainer(OpenTK.Vector2 position, Drawable exclude)
		{
			if (_hScrollBar.Pick(position)) return _hScrollBar.PickContainer(position, exclude);
			if (_vScrollBar.Pick(position)) return _vScrollBar.PickContainer(position, exclude);
			return base.PickContainer(position, exclude);
		}
	}
}
