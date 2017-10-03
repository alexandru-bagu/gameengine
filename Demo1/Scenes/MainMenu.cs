using System;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using Demo1;
using GameEngine.Graphics;

public class MainMenu : GraphicsScene
{
	private Label _titleLabel;
	private Panel _mainPanel;
	private Button _singlePlayerButton, _twoPlayersButton, _settingsButton, _closeButton;
	private int _panelAlpha;
	private int _panelAlphaSign = 1;

	public MainMenu(int width, int height) : base(width, height)
	{
		_titleLabel = new Label();
		_titleLabel.Text = "Air Hockey";
		_titleLabel.AutoSize = true;
		_titleLabel.Font = Fonts.TitleFont;
		_titleLabel.Position = new Vector2(Width * 0.4565037f, 10);
		Add(_titleLabel);

		_mainPanel = new Panel();
		_mainPanel.BorderWidth = 3;
		_mainPanel.BorderColor = Color.FromArgb(20, Color.Black);
		_mainPanel.Size = new Vector2(Width * 0.7042735f, Height * 0.6915249f);
		_mainPanel.Position = new Vector2(Width * 0.1478633f, Height * 0.1542376f);
		Add(_mainPanel);

		_singlePlayerButton = new Button() { AutoSize = false };
		_singlePlayerButton.Font = Fonts.ButtonFont;
		_singlePlayerButton.Text = "Single player";
		_singlePlayerButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_singlePlayerButton.Position = new Vector2(Width * 0.117094f, Height * 0.03870851f);
		_singlePlayerButton.MouseClick += SinglePlayerButton_MouseClick;
		_mainPanel.Add(_singlePlayerButton);

		_twoPlayersButton = new Button() { AutoSize = false };
		_twoPlayersButton.Font = Fonts.ButtonFont;
		_twoPlayersButton.Text = "Two players (lan)";
		_twoPlayersButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_twoPlayersButton.Position = new Vector2(Width * 0.117094f, Height * 0.1165093f);
		_twoPlayersButton.MouseClick += TwoPlayersButton_MouseClick;
		_mainPanel.Add(_twoPlayersButton);

		_settingsButton = new Button() { AutoSize = false };
		_settingsButton.Font = Fonts.ButtonFont;
		_settingsButton.Text = "Settings";
		_settingsButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_settingsButton.Position = new Vector2(Width * 0.117094f, Height * 0.1953475f);
		_settingsButton.MouseClick += SettingsButton_MouseClick;
		_mainPanel.Add(_settingsButton);

		_closeButton = new Button() { AutoSize = false };
		_closeButton.Font = Fonts.ButtonFont;
		_closeButton.Text = "Exit";
		_closeButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_closeButton.Position = new Vector2(Width * 0.117094f, Height * 0.6113226f);
		_closeButton.MouseClick += CloseButton_MouseClick;
		_mainPanel.Add(_closeButton);

		this.CenterHorizontally(_titleLabel);
		setColors(_singlePlayerButton);
		setColors(_twoPlayersButton);
		setColors(_settingsButton);
		setColors(_closeButton);
	}

	private void TwoPlayersButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		Program.MultiplayerScene.SetView(Manager);
		Program.MultiplayerScene.MoveCamera(Manager);
	}

	private void SettingsButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		Program.SettingsScene.SetView(Manager);
		Program.SettingsScene.MoveCamera(Manager);
	}

	private void SinglePlayerButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		Program.SinglePlayerScene.SetView(Manager);
		Program.SinglePlayerScene.MoveCamera(Manager);
	}

	private void CloseButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		if (arg3 == OpenTK.Input.MouseButton.Left)
		{
			Environment.Exit(0);
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