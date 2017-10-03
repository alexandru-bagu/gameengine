using System;
using OpenTK;
using System.Collections.Generic;
using System.Drawing;

namespace GameEngine.Collision
{
    public class Polygon
    {
        private Vector2[] _edges, _points;
        private Vector2 _center;
        private RectangleF _boundingRectangle;

        public ICollider Parent { get; set; }
        public Vector2 Velocity { get; set; }
        public float Mass { get; set; } = 1f;
        public int MovementSamples { get; set; } = 8;
        public bool Static { get; set; } = false;
        public bool Reflect { get; set; } = true;

        public Vector2[] Points => _points;
        public Vector2 Center => _center;

        public float Speed
        {
            get { return Velocity.LengthSquared; }
            set { Velocity = Velocity.Normalized() * (float)Math.Sqrt(value); }
        }

        public Polygon(ICollider parent, int movementSamples, float mass, params Vector2[] points)
        {
            Parent = parent;
            MovementSamples = movementSamples;
            Mass = mass;
            Velocity = new Vector2(0, 0);

            buildPolygon(points);
            generateBoundingRectangle(points);
        }

        private void generateBoundingRectangle(Vector2[] points)
        {
            float left = 0, top = 0, right = 0, bottom = 0;
            foreach (var point in points)
            {
                if (point.X < left) left = point.X;
                if (point.X > right) right = point.X;
                if (point.Y < top) top = point.Y;
                if (point.Y > bottom) bottom = point.Y;
            }
            _boundingRectangle = new RectangleF(left, top, right - left, bottom - top);
        }

        protected void buildPolygon(Vector2[] points)
        {
            generateBoundingRectangle(points);
            _points = points;
            _edges = computeEdges();
            _center = computeCenter();
        }

        private Vector2[] computeEdges()
        {
            List<Vector2> edges = new List<Vector2>();
            Vector2 p1, p2;
            for (int i = 0; i < _points.Length; i++)
            {
                p1 = _points[i];
                if (i + 1 >= _points.Length) p2 = _points[0];
                else p2 = _points[i + 1];
                edges.Add(p2 - p1);
            }
            return  edges.ToArray();
        }

        private Vector2 computeCenter()
        {
            float totalX = 0;
            float totalY = 0;
            for (int i = 0; i < Points.Length; i++)
            {
                totalX += Points[i].X;
                totalY += Points[i].Y;
            }
            return new Vector2(totalX / _points.Length, totalY / _points.Length);
        }

        public virtual bool ClipThrough(Polygon polygon)
        {
            return false;
        }

        public bool Collision(Polygon polygon)
        {
            return Collision(polygon, Vector2.Zero).Intersect;
        }

        //https://www.codeproject.com/Articles/15573/D-Polygon-Collision-Detection
        // Check if polygon A is going to collide with polygon B for the given velocity
        public virtual CollisionResult Collision(Polygon polygon, Vector2 velocity)
        {
            Polygon currentPolygon = this;
            CollisionResult result = new CollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = currentPolygon._edges.Length;
            int edgeCountB = polygon._edges.Length;
            float minIntervalDistance = float.PositiveInfinity;
            Vector2 translationAxis = new Vector2();
            Vector2 edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = currentPolygon._edges[edgeIndex];
                }
                else
                {
                    edge = polygon._edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, currentPolygon, ref minA, ref maxA);
                ProjectPolygon(axis, polygon, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                float velocityProjection = axis.Dot(velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector2 d = currentPolygon.Center - polygon.Center;
                    if (d.Dot(translationAxis) < 0) translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * minIntervalDistance;
            result.TranslationAxis = translationAxis;

            return result;
        }

        public void Offset(Vector2 offset, bool offsetParent = true)
        {
            for (int i = 0; i < _points.Length; i++)
                _points[i] = _points[i] + offset;
            _center += offset;
            _boundingRectangle.Offset(new PointF(offset.X, offset.Y));
            if (Parent != null && offsetParent) Parent.Position += offset;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
                return minB - maxA;
            else
                return minA - maxB;
        }

        public virtual bool BoundIntersect(Polygon target, Vector2 velocity)
        {
            var bound = _boundingRectangle;
            bound.Offset(new PointF(velocity.X, velocity.Y));
            return target._boundingRectangle.IntersectsWith(bound);
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public void ProjectPolygon(Vector2 axis, Polygon polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = axis.Dot(polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Length; i++)
            {
                d = polygon.Points[i].Dot(axis);
                if (d < min)
                    min = d;
                else if (d > max)
                    max = d;
            }
        }
    }
}
