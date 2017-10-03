using GameEngine.Graphics.Drawables;
using GameEngine.Graphics;
using OpenTK;
using System.Drawing;
using System;
using GameEngine;

namespace SceneMaker.Controls
{
    public class Toolbox : Panel
    {
        public event Action<Tool, DragInformation, Vector2> ToolDrag;
        public new event Action<Tool> DragEnd;

        public Toolbox()
        {
			Padding = new Vector4(2, 2, 2, 2);
            Position = new Vector2(0, 0);
            BackColor = Color.LightGray;
        }

        private float getLastToolY()
        {
            if (Objects.Count == 0) return -20;
            return Objects[Objects.Count - 1].Position.Y;
        }

        public void GenerateTools()
        {
            Func<string, Drawable> newLabel = (name) => new Label() { Name = name, Text = name };
            Func<string, Drawable> newTextBox = (name) => new TextBox() { Name = name, Text = name, Size = new Vector2(100, 25), Enabled = false };
            Func<string, Drawable> newPanel = (name) => new Panel() { Name = name, Size = new Vector2(300, 200), BackColor = SystemColors.Control, BorderColor = Color.Black, BorderWidth = 1 };
            Func<string, Drawable> newRoundedPanel = (name) => new RoundedPanel() { Name = name, Size = new Vector2(300, 200), BackColor = SystemColors.Control, BorderColor = Color.Black, BorderWidth = 1 };
            Func<string, Drawable> newCheckBox = (name) => new CheckBox() { Name = name, Text = name };
            Func<string, Drawable> newButton = (name) => new Button() { Name = name, Text = name, Size = new Vector2(80, 20) };
			Func<string, Drawable> newProgressBar = (name) => new ProgressBar() { Name = name, Size = new Vector2(200, 20), Marquee = true };
            Func<string, Drawable> newTrackBar = (name) => new TrackBar() { Name = name, Size = new Vector2(200, 40) };

            AddTool(new Tool("Label", newLabel) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Text Box", newTextBox) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Panel", newPanel) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Rounded Panel", newRoundedPanel) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Check Box", newCheckBox) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Button", newButton) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Progress Bar", newProgressBar) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
            AddTool(new Tool("Track Bar", newTrackBar) { Position = new Vector2(0, 20 + getLastToolY()), Size = new Vector2(Width, 20) });
        }
        
        private void AddTool(Tool tool)
        {
            Add(tool);
            tool.Draggable = true;
            tool.Drag += (a, b, c) => 
            {
                ToolDrag?.Invoke((Tool)a, b, c);
            };
            tool.DragEnd += (a, b, c) =>
            {
                DragEnd?.Invoke((Tool)a);
            };
        }
    }
}
