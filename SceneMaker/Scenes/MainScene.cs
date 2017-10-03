using GameEngine;
using GameEngine.Assets;
using GameEngine.Graphics.Drawables;
using GameEngine.Graphics;
using OpenTK;
using SceneMaker.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace SceneMaker.Scenes
{
    public class MainScene : GraphicsScene
    {
        private const float gRatio = 1.61803398875f;
        private HashSet<string> _names = new HashSet<string>();
        private Panel _background;
        private Toolbox _toolbox;
        private Panel _scene;
        private Drawable _workingElement;
        private Action<Drawable, DragInformation, Vector2> dragAction ;
        private void objDrag(Drawable a, DragInformation b, Vector2 c) 
        {
            a.ProcessDrag(b, c);
        }
        private Drawable _selectedElement;
        private Drawable[] _sizeCorners;
        private IContainer _workingContainer;
        private TextBox _sceneName;
        private Label _sceneNameLabel;
        private Button _sceneSaveButton, _orientationButton;
        private ContextMenu _menu;

        private HashSet<string> _docked, _absolute;
        private Dictionary<string, Vector2> _prevSizes;
        private Dictionary<string, Vector2> _prevPos;
        private int _prevIndex;
        private IContainer _prevParent;
        private Vector2 _size;

		public MainScene(int width, int height) : base(width, height)
		{
			dragAction = objDrag;
			_prevSizes = new Dictionary<string, Vector2>();
			_prevPos = new Dictionary<string, Vector2>();
			_docked = new HashSet<string>();
			_absolute = new HashSet<string>();
			_menu = new ContextMenu();
			_menu.PreShow += _menu_PreShow;
			_menu.SelectOption += _menu_SelectOption;
			Add(_menu);

			Add(_background = new Panel() { Size = new Vector2(width, height), BackColor = SystemColors.Control });
			Add(_toolbox = new Toolbox() { Size = new Vector2(175, height) });
			_toolbox.GenerateTools();
			Add(_scene = new Panel()
			{
				Padding = Vector4.Zero,
				BorderWidth = 1,
				BorderColor = Color.DarkGray,
				Position = new Vector2(_toolbox.Width + 20, 40),
				Size = new Vector2(width - _toolbox.Width - 40, height - 60)
			});
			Add(_sceneNameLabel = new Label() { Text = "Scene name:", Position = new Vector2(_toolbox.Width + 20, 10) });
			Add(_sceneName = new TextBox() { Text = "Untitled", Position = _sceneNameLabel.Position + new Vector2(_sceneNameLabel.Width + 5, -2) });
			_sceneName.Size = new Vector2(Width - _sceneName.Position.X - 100, 28);
			Add(_sceneSaveButton = new Button() { Text = "Save", Position = _sceneName.Position + new Vector2(_sceneName.Width + 5, 2), Size = new Vector2(80, 22) });

			Add(_orientationButton = new Button() { Size = new Vector2(100, 20) });
			_orientationButton.Position = new Vector2((_toolbox.Size.X - _orientationButton.Size.X) / 2, Height - 40);
			if (width > height) _orientationButton.Text = "Portrait";
			else _orientationButton.Text = "Landscape";
			_orientationButton.MouseClick += _orientationButton_MouseClick;
			_sceneSaveButton.MouseClick += _sceneSaveButton_MouseClick;
			createSelectionCorners();

			_toolbox.ToolDrag += _toolbox_ToolDrag;
			_toolbox.DragEnd += _toolbox_DragEnd;
			MouseClick += MainScene_MouseClick;
			PreviewKeys = true;
			PreviewKeyPress += MainScene_PreviewKeyPress;
		}

		private void _orientationButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
		{
			if (Width > Height)
				Size = new Vector2(Height * Program.Ratio, Height);
			else
				Size = new Vector2(Height / Program.Ratio, Height);
			_background.Size = new Vector2(Width, Height);
			_toolbox.Size = new Vector2(175, Height);
			_scene.Position = new Vector2(_toolbox.Width + 20, 40);
			_scene.Size = new Vector2(Width - _toolbox.Width - 40, Height - 60);
			_sceneNameLabel.Position = new Vector2(_toolbox.Width + 20, 10);
			_sceneName.Position = _sceneNameLabel.Position + new Vector2(_sceneNameLabel.Width + 5, -2);
			_sceneName.Size = new Vector2(Width - _sceneName.Position.X - 100, 28);
			_sceneSaveButton.Position = _sceneName.Position + new Vector2(_sceneName.Width + 5, 2);
			_orientationButton.Position = new Vector2((_toolbox.Size.X - _orientationButton.Size.X) / 2, Height - 40);
			if (Width > Height) _orientationButton.Text = "Portrait";
			else _orientationButton.Text = "Landscape";

			Program.Manager.Resize((int)Width, (int)Height, false);
		}

        private void _menu_SelectOption(Drawable arg1, Drawable arg2, OpenTK.Input.MouseButton arg3)
        {
            if (arg2.Name == "Dock")
            {
                _docked.Add(arg1.Name);
                arg1.Draggable = false;
                _prevSizes[arg1.Name] = arg1.Size;
                _prevPos[arg1.Name] = arg1.Position;
                var parent = arg1.Parent;
                arg1.Size = parent.Size - 2 * parent.Padding.Xy;
                arg1.Position = new Vector2(0, 0);
            }
            else if (arg2.Name == "Undock")
            {
                arg1.Draggable = true;
                arg1.Size = _prevSizes[arg1.Name];
                arg1.Position = _prevPos[arg1.Name];
                _docked.Remove(arg1.Name);
            }
            else if (arg2.Name == "Absolute")
            {
                _absolute.Add(arg1.Name);
            }
            else if (arg2.Name == "Relative")
            {
                _absolute.Remove(arg1.Name);
            }
            else if (arg2.Name == "Golden height")
            {
                var parent = arg1.Parent;
                if (parent != null)
                {
                    arg1.Size = new Vector2(arg1.Width, arg1.Width * gRatio);
                }
            }
            else if (arg2.Name == "Golden width")
            {
                var parent = arg1.Parent;
                if (parent != null)
                {
                    arg1.Size = new Vector2(arg1.Width, arg1.Width / gRatio);
                }
            }
            else if (arg2.Name == "Center")
            {
                var parent = arg1.Parent;
                if (parent != null)
                {
                    arg1.Position = (parent.Size - arg1.Size) / 2;
                }
            }
            else if (arg2.Name == "Center vertically")
            {
                var parent = arg1.Parent;
                if (parent != null)
                {
                    parent.CenterVertically(arg1);
                }
            }
            else if (arg2.Name == "Center hortizontally")
            {
                var parent = arg1.Parent;
                if (parent != null)
                {
                    parent.CenterHorizontally(arg1);
                }
            }
            else if (arg2.Name == "Bring to front")
            {
                var parent = arg1.Parent;
                if (parent != null && parent is IContainer)
                {
                    (parent as IContainer).BringToFront(arg1);
                }
            }
            else if (arg2.Name == "Send to back")
            {
                var parent = arg1.Parent;
                if (parent != null && parent is IContainer)
                {
                    (parent as IContainer).SendToBack(arg1);
                }
            }
            else if (arg2.Name == "Delete")
            {
                var parent = arg1.Parent;
                if (parent != null && parent is IContainer)
                    ((IContainer)parent).Remove(arg1);
                _names.Remove(arg1.Name);
                if (arg1 is IContainer)
                    removeElements(arg1 as IContainer);
                _selectedElement = null;
                setCornerPosition();
            }
            else if (arg2.Name == "Copy size")
            {
                _size = arg1.Size;
            }
            else if (arg2.Name == "Paste size")
            {
                arg1.Size = _size;
            }
        }

        private void _menu_PreShow(Drawable arg1, Drawable arg2)
        {
            _menu.Clear();

            _menu.Option("Delete");

            _menu.Separate();

            if (_docked.Contains(arg2.Name)) _menu.Option("Undock");
            else _menu.Option("Dock");

            if (_absolute.Contains(arg2.Name)) _menu.Option("Relative");
            else _menu.Option("Absolute");

            if (!_docked.Contains(arg2.Name))
            {
                _menu.Separate();

                _menu.Option("Golden height");
                _menu.Option("Golden width");

                _menu.Separate();

                _menu.Option("Center");
                _menu.Option("Center vertically");
                _menu.Option("Center hortizontally");
            }
            _menu.Separate();

            _menu.Option("Bring to front");
            _menu.Option("Send to back");

            _menu.Separate();

            _menu.Option("Copy size");
            _menu.Option("Paste size");
        }

        private void getObjects(Drawable current, List<Tuple<string, Type, Drawable>> types, bool includeFirst = false)
        {
            if (includeFirst && !string.IsNullOrEmpty(current.Name)) types.Add(Tuple.Create(current.Name, current.GetType(), current));
			if (current is IContainer  && !((IContainer)current).Sealed)
            {
                foreach(var obj in ((IContainer)current).Objects)
                    getObjects(obj, types, true);
            }
        }

        private void _sceneSaveButton_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
        {
            List<Tuple<string, Type, Drawable>> types = new List<Tuple<string, Type, Drawable>>();
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("using System;");
            builder.AppendLine("using GameEngine;");
            builder.AppendLine("using GameEngine.Graphics;");
            builder.AppendLine("using GameEngine.Graphics.Drawables;");
            builder.AppendLine("using OpenTK;");
            builder.AppendLine("");
            builder.AppendLine($"public class {_sceneName.Text} : GraphicsScene");
            builder.AppendLine("{");
            getObjects(_scene, types);
            foreach (var obj in types)
                builder.AppendLine($"    private {obj.Item2.ToString().Replace("GameEngine.Graphics.Drawables.", "")} {obj.Item1};");
            builder.AppendLine("");
            builder.AppendLine($"\tpublic {_sceneName.Text}(int width, int height) : base(width, height)");
            builder.AppendLine("\t{");
            foreach (var obj in types)
            {
                builder.AppendLine($"\t\t{obj.Item1} = new {obj.Item2.ToString().Replace("GameEngine.Graphics.Drawables.", "")}();");
                if (_absolute.Contains(obj.Item1))
                {
                    builder.AppendLine($"\t\t{obj.Item1}.Size = new Vector2({obj.Item3.Width}, {obj.Item3.Height});");
                    builder.AppendLine($"\t\t{obj.Item1}.Position = new Vector2({obj.Item3.Position.X}, {obj.Item3.Position.Y});");
                }
                else
                {
                    builder.AppendLine($"\t\t{obj.Item1}.Size = new Vector2(Width * {obj.Item3.Width / _scene.Width}f, Height * {obj.Item3.Height / _scene.Height}f);");
                    builder.AppendLine($"\t\t{obj.Item1}.Position = new Vector2(Width * {obj.Item3.Position.X / _scene.Width}f, Height * {obj.Item3.Position.Y / _scene.Height}f);");
                }
                if (obj.Item3.Parent != null && !string.IsNullOrEmpty((obj.Item3.Parent as Drawable).Name))
                    builder.AppendLine($"\t\t{(obj.Item3.Parent as Drawable).Name}.Add({obj.Item1});");
                else
                    builder.AppendLine($"\t\tAdd({obj.Item1});");
                builder.AppendLine("");
            }
            builder.AppendLine("\t}");
            builder.AppendLine("}");
            File.WriteAllText(_sceneName.Text + ".cs", builder.ToString());
        }

        private void removeElements(IContainer container)
        {
            foreach (var elem in container.Objects)
            {
                _names.Remove(elem.Name);
                if (elem is IContainer)
                    removeElements(elem as IContainer);
            }
        }

        private bool MainScene_PreviewKeyPress(GraphicsScene source, OpenTK.Input.Key key)
        {
            if (key == OpenTK.Input.Key.Delete || key == OpenTK.Input.Key.KeypadDecimal || key == OpenTK.Input.Key.BackSpace)
            {
                if(_selectedElement != null)
                {
                    var parent = _selectedElement.Parent;
                    if (parent != null && parent is IContainer)
                        ((IContainer)parent).Remove(_selectedElement);
                    _names.Remove(_selectedElement.Name);
                    if (_selectedElement is IContainer)
                        removeElements(_selectedElement as IContainer);
                     _selectedElement = null;
                    setCornerPosition();
                }
            }
            return false;
        }

        private void createSelectionCorners()
        {
            _sizeCorners = new Drawable[4]
            {
                new SizePanel() { Size = new Vector2(5,5), BackColor = Color.Blue, BorderColor = Color.Black, BorderWidth = 1, Draggable = true },
                new SizePanel() { Size = new Vector2(5,5), BackColor = Color.Blue, BorderColor = Color.Black, BorderWidth = 1, Draggable = true },
                new SizePanel() { Size = new Vector2(5,5), BackColor = Color.Blue, BorderColor = Color.Black, BorderWidth = 1, Draggable = true },
                new SizePanel() { Size = new Vector2(5,5), BackColor = Color.Blue, BorderColor = Color.Black, BorderWidth = 1, Draggable = true }
            };
            _sizeCorners[0].Drag += dragTopLeft;
            _sizeCorners[1].Drag += dragTopRight;
            _sizeCorners[2].Drag += dragBottomLeft;
            _sizeCorners[3].Drag += dragBottomRight;
            foreach (var corner in _sizeCorners)
            {
                _scene.Add(corner);
                corner.MouseEnter += (a, b) => { ((Panel)a).BackColor = Color.LightBlue; };
                corner.MouseLeave += (a, b) => { ((Panel)a).BackColor = Color.Blue; };
            }
            setCornerPosition();
        }

        private void dragBottomRight(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            arg1.ProcessDrag(arg2, arg3);
            if (_selectedElement != null)
            {
                var p = _selectedElement.Position;
                var s = _selectedElement.Size;
                var newS = (arg1.AbsolutePosition - _selectedElement.AbsolutePosition);
                newS = new Vector2(Math.Max(5, newS.X), Math.Max(5, newS.Y));
                _selectedElement.Size = newS;
                setCornerPosition();
            }
        }

        private void dragBottomLeft(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            arg1.ProcessDrag(arg2, arg3);
            if (_selectedElement != null)
            {
                var p = _selectedElement.Position;
                var s = _selectedElement.Size;
                var sDiff = (arg1.AbsolutePosition - _selectedElement.AbsolutePosition);
                var newP = p + new Vector2(sDiff.X, 0);
                var newS = new Vector2(s.X -sDiff.X, sDiff.Y);
                if (newS.X <= 5 || newS.Y <= 5 || s.X <= 5 || s.Y <= 5)
                    newS = new Vector2(Math.Max(5, newS.X), Math.Max(5, newS.Y));
                _selectedElement.Position = newP;
                _selectedElement.Size = newS;
                setCornerPosition();
            }
        }

        private void dragTopRight(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            arg1.ProcessDrag(arg2, arg3);
            if (_selectedElement != null)
            {
                var p = _selectedElement.Position;
                var s = _selectedElement.Size;
                var sDiff = (arg1.AbsolutePosition - _selectedElement.AbsolutePosition) - new Vector2(s.X, 0);
                var newP = p + new Vector2(0, sDiff.Y);
                var newS = s + new Vector2(sDiff.X, -sDiff.Y);
                if (newS.X <= 5 || newS.Y <= 5 || s.X <= 5 || s.Y <= 5)
                    newS = new Vector2(Math.Max(5, newS.X), Math.Max(5, newS.Y));
                _selectedElement.Position = newP;
                _selectedElement.Size = newS;
                setCornerPosition();
            }
        }

        private void dragTopLeft(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            arg1.ProcessDrag(arg2, arg3);
            if (_selectedElement != null)
            {
                var p = _selectedElement.Position;
                var s = _selectedElement.Size;
                var diff = (arg1.AbsolutePosition - _selectedElement.AbsolutePosition) + arg1.Size;
                var newP = _selectedElement.Position + diff;
                var newS = s - diff;

                if (newS.X <= 5 || newS.Y <= 5 || s.X <= 5 || s.Y <= 5)
                    newS = new Vector2(Math.Max(5, newS.X), Math.Max(5, newS.Y));
                _selectedElement.Position = newP;
                _selectedElement.Size = newS;
                setCornerPosition();
            }
        }

        private void setCornerPosition()
        {
            var cSize = _sizeCorners[0].Size;
            if (_selectedElement == null || (_selectedElement is Label && (_selectedElement as Label).AutoSize) || _docked.Contains(_selectedElement.Name))
            {
                foreach (var corner in _sizeCorners)
                    corner.Position = new Vector2(-100, -100);
            }
            else
            {
                var p = _selectedElement.AbsolutePosition - _scene.Position;
                var s = _selectedElement.Size;

                _sizeCorners[0].Position = p - cSize;
                _sizeCorners[1].Position = p + new Vector2(s.X, -cSize.Y);
                _sizeCorners[2].Position = p + new Vector2(-cSize.X, s.Y);
                _sizeCorners[3].Position = p + new Vector2(s.X, s.Y);
            }
        }

        private void MainScene_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
        {
            if (_selectedElement != null && !_selectedElement.ClientArea.Contains(arg2.X, arg2.Y))
            {
                _selectedElement = null;
                setCornerPosition();
            }
        }
        private void _toolbox_DragEnd(Tool obj)
        {
            if (_workingElement != null)
            {
                if (_workingElement.Position.X < 0 || _workingElement.Position.Y < 0)
                {
                    _scene.Remove(_workingElement);
                    _names.Remove(_workingElement.Name);
                }
                else
                {
                    _workingElement.MouseDown += _workingElement_MouseClick;
                    _workingElement.MouseClick += _workingElement_MouseClick;
                    _workingElement.DragStart += _workingElement_DragStart;
                    _workingElement.DragEnd += _workingElement_DragEnd;
                    _workingElement.ContextMenu = _menu;

					_workingContainer = _scene.PickContainer(_workingElement.Position + _scene.Position, _workingElement);
					var absolutePos = _workingElement.AbsolutePosition;
                    _scene.Remove(_workingElement);
					_workingElement.Position = absolutePos - (_workingContainer as Drawable).AbsolutePosition;
                    _workingContainer.Add(_workingElement);
                    foreach (var corner in _sizeCorners) _scene.BringToFront(corner);
                }
            }
            _workingElement = null;
        }

        private void _workingElement_DragEnd(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            _workingContainer = _scene.PickContainer(arg3, _selectedElement);
			var absolutePos = _selectedElement.AbsolutePosition;
            _scene.Remove(_selectedElement);
			_selectedElement.Position =  absolutePos - (_workingContainer as Drawable).AbsolutePosition;
            if (_workingContainer != _prevParent)
                _workingContainer.Add(_selectedElement);
            else
            {
                _prevParent.Objects.Insert(_prevIndex, _selectedElement);
				_selectedElement.Parent = (Container)_prevParent;
            }
        }

        private void _workingElement_DragStart(Drawable arg1, DragInformation arg2, Vector2 arg3)
        {
            var parent = _selectedElement.Parent as IContainer;
            var prev = _selectedElement.Position;
			_selectedElement.Position = _selectedElement.AbsolutePosition - _scene.AbsolutePosition;
             _prevParent = parent;
            _prevIndex = parent.Objects.FindIndex(p => p == _selectedElement);
            parent.Remove(_selectedElement);
            _scene.Add(_selectedElement);
            var newPos = _selectedElement.Position - prev;
            arg2.UpdateAbsoluteStart(arg2.AbsoluteMouseStart - newPos);
        }

        private void _workingElement_MouseClick(Drawable arg1, Vector2 arg2, OpenTK.Input.MouseButton arg3)
        {
            _selectedElement = arg1;
            setCornerPosition();
            foreach (var corner in _sizeCorners) _scene.BringToFront(corner);
        }

        private void _toolbox_ToolDrag(Tool arg1, DragInformation arg2, Vector2 arg3)
        {
            if (_workingElement == null)
            {
                _workingElement = arg1.Generate(_names);
                _workingElement.Draggable = true;
                _workingElement.Drag += dragAction;
                _scene.Add(_workingElement);
            }
            _workingElement.Position = arg3 + arg2.AbsoluteMouseStart - arg2.RelativeMouseStart - _scene.Position;
        }

        public override void Tick()
        {
            setCornerPosition();
            base.Tick();
        }
    }
}
