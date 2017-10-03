using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using Demo1;
using GameEngine.Graphics;
using OpenTK.Input;
using Demo1.Networking;
using GameEngine.Networking.Primitives;

public class PauseMenu : GraphicsScene
{
	private Label _titleLabel;
	private Panel _background, _mainPanel;
	private Button _settingsButton, _exitButton, _backButton;
	private int _panelAlpha;
	private int _panelAlphaSign = 1;

	public PauseMenu(int width, int height) : base(width, height)
	{
		_background = new Panel();
		_background.Size = Size;
		_background.BackColor = Color.FromArgb(100, Color.Gray);
		Add(_background);

		_titleLabel = new Label();
		_titleLabel.Text = "Air Hockey";
		_titleLabel.AutoSize = true;
		_titleLabel.Font = Fonts.TitleFont;
		_titleLabel.ForeColor = Color.Silver;
		_titleLabel.Position = new Vector2(Width * 0.4565037f, 10);
		Add(_titleLabel);

		_mainPanel = new Panel();
		_mainPanel.BorderWidth = 3;
		_mainPanel.BorderColor = Color.FromArgb(20, Color.Black);
		_mainPanel.Size = new Vector2(Width * 0.7042735f, Height * 0.6915249f);
		_mainPanel.Position = new Vector2(Width * 0.1478633f, Height * 0.1542376f);
		Add(_mainPanel);

		_settingsButton = new Button() { AutoSize = false };
		_settingsButton.Font = Fonts.ButtonFont;
		_settingsButton.Text = "Settings";
		_settingsButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_settingsButton.Position = new Vector2(Width * 0.117094f, Height * 0.03870851f);
		_settingsButton.MouseClick += SettingsButton_MouseClick;
		_mainPanel.Add(_settingsButton);

		_exitButton = new Button() { AutoSize = false };
		_exitButton.Font = Fonts.ButtonFont;
		_exitButton.Text = "Exit to main menu";
		_exitButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_exitButton.Position = new Vector2(Width * 0.117094f, Height * 0.1165093f);
		_exitButton.MouseClick += ExitButton_MouseClick;
		_mainPanel.Add(_exitButton);

		_backButton = new Button() { AutoSize = false };
		_backButton.Font = Fonts.ButtonFont;
		_backButton.Text = "Back";
		_backButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_backButton.Position = new Vector2(Width * 0.117094f, Height * 0.6113226f);
		_backButton.MouseClick += CloseButton_MouseClick;
		_mainPanel.Add(_backButton);

		this.CenterHorizontally(_titleLabel);
		setColors(_settingsButton);
		setColors(_backButton);

		PreviewKeys = true;
		PreviewKeyPress += PauseMenu_PreviewKeyPress;
	}

	private bool PauseMenu_PreviewKeyPress(GraphicsScene source, Key key)
	{
		if (key == Key.Escape)
		{
			RestorePosition();
			Program.GameScene.SetView(Manager);
		}
		return false;
	}

	private void ExitButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		RestorePosition();

		Program.MainScene.SetView(Manager);
		Program.MainScene.MoveCamera(Manager);
		var gameScene = Program.GameScene as GameScene;
		gameScene.Reset();
		if (gameScene.Multiplayer)
		{
			MultiplayerInstance.Instance.Client.Send<ResetGame>(new EmptyPrimitive());
		}
	}

	private void SettingsButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		Visible = false;
		(Program.SettingsScene as SettingsMenu).Background.Visible = true;
		Program.SettingsScene.SavePosition();
		Program.SettingsScene.Position = Position;
		Program.SettingsScene.Focus();
		Program.SettingsScene.SetView(Manager);
	}

	private void CloseButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		if (arg3 == OpenTK.Input.MouseButton.Left)
		{
			RestorePosition();
			Program.GameScene.SetView(Manager);
		}
	}

	private void setColors(Button button)
	{
		var alpha = _panelAlpha ^ 255;
		button.BackColor = Color.FromArgb(alpha, Color.White);
		button.HighlightColor = Color.FromArgb(alpha, Color.LightBlue);
		button.PressColor = Color.FromArgb(alpha, Color.CadetBlue);
	}

	public override void Tick()
	{
		_panelAlpha = (_panelAlpha + _panelAlphaSign);
		if (_panelAlpha == 0 || _panelAlpha == 120) _panelAlphaSign *= -1;
		_mainPanel.BorderColor = Color.FromArgb(_panelAlpha, Color.Black);
		foreach (var obj in _mainPanel.Objects)
			if (obj is Button)
				setColors((Button)obj);

		base.Tick();
		//Propagate tick to other menus
		if (Paused) return;
		foreach (var scene in Program.AlphaSharedScenes)
			if (scene != this)
				scene.Tick();
	}

    protected override void Draw()
	{
		base.Draw();
	}
}