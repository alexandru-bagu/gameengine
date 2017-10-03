using OpenTK.Input;
using Demo1;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;
using GameEngine.Graphics;
using Demo1.Networking.Primitives;

public class GameScene : GraphicsScene
{
    private Panel _background;
    private Game _game;
	private int _topScore, _bottomScore;
	private Label _topName, _bottomName, _topScoreLabel, _bottomScoreLabel, _separatorLabel;
    private Label _middleTextAnimation, _middleTextAnimationShadow;
    private int _countDown;
    private bool _start, _end;
    private float _animationStopwatch;
    private int _animationTimeout;

	public bool Starting => _start;
	public bool Multiplayer => _game != null && _game is MultiPlayer;

    public GameScene(int width, int height) : base(width, height)
    {
        _background = new Panel();
        _background.Size = new Vector2(width, height);
        _background.BackColor = Color.Black;
        Add(_background);

		_separatorLabel = new Label();
		_separatorLabel.Font = Fonts.SeparatorFont;
		_separatorLabel.Text = ":";
		_separatorLabel.ForeColor = Color.White;
		Add(_separatorLabel);
		_separatorLabel.Position = new Vector2(0, 2);
		this.CenterHorizontally(_separatorLabel);

		_topScoreLabel = new Label();
		_topScoreLabel.Font = _separatorLabel.Font;
		_topScoreLabel.Text = _topScore.ToString();
		_topScoreLabel.ForeColor = Color.Green;
		Add(_topScoreLabel);
		_topScoreLabel.Position = _separatorLabel.Position - new Vector2(20, 0);

		_topName = new Label();
		_topName.Font = Fonts.ButtonFont;
		_topName.ForeColor = Color.Green;
		Add(_topName);

		_bottomScoreLabel = new Label();
		_bottomScoreLabel.Font = _separatorLabel.Font;
		_bottomScoreLabel.Text = _bottomScore.ToString();
		_bottomScoreLabel.ForeColor = Color.Blue;
		Add(_bottomScoreLabel);
		_bottomScoreLabel.Position = _separatorLabel.Position + new Vector2(20, 0);

		_bottomName = new Label();
		_bottomName.Font = Fonts.ButtonFont;
		_bottomName.ForeColor = Color.Blue;
		Add(_bottomName);

        _middleTextAnimationShadow = new Label();
        _middleTextAnimationShadow.Visible = false;
        _middleTextAnimationShadow.ClipContents = false;
        Add(_middleTextAnimationShadow);

        _middleTextAnimation = new Label();
        _middleTextAnimation.Visible = false;
        _middleTextAnimation.ClipContents = false;
        Add(_middleTextAnimation);

        _animationStopwatch = 0;
        _countDown = 3;
        _start = true;
        _end = false;
        PreviewKeys = true;
        PreviewKeyPress += GameScene_PreviewKeyPress;
    }

	public void UpdateScore(ScorePrimitive data)
	{
		_game_Goal(!data.GoalGreen);
	}

	public void UpdateState(StatePrimitive data)
	{
		_game.UpdateState(data);
	}

	public void Reset()
	{
		_topScore = _bottomScore = 0;

		_topScoreLabel.Text = _topScore.ToString();
		_bottomScoreLabel.Text = _bottomScore.ToString();

		if (_game != null)
			_game.ResetPositions();
		
		_animationStopwatch = 0;
		_countDown = 3;
		_start = true;
		_end = false;

		_middleTextAnimationShadow.Visible = false;
		_middleTextAnimation.Visible = false;
	}

	public void SetAI(AI ai)
	{
		if (_game != null) Remove(_game);
		_game = new SinglePlayer(this, (int)Width, (int)(Height * 0.96f));
		_game.Position = new Vector2(0, Height - _game.Height);
		_game.Goal += _game_Goal;
		Add(_game);
        _game.SetAI(ai);

		setOpponentName("CPU");

		BringToFront(_middleTextAnimationShadow);
		BringToFront(_middleTextAnimation);
    }

	public void SetPlayerOpponent(string opponent)
	{
		if (_game != null) Remove(_game);
		_game = new MultiPlayer(this, (int)Width, (int)(Height * 0.96f));
		_game.Position = new Vector2(0, Height - _game.Height);
		_game.Goal += _game_Goal;
		Add(_game);
		_game.SetOpponent(opponent);
		setOpponentName(opponent);

		BringToFront(_middleTextAnimationShadow);
		BringToFront(_middleTextAnimation);
	}

	private void setOpponentName(string opponent)
	{
		_topName.Text = opponent;
		_topName.Position = _topScoreLabel.Position - _topName.Size * Vector2.UnitX;
		_bottomName.Text = "You";
		_bottomName.Position = _bottomScoreLabel.Position + _bottomScoreLabel.Size * Vector2.UnitX;
	}

	private void _game_Goal(bool topSide)
	{
		_game.ResetPositions();
        if (topSide) _bottomScore++;
        else _topScore++;

        _topScoreLabel.Text = _topScore.ToString();
        _bottomScoreLabel.Text = _bottomScore.ToString();
        _start = true;

		int maxScore = 7;
        if (_bottomScore >= maxScore || _topScore >= maxScore)
        {
			if (topSide) beginAnimation("Blue won!", Color.Blue, 3000, Fonts.GoalFont, false);
            else beginAnimation("Green won!", Color.Green, 3000, Fonts.GoalFont, false);
            _end = true;
        }
        else
        {
            if (topSide) beginAnimation("Goal blue!", Color.Blue, 2000, Fonts.GoalFont, false);
            else beginAnimation("Goal green!", Color.Green, 2000, Fonts.GoalFont, false);
            _countDown = 3;
        }
    }

    private void beginAnimation(string text, Color color, int timeout, FontAsset font, bool shadow = true)
	{
        _animationStopwatch = 0;
        _animationTimeout = timeout;
        _middleTextAnimation.Visible = true;
        _middleTextAnimation.Text = text;
        _middleTextAnimation.Font = font;
        _middleTextAnimation.ForeColor = color;
        _middleTextAnimation.Scale = Vector3.UnitZ;
        _middleTextAnimation.RotationAngle = 0;
        this.Center(_middleTextAnimation);


        _middleTextAnimationShadow.Visible = shadow;
        _middleTextAnimationShadow.Text = text;
        _middleTextAnimationShadow.Font = font;
        _middleTextAnimationShadow.ForeColor = Color.FromArgb(50, Color.Black);
        _middleTextAnimationShadow.Scale = new Vector3(0.2f, 0.2f, 1f);
        _middleTextAnimationShadow.RotationAngle = 0;
        this.Center(_middleTextAnimationShadow);
        _middleTextAnimationShadow.Position += Vector2.UnitY;
    }

    private bool GameScene_PreviewKeyPress(GraphicsScene source, Key key)
    {
		if (key == Key.Escape)
        {
            Program.PauseScene.SavePosition();
            Program.PauseScene.Position = Position;
            Program.PauseScene.SetView(Manager);
        }
        return false;
    }

	public override void Tick()
	{
		_animationStopwatch += Manager.TicksPerSecond;
		if (_middleTextAnimation.Visible)
		{
			var scaleMax = 2;
			var scaleMin = 0;
			var scaleFactor = 0.05f;
			if (_animationStopwatch < _animationTimeout && _middleTextAnimation.Scale.X + scaleFactor < scaleMax)
			{
				_middleTextAnimation.Scale += new Vector3(scaleFactor, scaleFactor, 0);
				_middleTextAnimationShadow.Scale += new Vector3(scaleFactor, scaleFactor, 0);
				var steps = (scaleMax - scaleMin) / scaleFactor;
				var angleFactor = 360f / steps;
				_middleTextAnimation.RotationAngle += angleFactor;
			}

			if ((_animationStopwatch >= _animationTimeout && _middleTextAnimation.Scale.X + scaleFactor >= scaleMax) || _animationStopwatch >= _animationTimeout)
			{
				_middleTextAnimation.Visible = false;
				_middleTextAnimationShadow.Visible = false;
			}

			_middleTextAnimationShadow.RotationAngle = _middleTextAnimation.RotationAngle;
		}

		if (_middleTextAnimation.Visible == false && !_end)
		{
			if (_countDown > 0)
			{
				beginAnimation(_countDown.ToString(), Color.Red, 1000, Fonts.BeginFont);
				_countDown--;
			}
			else if (_countDown == 0)
			{
				_countDown--;
				beginAnimation("GO!", Color.Red, 1000, Fonts.BeginFont);
			}
			else if (_countDown == -1 && _start)
			{
				_start = false;
			}
		}
		else if (_middleTextAnimation.Visible == false && _end)
		{
			Program.MainScene.SetView(Manager);
			Program.MainScene.MoveCamera(Manager);
			Reset();
			MultiplayerInstance.Instance.CleanUp(Manager);
		}

        base.Tick();
    }
}