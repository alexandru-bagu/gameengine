using GameEngine.Collections;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Collision
{
    public class CollisionPlane
    {
        private ReadOptimizedList<Polygon> _polygons;
        private float _friction, _elasticity;
        private int _maximumIterations;
        private float _minimumSpeed, _maximumSpeed, _maximumSpeedSqrt;
        private Vector2 _gravityTest;

        public float MaximumSpeed => _maximumSpeed;
        public float MinimumSpeed => _minimumSpeed;
        public Vector2 Gravity { get; set; } = Vector2.Zero;
        public IEnumerable<Polygon> Objects => _polygons.Array;

        public CollisionPlane(int maxCollisionIterations, float friction, float elasticity, float minimumSpeed = 0, float maximumSpeed = 100f)
        {
            _polygons = new ReadOptimizedList<Polygon>();
            _friction = friction;
            _elasticity = elasticity;
            if (maxCollisionIterations < 1) maxCollisionIterations = 1;
            _maximumIterations = maxCollisionIterations;
            _minimumSpeed = minimumSpeed * minimumSpeed;
            _maximumSpeedSqrt = maximumSpeed;
            _maximumSpeed = maximumSpeed * maximumSpeed;
        }

        public CollisionPlane(int maxCollisionIterations, float friction, float elasticity, Vector2 gravity, float minimumSpeed = 0, float maximumSpeed = 100f) : this(maxCollisionIterations, friction, elasticity, minimumSpeed, maximumSpeed)
        {
            Gravity = gravity;
            _gravityTest = gravity * 16;//2 * sample rate
        }

        public void Add(params Polygon[] polygon) { _polygons.AddRange(polygon); }
        public void Add(IEnumerable<Polygon> polygons) { _polygons.AddRange(polygons); }
        public void Remove(Polygon polygon) { _polygons.Remove(polygon); }

        public virtual Vector2 Reflect(Vector2 velocity, Vector2 normal)
        {
            var Vn = velocity.Dot(normal) * normal;
            var Vt = velocity - Vn;

            var newVelocity = Vt * (1 - _friction) + Vn * -(_elasticity);

            var speed = newVelocity.LengthSquared;
            if (speed > _maximumSpeed) return newVelocity.Normalized() * _maximumSpeedSqrt;
            else if (speed > _minimumSpeed) return newVelocity;
            else return Vt + Vn * -1; //no friction/elasticity
        }

        public IEnumerable<Polygon> TickFreeMovement(Polygon polygon)
        {
            return TickFreeMovement(polygon, _maximumIterations);
        }

        protected IEnumerable<Polygon> TickFreeMovement(Polygon polygon, int iterationOverride)
        {
            List<Polygon> polygons = new List<Polygon>(_polygons.Count / 3);
            freeMovement(polygon, iterationOverride, polygons);
            return polygons;
        }

        private void freeMovement(Polygon polygon, int iteration, List<Polygon> list)
        {
            if (iteration <= 0) return;
            var velocity = polygon.Velocity;
            int samples = polygon.MovementSamples;
            var velSplit = velocity / samples;

            for (int i = 0; i < samples; i++)
            {
                foreach (var obj in _polygons)
                {
                    if (obj != polygon && !polygon.ClipThrough(obj) && polygon.BoundIntersect(obj, polygon.Velocity))
                    {
                        var res = polygon.Collision(obj, velSplit);
                        if (res.Intersect && res.WillIntersect && res.MinimumTranslationVector.LengthSquared >= 0.01f)
                        {
                            if (polygon.Static)
                            {
                                obj.Offset(-res.MinimumTranslationVector);

                                if (obj.Static && obj.Velocity == Vector2.Zero)
                                {
                                    polygon.Offset(res.MinimumTranslationVector);
                                }
                                else
                                {
                                    if (obj.Reflect)
                                        obj.Velocity = Reflect(obj.Velocity - velocity, res.TranslationAxis);
                                }
                            }
                            else
                            {
                                polygon.Offset(res.MinimumTranslationVector);

                                if (obj.Static)
                                {
                                    if (polygon.Reflect)
                                        polygon.Velocity = Reflect(velocity - obj.Velocity, res.TranslationAxis);
                                }
                                else
                                {
                                    if (polygon.Reflect && obj.Reflect)
                                    {
                                        var V = Reflect(velocity - obj.Velocity, res.TranslationAxis);
                                        polygon.Velocity += V * (polygon.Mass) / (polygon.Mass + obj.Mass);
                                        obj.Velocity -= V * (obj.Mass) / (polygon.Mass + obj.Mass);
                                    }
                                    else if (polygon.Reflect)
                                    {
                                        var V = Reflect(velocity - obj.Velocity, res.TranslationAxis);
                                        polygon.Velocity += V * (polygon.Mass) / (polygon.Mass + obj.Mass);
                                    }
                                    else if (obj.Reflect)
                                    {
                                        var V = Reflect(velocity - obj.Velocity, res.TranslationAxis);
                                        obj.Velocity -= V * (obj.Mass) / (polygon.Mass + obj.Mass);
                                    }
                                }
                            }

                            list.Add(obj);
                            freeMovement(polygon, iteration - i - 1, list);
                            return;
                        }
                    }
                }
                polygon.Offset(velSplit);
            }
        }
        
        public IEnumerable<Polygon> TestMovement(Polygon polygon, Vector2 velocity)
        {
            return TestMovement(polygon, velocity, _maximumIterations);
        }

        protected IEnumerable<Polygon> TestMovement(Polygon polygon, Vector2 velocity, int iterationOverride)
        {
            List<Polygon> polygons = new List<Polygon>(_polygons.Count / 3);
            polygon.Offset(-testMovement(polygon, velocity, iterationOverride, polygons));
            return polygons;
        }

        private Vector2 testMovement(Polygon polygon, Vector2 velocity, int iteration, List<Polygon> list)
        {
            Vector2 offset = Vector2.Zero;
            if (iteration <= 0) return offset;
            int samples = Math.Max(8, polygon.MovementSamples);
            var velSplit = velocity / samples;

            for (int i = 0; i < samples; i++)
            {
                foreach (var obj in _polygons)
                {
                    if (obj != polygon && !polygon.ClipThrough(obj) && polygon.BoundIntersect(obj, polygon.Velocity))
                    {
                        var res = polygon.Collision(obj, velSplit);
                        if (res.Intersect && res.WillIntersect && res.MinimumTranslationVector.LengthSquared >= 0.01f)
                        {
                            if (!polygon.Static)
                            {
                                offset += res.MinimumTranslationVector;
                                polygon.Offset(res.MinimumTranslationVector);
                            }
                            list.Add(obj);
                            return offset;
                        }
                    }
                }
                offset += velSplit;
                polygon.Offset(velSplit);
            }
            return offset;
        }


        public List<Polygon> TickMovement(Polygon polygon, Vector2 newPosition)
        {
            List<Polygon> polygons = new List<Polygon>(_polygons.Count / 3);
            var delta = newPosition - polygon.Center;
            int samples = polygon.MovementSamples;
            var dstSplit = delta / samples;

            for (int i = 0; i < samples; i++)
            {
                foreach (var target in _polygons)
                {
                    if (!target.Static && !polygon.ClipThrough(target) && polygon.BoundIntersect(target, delta))
                    {
                        var res = target.Collision(polygon, dstSplit);
                        if (res.Intersect && res.WillIntersect && res.MinimumTranslationVector.LengthSquared >= 0.01f)
                        {
                            var velocity = polygon.Velocity - target.Velocity;
                            target.Offset(res.MinimumTranslationVector);
                            target.Velocity = Reflect(velocity, res.TranslationAxis);
                            polygons.Add(target);
                        }
                    }
                }
                polygon.Offset(dstSplit);
            }
            return polygons;
        }

        public void TickGravity()
        {
            foreach (var obj in _polygons)
            {
                if (!obj.Static)
                {
					var floating = TestMovement(obj, Gravity * obj.Mass).Count() == 0;
                    if (floating) obj.Velocity += Gravity * obj.Mass;
                }
            }
        }
    }
}