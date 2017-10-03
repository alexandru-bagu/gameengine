using OpenTK;
using System;
using System.Collections.Generic;

namespace GameEngine.Collision
{
    public class CircleShape : Polygon
    {
        public Vector2 Position { get; private set; }
        public float Radius { get; private set; }

        public CircleShape(ICollider parent, float x, float y, float radius, int segments = 30) : this(parent, new Vector2(x, y), radius, segments) { }
        public CircleShape(ICollider parent, Vector2 center, float radius, int segments = 30) : base(parent, 8, 1)
        {
            Position = center;
            Radius = radius;

            buildPolygon(buildPolygonFromCircle(segments));
        }

        private Vector2[] buildPolygonFromCircle(int segments = 30)
        {
            List<Vector2> vertices = new List<Vector2>();
            if (segments < 3) throw new ArgumentException("segments must be bigger or equal to 3");
            var p = Position;
            var step = 2 * Math.PI / segments;
            var value = Math.PI / 4;
            for (int i = 0; i < segments; i++)
            {
                var cos = (float)Math.Cos(value);
                var sin = (float)Math.Sin(value);
                vertices.Add(new Vector2(p.X + cos * Radius, p.Y + sin * Radius));
                value += step;
            }
            return vertices.ToArray();
        }

		public override CollisionResult Collision(Polygon polygon, Vector2 velocity)
		{
			if (polygon is CircleShape)
			{
				CollisionResult result = new CollisionResult();

				var circle1 = this;
				var circle2 = (CircleShape)polygon;
				var radii = circle1.Radius + circle2.Radius;
				var d = Center - polygon.Center;
				var axis = d.Normalized();
				if (d.Dot(axis) < 0) axis = -axis;
				var distance = d.LengthSquared;
				var minimumDistance = 0.0f;

				if (distance <= radii * radii)
				{
					result.Intersect = true;
					minimumDistance = radii - d.Length;
				}

				var newCenter = Center + velocity;
				d = newCenter - polygon.Center;
				axis = d.Normalized();
				if (d.Dot(axis) < 0) axis = -axis;
				distance = d.LengthSquared;
				if (distance <= radii * radii)
				{
					result.WillIntersect = true;
					minimumDistance = radii - d.Length;
				}
				

				result.TranslationAxis = axis;
				result.MinimumTranslationVector = axis * minimumDistance;

				return result;
			}
			return base.Collision(polygon, velocity);
		}
    }
}
