using Demo1;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using GameEngine.Graphics;

public class SinglePlayerMenu : GraphicsScene
{
	private Label _titleLabel;
	private Panel _mainPanel;
	private Button _easyButton,_mediumButton,_hardButton,_backButton;
	private int _panelAlpha;
	private int _panelAlphaSign = 1;

    public SinglePlayerMenu(int width, int height) : base(width, height)
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

		_easyButton = new Button();
        _easyButton.Font = Fonts.ButtonFont;
        _easyButton.Text = "Easy";
        _easyButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
        _easyButton.Position = new Vector2(Width * 0.117094f, Height * 0.03870851f);
        _easyButton.MouseClick += SinglePlayerButton_MouseClick;
        _mainPanel.Add(_easyButton);

		_mediumButton = new Button();
        _mediumButton.Font = Fonts.ButtonFont;
        _mediumButton.Text = "Medium";
        _mediumButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
        _mediumButton.Position = new Vector2(Width * 0.117094f, Height * 0.1165093f);
        _mediumButton.MouseClick += MediumButton_MouseClick;
        _mainPanel.Add(_mediumButton);

		_hardButton = new Button();
        _hardButton.Font = Fonts.ButtonFont;
        _hardButton.Text = "Hard";
        _hardButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
        _hardButton.Position = new Vector2(Width * 0.117094f, Height * 0.1953475f);
        _hardButton.MouseClick += HardButton_MouseClick;
        _mainPanel.Add(_hardButton);

		_backButton = new Button();
        _backButton.Font = Fonts.ButtonFont;
        _backButton.Text = "Back";
        _backButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
        _backButton.Position = new Vector2(Width * 0.117094f, Height * 0.6113226f);
        _backButton.MouseClick += BackButton_MouseClick;
        _mainPanel.Add(_backButton);

        this.CenterHorizontally(_titleLabel);
        setColors(_easyButton);
        setColors(_mediumButton);
        setColors(_hardButton);
        setColors(_backButton);
    }

    private void HardButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
    {
        ((GameScene)Program.GameScene).SetAI(new AI(16));
        Program.GameScene.SetView(Manager);
        Program.GameScene.MoveCamera(Manager);
    }

    private void MediumButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
    {
        ((GameScene)Program.GameScene).SetAI(new AI(12));
        Program.GameScene.SetView(Manager);
        Program.GameScene.MoveCamera(Manager);
    }

    private void SinglePlayerButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
    {
        ((GameScene)Program.GameScene).SetAI(new AI(8));
        Program.GameScene.SetView(Manager);
        Program.GameScene.MoveCamera(Manager);
    }

    private void BackButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
    {
        Program.MainScene.SetView(Manager);
        Program.MainScene.MoveCamera(Manager);
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