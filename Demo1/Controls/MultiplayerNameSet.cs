using System;
using GameEngine.Graphics.Drawables;
using GameEngine.Graphics;
using OpenTK;
using OpenTK.Input;

namespace Demo1.Controls
{
	public class MultiplayerNameSet : Panel
	{
		public event Action OKClicked;

		private Label _infoLabel;
		private TextBox _nameTextbox;
		private Button _okButton;

		public string Identifier => _nameTextbox.Text;

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			_infoLabel.Size = new Vector2(Width - 2, 40);
			this.CenterHorizontally(_infoLabel);

			_nameTextbox.Size = new Vector2(Width - 8 - _okButton.Size.X, 30);
			_nameTextbox.Position = new Vector2(2, _infoLabel.Height + 2);
			_okButton.Position = new Vector2(_nameTextbox.Position.X + _nameTextbox.Width + 2, _nameTextbox.Position.Y - 1);
		}

		public string Text { get { return _infoLabel.Text; } set { _infoLabel.Text = value; } }

		public MultiplayerNameSet()
		{
			_infoLabel = new Label();
			_nameTextbox = new TextBox();
			_okButton = new Button();
			_okButton.AutoSize = true;
			_infoLabel.AutoSize = false;
			_infoLabel.Text = "Input an identifier for others to find you as";
			_okButton.Text = "OK";
			_okButton.MouseClick += _okButton_MouseClick;
			Add(_infoLabel);
			Add(_nameTextbox);
			Add(_okButton);
		}

		private void _okButton_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
		{
			OKClicked?.Invoke();
		}

		public void Reset()
		{
			_nameTextbox.Text = "";
		}
	}
}
