using GameEngine.Graphics.Drawables;
using GameEngine.Graphics;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using Demo1;

public class SettingsMenu : GraphicsScene
{
	private Panel _mainPanel, _panel2, _panel3;
	private Label _titleLabel,_mvLabel, _evLabel;
	private Button _backButton;
	private TrackBar _mvTrackBar, _evTrackBar;
	private int _panelAlpha;
	private int _panelAlphaSign = 1;
	private Panel _background;

	public Panel Background => _background;

    public SettingsMenu(int width, int height) : base(width, height)
    {
		_background = new Panel();
		_background.Size = Size;
		_background.BackColor = Color.FromArgb(150, Color.Gray);
		_background.Visible = false;
		Add(_background);

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

        _backButton = new Button() { AutoSize = false };
        _backButton.Font = Fonts.ButtonFont;
        _backButton.Text = "Back";
        _backButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
        _backButton.Position = new Vector2(Width * 0.117094f, Height * 0.6113226f);
        _backButton.MouseClick += BackButton_MouseClick;
        _mainPanel.Add(_backButton);

        _panel2 = new Panel();
        _panel2.Size = new Vector2(_mainPanel.Width, Height * 0.1078125f);
        _panel2.Position = new Vector2(Width * 0.0175f, Height * 0.06920329f);
        _mainPanel.Add(_panel2);

        _mvLabel = new Label();
        _mvLabel.Size = new Vector2(Width * 0.06361327f, Height * 0.02159118f);
        _mvLabel.Position = new Vector2(Width * 0.005f, Height * 0.01269531f);
        _mvLabel.Text = "Music volume"; 
		_mvLabel.Font = Fonts.ButtonFont;
        _panel2.Add(_mvLabel);

        _mvTrackBar = new TrackBar();
		_mvTrackBar.Size = new Vector2(_panel2.Width * 0.9f, Height * 0.03613281f);
        _mvTrackBar.BackColor = Color.Transparent;
        _mvTrackBar.Minimum = 0;
        _mvTrackBar.Maximum = 1;
        _mvTrackBar.Value = Program.MusicVolume;
        _mvTrackBar.ValueChanged += MvTrackBar_ValueChanged;
        _panel2.Add(_mvTrackBar);

        _panel3 = new Panel();
        _panel3.Size = new Vector2(_mainPanel.Width, Height * 0.1078125f);
		_panel3.Position = _panel2.Position + Vector2.UnitY * _panel2.Size;
        _mainPanel.Add(_panel3);

        _evLabel = new Label();
		_evLabel.Font = Fonts.ButtonFont;
        _evLabel.Size = new Vector2(Width * 0.06361327f, Height * 0.02159118f);
        _evLabel.Position = new Vector2(Width * 0.005f, Height * 0.01464847f);
        _evLabel.Text = "Effects volume";
        _panel3.Add(_evLabel);

        _evTrackBar = new TrackBar();
		_evTrackBar.Size = new Vector2(_panel3.Width * 0.9f, Height * 0.03613281f);
        _evTrackBar.BackColor = Color.Transparent;
        _evTrackBar.Minimum = 0;
        _evTrackBar.Maximum = 1;
        _evTrackBar.Value = Program.EffectsVolume;
        _evTrackBar.ValueChanged += EvTrackBar_ValueChanged;
        _panel3.Add(_evTrackBar);

		_mvTrackBar.Position = _mvLabel.Position + new Vector2(0, 1 + _mvLabel.Size.Y);
		_evTrackBar.Position = _evLabel.Position + new Vector2(0, 1 + _evLabel.Size.Y);
		_panel2.CenterHorizontally(_mvTrackBar);
		_panel3.CenterHorizontally(_evTrackBar);

        this.CenterHorizontally(_titleLabel);
    }

    private void EvTrackBar_ValueChanged(TrackBar arg1, float arg2)
    {
        Program.SetEffectsVolume(arg2);
    }

    private void MvTrackBar_ValueChanged(TrackBar arg1, float arg2)
    {
        Program.SetMusicVolume(arg2);
    }

	private void BackButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		if (_background.Visible)
		{
			_background.Visible = false;
			RestorePosition();
			Program.PauseScene.Visible = true;
			Program.PauseScene.SetView(Manager);
		}
		else
		{
			Program.MainScene.SetView(Manager);
			Program.MainScene.MoveCamera(Manager);
		}
		Program.AppSettings.Save();
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
}
