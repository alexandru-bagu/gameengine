using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System.Drawing;
using GameEngine.Assets;

public class LoadingScene : GraphicsScene
{
    private Panel _mainPanel;
    private Label _nameLabel;
    private ProgressBar _progressBar;
    
    public LoadingScene(int width, int height) : base(width, height)
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
        _nameLabel.Position = new Vector2(Width * 0.02781211f, Height * 0.03620423f);
        _nameLabel.Font = FontAsset.DefaultBold;
        _nameLabel.Text = "Looking for a match...";
        _nameLabel.ForeColor = Color.Black;
        _mainPanel.Add(_nameLabel);
        
        _progressBar = new ProgressBar();
        _progressBar.Size = new Vector2(_mainPanel.Width, 7);
        _progressBar.Marquee = true;
        _progressBar.BackColor = Color.Transparent;
        _progressBar.BorderWidth = 0;
        _mainPanel.Add(_progressBar);

        _mainPanel.CenterVertically(_progressBar);
        _mainPanel.CenterHorizontally(_nameLabel);
    }
}
