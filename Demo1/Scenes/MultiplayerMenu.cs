using System;
using System.Collections.Generic;
using System.Drawing;
using Demo1;
using Demo1.Controls;
using Demo1.Networking;
using GameEngine.Graphics;
using GameEngine.Graphics.Drawables;
using GameEngine.Networking;
using GameEngine.Networking.Primitives;
using GameEngine.Networking.UDP;
using OpenTK;
using OpenTK.Input;

public class MultiplayerMenu : GraphicsScene
{
	private Label _titleLabel, _listeningStatus;
	private Panel _mainPanel, _lanBackgroundPanel;
	private ScrollablePanel _lanPanel;
	private MultiplayerNameSet _mnsPanel;
	private Button _backButton;
	private int _panelAlpha;
	private int _panelAlphaSign = 1;
	private ProgressBar _searchBar;
	private List<Button> _identifierButtons;
	private bool _hasFocus;

	public MultiplayerMenu(int width, int height) : base(width, height)
	{
		_identifierButtons = new List<Button>();

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

		_mnsPanel = new MultiplayerNameSet();
		_mnsPanel.Size = new Vector2(_mainPanel.Width - 4, 80);
		_mnsPanel.Position = new Vector2(2, 2);
		_mnsPanel.OKClicked += _mnsPanel_OKClicked;
		_mainPanel.Add(_mnsPanel);

		_listeningStatus = new Label();
		_listeningStatus.Position = new Vector2(2, 2);
		_listeningStatus.Visible = false;
		_mainPanel.Add(_listeningStatus);

		_lanBackgroundPanel = new Panel();
		_lanBackgroundPanel.Size = new Vector2(_mainPanel.Width - 6, _mainPanel.Height - _mnsPanel.Height - 4);
		_lanBackgroundPanel.Position = new Vector2(2, _mnsPanel.Height + 2);
		_mainPanel.Add(_lanBackgroundPanel);
		_mainPanel.CenterHorizontally(_lanBackgroundPanel);

		_backButton = new Button() { AutoSize = false };
		_backButton.Font = Fonts.ButtonFont;
		_backButton.Text = "Back";
		_backButton.Size = new Vector2(Width * 0.4700855f, Height * 0.06016598f);
		_backButton.Position = new Vector2(Width * 0.117094f, Height * 0.6113226f);
		_backButton.MouseClick += BackButton_MouseClick;
		_mainPanel.Add(_backButton);

		_lanPanel = new ScrollablePanel();
		_lanPanel.Position = new Vector2(0, 3);
		_lanPanel.Size = _backButton.Position - _lanBackgroundPanel.Position - _lanPanel.Position;
		_lanPanel.Size = new Vector2(_lanBackgroundPanel.Width, _lanPanel.Height - 4);
		_lanPanel.VerticalScrollBar.BackColor = Color.Transparent;
		_lanPanel.HorizontalScrollBar.BackColor = Color.Transparent;
		_lanPanel.PerformLayout += LanPanel_PerformLayout;
		_lanBackgroundPanel.Add(_lanPanel);
		_lanBackgroundPanel.CenterHorizontally(_lanPanel);

		_searchBar = new ProgressBar();
		_searchBar.Size = new Vector2(_lanPanel.Width, 3);
		_searchBar.Marquee = true;
		_searchBar.BackColor = Color.Transparent;
		_searchBar.BorderWidth = 0;
		_lanBackgroundPanel.Add(_searchBar);

		this.CenterHorizontally(_titleLabel);
		setColors(_backButton);

		GotFocus += MultiplayerMenu_GotFocus;
		LostFocus += MultiplayerMenu_LostFocus;
	}

	private void LanPanel_PerformLayout(Drawable obj)
	{
		foreach (var button in _identifierButtons)
			button.Size = new Vector2(_lanPanel.WorkingArea.Width - 4, button.Height);
	}

	private void discoveryFinished(IAsyncResult res)
	{
		if (_hasFocus)
		{
			if (res != null)
			{
				base.Manager.Invoke(() =>
				{
					processResults(res);
				});
			}
			Discovery.Search(MultiplayerInstance.Port, null, discoveryFinished, null);
		}
	}

	private void processResults(IAsyncResult res)
	{
		removePreviousIdentifiers();
		var ips = Discovery.GetIPAddresses();
		DiscoveryResult result = (DiscoveryResult)res;
		foreach (var address in result.Results)
		{
			bool isMine = false;

			if (!_mnsPanel.Visible)
			{
				foreach (var ip in ips)
					if (ip.Equals(address))
						isMine = true;
			}

			//if (!isMine)
			{
				var client = new UDPClient();
                client.Init(address, MultiplayerInstance.Port);
				client.Send<RequestIdentifier>(new IdentifierPrimitive() { IP = address.ToString() });
				Manager.Register(client);
			}
		}
	}

	private void removePreviousIdentifiers()
	{
		foreach (var button in _identifierButtons)
			_lanPanel.Remove(button);
		_identifierButtons.Clear();
	}

	private void MultiplayerMenu_GotFocus(Drawable obj)
	{
		_hasFocus = true;
		discoveryFinished(null);
	}

	private void MultiplayerMenu_LostFocus(Drawable obj)
	{
		_hasFocus = false;
		removePreviousIdentifiers();
	}

	private void _mnsPanel_OKClicked()
	{
		if (_mnsPanel.Visible)
		{
			_mnsPanel.Visible = false;
			_listeningStatus.Visible = true;
			_listeningStatus.Text = "You are now visible as: " + _mnsPanel.Identifier;
			MultiplayerInstance.Instance.Identifier = _mnsPanel.Identifier;
			MultiplayerInstance.Instance.StartServer(Manager);
		}
	}

	public void AddIdentifier(string name, string ip)
	{
		var objects = _lanPanel.Objects;
		Drawable last = null;
		if (objects.Count > 0)
			last = objects[objects.Count - 1];
		var button = new Button() { AutoSize = false };

		button.Size = new Vector2(_lanPanel.WorkingArea.Width - 4, 20);
		button.Text = name;
		button.Tag = ip;
		if (last == null)
			button.Position = new Vector2(2, 2);
		else
			button.Position = new Vector2(2, last.Position.Y + last.Size.Y + 2);
		button.MouseClick += serverButton_MouseClick;
		_lanPanel.Add(button);
		_identifierButtons.Add(button);
	}

	private void serverButton_MouseClick(Drawable arg1, Vector2 arg2, MouseButton arg3)
	{
		var ip = (string)arg1.Tag;
		if (ip == null) return;
		if (_mnsPanel.Identifier == "")
		{
			MessageBox.Show(this, "Warning", "You have to input an identifier first.", "Ok");
			return;
		}
		MultiplayerInstance.Instance.Identifier = _mnsPanel.Identifier;
		var client = new UDPClient();
        client.Init(ip, MultiplayerInstance.Port);
		client.Send<RequestGame>(new EmptyPrimitive());
		Manager.Register(client);
	}

	private void BackButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
	{
		_mnsPanel.Reset();
		if (!_mnsPanel.Visible)
		{
			_mnsPanel.Visible = true;
			_listeningStatus.Visible = false;
			MultiplayerInstance.Instance.StopServer(Manager);
		}
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