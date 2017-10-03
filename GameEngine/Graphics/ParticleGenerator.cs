using System;
using System.Collections.Generic;
using System.Drawing;
using GameEngine.Assets;
using GameEngine.Graphics.Drawables;
using OpenTK;

namespace GameEngine.Graphics
{
    public class ParticleGenerator : Drawable
    {
        private List<List<Particle>> _generations;
        private Random _random;
        public Random RandomGenerator => _random;

        public ParticleGenerator()
        {
            _random = new Random();
            _generations = new List<List<Particle>>();
        }

        public float Radians(float degrees)
        {
            return (float)(Math.PI * degrees / 180.0);
        }

        public float Degrees(float radians)
        {
            return (float)(radians * (180.0 / Math.PI));
        }

        public float Angle(Vector2 rs, Vector2 aim)
        {
            return Degrees((float)Math.Atan2(aim.Y - rs.Y, aim.X - rs.X));
        }

        public float RandomF(float min, float max)
        {
            var value = _random.Next((int)(min * 1000), (int)(max * 1000));
            return value / 1000f;
        }

        public int Random(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Generates particles
        /// </summary>
        /// <param name="amount">Number of particles</param>
        /// <param name="asset">Particle texture</param>
        /// <param name="baseColor">Particle texture color multiplier</param>
        /// <param name="timeToLive">Time in milliseconds</param>
        /// <param name="timeToLiveRandomness">Random(-ttlr/2,ttlr/2)</param>
        /// <param name="opacityMax">Value between 0-255</param>
        /// <param name="position">Absolute location</param>
        /// <param name="size">Size</param>
        /// <param name="sizeRandomnes">Size random multplier Random(-sr/2,sr/2)</param>
        /// <param name="angleDegrees">Angle for direction in degrees</param>
        /// <param name="angleRandomness">Direction ranomness Random(-ar/2,ar/2)</param>
        /// <param name="speed">Particle speed</param>
        /// <param name="speedRandomness">Speed random Random(-sr/2,sr/2)</param>
        /// <param name="distance">Particle initial distance from given position</param>
        /// <param name="distanceRandomness">Distance random Random(-dr/2,dr/2)</param>
        public void Generate(int amount, TextureAsset asset, Func<Color> baseColor,
                             int timeToLive, int timeToLiveRandomness,
                             int opacityMax,
                             Vector2 position,
                             Vector2 size, float sizeRandomnes,
                             float angleDegrees, float angleRandomness,
                             float speed, float speedRandomness,
                             float distance, float distanceRandomness)
        {
            List<Particle> generation = new List<Particle>(amount);
            for (int i = 0; i < amount; i++)
            {
                var direction = angleDegrees + RandomF(-angleRandomness / 2, angleRandomness / 2);
                var radians = Radians(direction);
                var velocityNormal = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                var velocity = velocityNormal * (speed + RandomF(-speedRandomness / 2, speedRandomness / 2));
                Particle particle = new Particle(timeToLive + Random(-timeToLiveRandomness / 2, timeToLiveRandomness / 2),
                                                 opacityMax, velocity);
                particle.BackColor = baseColor();
                particle.Texture = asset;
                particle.Position = AbsolutePosition + position + velocityNormal * (distance + RandomF(-distanceRandomness / 2, distanceRandomness / 2));
                particle.Size = size + Vector2.One * RandomF(-sizeRandomnes / 2, sizeRandomnes / 2);
                generation.Add(particle);
            }
            _generations.Add(generation);
        }

        /// <summary>
        /// Generates particles
        /// </summary>
        /// <param name="amount">Number of particles</param>
        /// <param name="asset">Particle texture</param>
        /// <param name="baseColor">Particle texture color multiplier</param>
        /// <param name="timeToLive">Time in milliseconds</param>
        /// <param name="timeToLiveRandomness">Random(-ttlr/2,ttlr/2)</param>
        /// <param name="opacityMax">Value between 0-255</param>
        /// <param name="position">Absolute location</param>
        /// <param name="size">Size</param>
        /// <param name="sizeRandomnes">Size random multplier Random(-sr/2,sr/2)</param>
        /// <param name="angleDegrees">Angle for direction in degrees</param>
        /// <param name="angleRandomness">Direction ranomness Random(-ar/2,ar/2)</param>
        /// <param name="speed">Particle speed</param>
        /// <param name="speedRandomness">Speed random Random(-sr/2,sr/2)</param>
        /// <param name="distance">Particle initial distance from given position</param>
        /// <param name="distanceRandomness">Distance random Random(-dr/2,dr/2)</param>
        /// <param name="scaleMin">Texture scaling start, interpolating during lifetime towards scaleMax</param>
        /// <param name="scaleMax">Texture scaling maximum</param>
        /// <param name="rotationMin">Texture rotation start, interpolating during lifetime towards rotationMax</param>
        /// <param name="rotationMax">Texture rotation maximum</param>
        public void Generate(int amount, TextureAsset asset, Func<Color> baseColor,
                                 int timeToLive, int timeToLiveRandomness,
                                 int opacityMax,
                                 Vector2 position,
                                 Vector2 size, float sizeRandomnes,
                                 float angleDegrees, float angleRandomness,
                                 float speed, float speedRandomness,
                                 float distance, float distanceRandomness,
                                 float scaleMin, float scaleMax,
                                 float rotationMin, float rotationMax)
        {
            List<Particle> generation = new List<Particle>(amount);
            for (int i = 0; i < amount; i++)
            {
                var direction = angleDegrees + RandomF(-angleRandomness / 2, angleRandomness / 2);
                var radians = Radians(direction);
                var velocityNormal = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                var velocity = velocityNormal * (speed + RandomF(-speedRandomness / 2, speedRandomness / 2));
                Particle particle = new Particle(timeToLive + Random(-timeToLiveRandomness / 2, timeToLiveRandomness / 2),
                                                 opacityMax, velocity);
                particle.BackColor = baseColor();
                particle.Texture = asset;
                particle.Position = AbsolutePosition + position + velocityNormal * (distance + RandomF(-distanceRandomness / 2, distanceRandomness / 2));
                particle.Size = size + Vector2.One * RandomF(-sizeRandomnes / 2, sizeRandomnes / 2);
                particle.EnableScaling(scaleMin, scaleMax);
                particle.EnableRotation(rotationMin, rotationMax);
                generation.Add(particle);
            }
            _generations.Add(generation);
        }

        protected override void Draw()
        {
            List<List<Particle>> _expiredGenerations = new List<List<Particle>>();
            foreach (var generation in _generations)
            {
                bool allExpired = true;

                foreach (var particle in generation)
                {
                    if (!particle.Expired)
                    {
                        allExpired = false;
                        particle.InvokeDraw();
                    }
                }

                if (allExpired)
                    _expiredGenerations.Add(generation);
            }

            foreach (var generation in _expiredGenerations)
                _generations.Remove(generation);
        }

        public override void Tick()
        {
            List<List<Particle>> _expiredGenerations = new List<List<Particle>>();
            foreach (var generation in _generations)
            {
                bool allExpired = true;

                foreach (var particle in generation)
                {
                    if (!particle.Expired)
                    {
                        allExpired = false;
                        particle.Tick();
                    }
                }

                if (allExpired)
                    _expiredGenerations.Add(generation);
            }

            foreach (var generation in _expiredGenerations)
                _generations.Remove(generation);
        }
    }
}
