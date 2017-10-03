using System;
using GameEngine;
using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using Demo2;

public class MenuScene : GraphicsScene
{
    private Panel _mainPanel;
    private Label _nameLabel;
    private TextBox _nameTextbox;
    private Button _startButton;
    private Label _volumeLabel;
    private TrackBar _volumeTrackbar;
    private Label _sfxLabel;
    private TrackBar _sfxTrackbar;
    private Button _exitButton;
    
	public MenuScene(int width, int height) : base(width, height)
	{
        Texture = TextureAsset.LoadRelativePath("background.jpeg");

		_mainPanel = new Panel();
		_mainPanel.Size = new Vector2(Width * 0.5859085f, Height * 0.4333552f);
		_mainPanel.Position = new Vector2(Width * 0.2082818f, Height * 0.2848017f);
        _mainPanel.BorderWidth = 1;
        _mainPanel.BorderColor = Color.Black;
        _mainPanel.BackColor = Color.FromArgb(127, Color.Gray);
		Add(_mainPanel);

		_nameLabel = new Label();
		_nameLabel.Size = new Vector2(Width * 0.05796121f, Height * 0.03566475f);
		_nameLabel.Position = new Vector2(Width * 0.02781211f, Height * 0.03620423f);
        _nameLabel.Text = "Name";
		_mainPanel.Add(_nameLabel);

		_nameTextbox = new TextBox();
		_nameTextbox.Size = new Vector2(Width * 0.39f, Height * 0.045f);
        _nameTextbox.Position = new Vector2(Width * 0.17f, Height * 0.03472494f);
        _nameTextbox.Text = Program.SettingsPlayerName;
		_mainPanel.Add(_nameTextbox);

		_volumeLabel = new Label();
		_volumeLabel.Size = new Vector2(Width * 0.05796121f, Height * 0.03566475f);
		_volumeLabel.Position = new Vector2(Width * 0.02657602f, Height * 0.09833441f);
        _volumeLabel.Text = "Volume";
		_mainPanel.Add(_volumeLabel);

		_volumeTrackbar = new TrackBar();
		_volumeTrackbar.Size = new Vector2(Width * 0.39f, Height * 0.04142012f);
        _volumeTrackbar.Position = new Vector2(Width * 0.17f, Height * 0.09833441f);
        _volumeTrackbar.BackColor = Color.Transparent;
        _volumeTrackbar.BorderWidth = 0;
        _volumeTrackbar.Maximum = 1f;
        _volumeTrackbar.Minimum = 0f;
        _volumeTrackbar.Value = Program.Volume;
        _mainPanel.Add(_volumeTrackbar);
        
        _startButton = new Button();
        _startButton.Size = new Vector2(Width * 0.1075402f, Height * 0.03846154f);
        _startButton.Position = new Vector2(Width * 0.06365884f, Height * 0.3793995f);
        _startButton.Text = "Start";
        _startButton.MouseClick += StartButton_MouseClick;
        _mainPanel.Add(_startButton);

        _exitButton = new Button();
		_exitButton.Size = new Vector2(Width * 0.1075402f, Height * 0.03846154f);
		_exitButton.Position = new Vector2(Width * 0.434487f, Height * 0.3793995f);
        _exitButton.Text = "Quit";
        _exitButton.MouseClick += ExitButton_MouseClick;
        _mainPanel.Add(_exitButton);
	}

	private void StartButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		saveSettings();

		Program.Loading.SetView(Manager);
		Program.Loading.MoveCamera(Manager, View.TweenType.CubicInOut);

		Multiplayer.Init(Manager, false);
	}

    private void saveSettings()
	{
		Program.SettingsPlayerName = 
        Program.PlayerName = _nameTextbox.Text;
        Program.Volume = _volumeTrackbar.Value;
        Program.AppSettings.Save();
    }

    private void ExitButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
    {
        saveSettings();
        Environment.Exit(0);
    }
}
