using OpenTK;
using System;
using System.Drawing;

namespace GameEngine.Graphics.Drawables
{
    public class Line : Drawable
    {
        private Vector2[] _polygon;
        private Vector2 _point1, _point2;
        private float _lineWidth;
        
        public Vector2 Point1 { get { return _point1; } set { Position = _point1 = value; generatePolygon(); } }
        public Vector2 Point2 { get { return _point2; } set { _point2 = value; generatePolygon(); } }

		public Color Color { get; set; } = Color.White;
        public float LineWidth { get { return _lineWidth; } set { _lineWidth = value; generatePolygon(); } }

        public Line()
        {
            _lineWidth = 1;
            _polygon = new Vector2[0];
        }

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			_point1 = Position;
			generatePolygon();
		}

        protected override void Draw()
        {
            if (Texture == null)
                Painter.Quad(_polygon, Color);
            else
                Painter.TextureQuad(Texture, _polygon, Color);
        }

        public override bool Pick(Vector2 position)
        {
            return Helper.IsPointInPolygon4(_polygon, position);
        }

        public override void ProcessDrag(DragInformation information, Vector2 position)
        {
            var diff = _point2 - _point1;
            _point1 = information.ReferencePoint - information.RelativeMouseStart + position;
            _point2 = _point1 + diff;
            Position = _point1;
            generatePolygon();
        }

		private void generatePolygon()
		{
			Size = new Vector2(_point1.Distance(_point2), LineWidth);
			_polygon = Helper.PolygonFromLine(AbsolutePosition, _point2 + AbsolutePosition - _point1, LineWidth);
		}

        public override void Tick()
        {

        }
    }
}
