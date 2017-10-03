using System;
using System.Collections.Generic;
using System.Drawing;
using GameEngine.Graphics.Drawables;
using OpenTK;
using OpenTK.Input;

namespace GameEngine.Graphics
{
	public class MessageBox : Panel
	{
		public event Action<MessageBox, string> ActionSelected;

		public override bool Focusable => true;
		protected Panel _boxPanel, _titleBackground;
		protected Label _title, _text;
		protected List<Button> _buttons;
		protected Line _separator;

		public string Title
		{
			get { return _title.Text; }
			set { _title.Text = value; UpdateLayout(); }
		}

		public string Text
		{
			get { return _text.Text; }
			set { _text.Text = value; UpdateLayout(); }
		}

		public MessageBox()
		{
			_boxPanel = new Panel();
			_boxPanel.Draggable = true;
			_boxPanel.Drag += _boxPanel_Drag;
			_boxPanel.BackColor = Color.White;
			_boxPanel.BorderWidth = 1;
			_boxPanel.BorderColor = Color.Black;
			_boxPanel.PerformLayout += _boxPanel_PerformLayout;

			_titleBackground = new Panel();
			_titleBackground.BackColor = Color.Gray;

			_title = new Label();
			_separator = new Line();
			_separator.LineWidth = 1;
			_separator.Color = Color.Black;
			_text = new Label();

			_buttons = new List<Button>();

			_boxPanel.Add(_titleBackground);
			_boxPanel.Add(_title);
			_boxPanel.Add(_separator);
			_boxPanel.Add(_text);

			Add(_boxPanel);
		}

		public void AddButton(string action)
		{
			var _button = new Button();
			_button.Text = action;
			_button.AutoSize = true;
			_button.MouseClick += _button_MouseClick;
			_boxPanel.Add(_button);
			_buttons.Add(_button);
		}

		private void _boxPanel_Drag(Drawable arg1, DragInformation arg2, Vector2 arg3)
		{
			_boxPanel.ProcessDrag(arg2, arg3);
		}

		private void _boxPanel_PerformLayout(Drawable obj)
		{
			_titleBackground.Size = new Vector2(_boxPanel.Width, _title.Height);
			_boxPanel.CenterHorizontally(_title);
			_separator.Point1 = Vector2.UnitY * _text.Size;
			_separator.Point2 = Vector2.UnitX * _boxPanel.Size + Vector2.UnitY * _text.Size;
			_text.Position = new Vector2(0, _title.Height + 4);
			_boxPanel.CenterHorizontally(_text);

			float btnWidth = 0, btnHeight = 0;
			foreach (var btn in _buttons)
			{
				btnWidth += btn.Width + 3;
				btnHeight = Math.Max(btnHeight, btn.Height);
			}
			var position = _boxPanel.CenterHorizontally(new Vector2(btnWidth, 0)).X;
			foreach (var btn in _buttons)
			{
				btn.Position = new Vector2(position, _text.Position.Y + _text.Height + 4);
				position += btn.Width + 3;
			}
		}

		private void _button_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
		{
			Parent.Remove(this);
			ActionSelected?.Invoke(this, (arg1 as Button).Text);
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();

			float btnWidth = 0, btnHeight = 0;
			foreach (var btn in _buttons)
			{
				btnWidth += btn.Width + 3;
				btnHeight = Math.Max(btnHeight, btn.Height);
			}

			var minWidth = Math.Max(_title.Width, Math.Max(btnWidth, _text.Width));
			var minHeight = _title.Height + _text.Height + btnHeight + 10;
			_boxPanel.Size = new Vector2(minWidth, minHeight);
		}

		public void Center(Vector2 cameraPosition, int windowWidth, int windowHeight)
		{
			_boxPanel.Position = cameraPosition + new Vector2((windowWidth - _boxPanel.Width) / 2, (windowHeight - _boxPanel.Height) / 2);
		}

		public override Drawable PickElement(Vector2 position)
		{
			var elem = base.PickElement(position);
			if (elem is Button && _buttons.Contains((Button)elem)) return elem;
			if (_boxPanel.Pick(position)) return _boxPanel;
			return this;
		}

		public void Show(GraphicsScene context)
		{
			Size = new Vector2(context.Width, context.Height);
			Center(context.Manager.Camera.Position - context.Position, context.Manager.Width, context.Manager.Height);
			context.Add(this);
		}

		public static MessageBox Show(GraphicsScene context, string title, string text, params string[] buttons)
		{
			var msgBox = Create(title, text, buttons);
			msgBox.Show(context);
			return msgBox;
		}

		public static MessageBox Create(string title, string text, params string[] buttons)
		{
			var msgBox = new MessageBox();
			msgBox.Title = title;
			msgBox.Text = text;
			foreach (var button in buttons)
				msgBox.AddButton(button);
			return msgBox;
		}
	}
}
