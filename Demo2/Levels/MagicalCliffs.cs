using System.Collections.Generic;
using GameEngine.Assets;
using GameEngine.Graphics.Drawables;
using OpenTK;
using System;
using System.Linq;
using GameEngine.Graphics;
using System.Drawing;

namespace Demo2.Levels
{
    public class MagicalCliffs : Level
    {
        private TextureAsset _seaAsset, _cloudsAsset, _farGroundsAsset, _skyAsset, _tilesetAsset;
        private TextureAsset _platformStartAsset, _platformMiddleAsset, _platformEndAsset;
        private TextureAsset _bridgeStartAsset, _bridgeMiddleAsset, _bridgeEndAsset;
        private List<TextureAsset> _bushes;
        private List<int> _bushHeightOffsets;
        private List<RectangleF> _platformBlocks;

        private List<Texture> _skies, _seas, _clouds, _platforms, _bridges;
        private Texture _farGrounds;
        private float _cloudWidthSpan;

        public override IEnumerable<Drawable> Platforms
        {
            get
            {
                foreach (var platform in _platforms) yield return platform;
                foreach (var bidge in _bridges) yield return bidge;
            }
        }

        public override IEnumerable<RectangleF> PlatformBlocks => _platformBlocks;

        public MagicalCliffs()
        {
            _platformBlocks = new List<RectangleF>();
            _seaAsset = TextureAsset.LoadRelativePath("MagicalCliffs", "sea.png");
            _seaAsset.Crop(1, 0, _seaAsset.Width - 2, _seaAsset.Height);
            _cloudsAsset = TextureAsset.LoadRelativePath("MagicalCliffs", "clouds.png");
            _cloudsAsset.Crop(1, 0, _cloudsAsset.Width - 2, _cloudsAsset.Height);
            _farGroundsAsset = TextureAsset.LoadRelativePath("MagicalCliffs", "far-grounds.png");
            _farGroundsAsset.Crop(0, 0, _farGroundsAsset.Width - 1, _farGroundsAsset.Height);
            _skyAsset = TextureAsset.LoadRelativePath("MagicalCliffs", "sky.png");
            _skyAsset.Crop(1, 0, _skyAsset.Width - 2, _skyAsset.Height);
            _tilesetAsset = TextureAsset.LoadRelativePath("MagicalCliffs", "tileset.png");

            _platformStartAsset = _tilesetAsset.Copy().Crop(325, 43, 58, 97);
            _platformMiddleAsset = _tilesetAsset.Copy().Crop(401, 43, 14, 96);
            _platformEndAsset = _tilesetAsset.Copy().Crop(433, 43, 58, 97);

            _bridgeStartAsset = _tilesetAsset.Copy().Crop(583, 188, 41, 30);
            _bridgeMiddleAsset = _tilesetAsset.Copy().Crop(640, 187, 32, 31);
            _bridgeEndAsset = _tilesetAsset.Copy().Crop(688, 188, 41, 30);

            _bushes = new List<TextureAsset>()
            {
                _tilesetAsset.Copy().Crop(144, 105, 32, 55),
                _tilesetAsset.Copy().Crop(194, 185, 46, 38),
                _tilesetAsset.Copy().Crop(258, 180, 30, 44),
                _tilesetAsset.Copy().Crop(308, 186, 30, 22),
                _tilesetAsset.Copy().Crop(355, 183, 14, 25),
                _tilesetAsset.Copy().Crop(385, 187, 30, 21),
                _tilesetAsset.Copy().Crop(432, 166, 31, 42),
            };
            _bushHeightOffsets = new List<int>() {
                18, 4, 10, 2, 5, 1, 19
            };
        }

        public override void Build(float width, float height, Vector2 viewportSize, Vector2 viewportOffset, float xRatio = 1, float yRatio = 1)
        {
            _width = width;
            _height = height;

            _skies = new List<Texture>();
            _seas = new List<Texture>();
            _clouds = new List<Texture>();
            _platforms = new List<Texture>();
            _bridges = new List<Texture>();

            for (float x = 0; x < width; x += _skyAsset.Width * xRatio)
                _skies.Add(new Texture(_skyAsset)
                {
                    Offset = viewportOffset,
                    Position = new Vector2(x, 0),
                    Size = new Vector2(_skyAsset.Width * xRatio, height - _seaAsset.Height)
                });

            for (float x = 0; x < width; x += _seaAsset.Width * xRatio)
                _seas.Add(new Texture(_seaAsset)
                {
                    Offset = viewportOffset,
                    Position = new Vector2(x, height - _seaAsset.Height),
                    Size = new Vector2(_seaAsset.Width * xRatio, _seaAsset.Height * yRatio)
                });
            for (float x = -_cloudsAsset.CropWidth * xRatio; x <= width; x += _cloudsAsset.CropWidth * xRatio)
                _clouds.Add(new Texture(_cloudsAsset)
                {
                    Offset = viewportOffset,
                    Position = new Vector2(x, height - _seaAsset.CropHeight - _cloudsAsset.CropHeight),
                    Size = new Vector2(_cloudsAsset.CropWidth * xRatio, _cloudsAsset.CropHeight * yRatio),
                    BackColor = Color.FromArgb(140, Color.White)
                });
            _cloudWidthSpan = _clouds.Count * _cloudsAsset.CropWidth - _cloudsAsset.CropWidth;

            _farGrounds = new Texture(_farGroundsAsset)
            {
                Offset = viewportOffset,
                Position = new Vector2((viewportSize.X - (_farGroundsAsset.Width * xRatio)), height - _seaAsset.Height - _farGroundsAsset.CropHeight + 0),
                Size = new Vector2(_farGroundsAsset.Width * xRatio, _farGroundsAsset.Height * yRatio)
            };

            createPlatform(xRatio, yRatio, viewportOffset);
            createHigherPlatform(xRatio, yRatio, viewportOffset);

            makePlatformBlock(_platforms.First(p => p.Texture == _platformStartAsset), _platforms.First(p => p.Texture == _platformEndAsset), viewportOffset);
        }

        private void makePlatformBlock(Drawable start, Drawable end, Vector2 viewportOffset)
        {
            RectangleF rect = new RectangleF(start.Position.X, start.Position.Y, end.Position.X - start.Position.X + end.Width, end.Position.Y - start.Position.Y + end.Height);
            rect.Offset(new PointF(viewportOffset.X, viewportOffset.Y));
            _platformBlocks.Add(rect);
        }

        private void createPlatform(float xRatio, float yRatio, Vector2 viewportOffset)
        {
            Random rand = new Random();
            var platformStart = 4 * _platformStartAsset.CropWidth * xRatio;
            var platformEnd = Width - 4 * _platformEndAsset.CropWidth * xRatio;
            var platformHeight = Height - _platformStartAsset.CropHeight / 2 * yRatio;

            _platforms.Add(new Texture(_platformStartAsset)
            {
                Offset = viewportOffset,
                Position = new Vector2(platformStart - _platformStartAsset.CropWidth * xRatio, platformHeight),
                Size = new Vector2(_platformStartAsset.CropWidth * xRatio, _platformStartAsset.CropHeight * yRatio)
            });
            for (; platformStart < platformEnd; platformStart += _platformMiddleAsset.CropWidth * xRatio)
            {
                _platforms.Add(new Texture(_platformMiddleAsset)
                {
                    Offset = viewportOffset,
                    Position = new Vector2(platformStart, platformHeight),
                    Size = new Vector2(_platformMiddleAsset.CropWidth * xRatio, _platformMiddleAsset.CropHeight * yRatio)
                });
            }
            _platforms.Add(new Texture(_platformEndAsset)
            {
                Offset = viewportOffset,
                Position = new Vector2(platformStart, platformHeight),
                Size = new Vector2(_platformEndAsset.CropWidth * xRatio, _platformEndAsset.CropHeight * yRatio)
            });

            platformStart = 3 * _platformStartAsset.CropWidth * xRatio;
            platformEnd = Width - 4 * _platformEndAsset.CropWidth * xRatio;
            for (; platformStart < platformEnd;)
            {
                var choice = rand.Next(_bushes.Count);
                choice = 1;
                var bush = _bushes[choice];
                if (platformStart + bush.CropWidth > platformEnd) break;
                platformStart += bush.CropWidth;
                if (rand.Next(100) >= 40)
                {
                    _platforms.Add(new Texture(bush)
                    {
                        Offset = viewportOffset,
                        Position = new Vector2(platformStart, platformHeight - _bushHeightOffsets[choice]),
                        Size = new Vector2(bush.CropWidth * xRatio, bush.CropHeight * yRatio),
                        Tag = new Vector2(0, 4)
                    });
                }
            }
        }

        private void createHigherPlatform(float xRatio, float yRatio, Vector2 viewportOffset)
        {
            var startPlatform = _platforms.First(p => p.Texture == _platformStartAsset);
            var endPlatform = _platforms.Last(p => p.Texture == _platformEndAsset);
            float x = startPlatform.Position.X + startPlatform.Size.X * 2;
            float y = startPlatform.Position.Y - startPlatform.Size.Y - startPlatform.Size.Y / 4;

            addBridge(x, y, 5, xRatio, yRatio, viewportOffset);
            makePlatformBlock(_bridges.Last(p => p.Texture == _bridgeStartAsset), _bridges.Last(p => p.Texture == _bridgeEndAsset), viewportOffset);
            var size = sizeBridge(7, xRatio, yRatio);
            x = Width / 2 - size / 2;
            addBridge(x, y, 7, xRatio, yRatio, viewportOffset);
            makePlatformBlock(_bridges.Last(p => p.Texture == _bridgeStartAsset), _bridges.Last(p => p.Texture == _bridgeEndAsset), viewportOffset);
            size = sizeBridge(5, xRatio, yRatio);
            x = endPlatform.Position.X - endPlatform.Width - size;
            addBridge(x, y, 5, xRatio, yRatio, viewportOffset);
            makePlatformBlock(_bridges.Last(p => p.Texture == _bridgeStartAsset), _bridges.Last(p => p.Texture == _bridgeEndAsset), viewportOffset);
        }

        private void addBridge(float x, float y, int platfroms, float xRatio, float yRatio, Vector2 viewportOffset)
        {
            _bridges.Add(new Texture(_bridgeStartAsset)
            {
                Offset = viewportOffset,
                Position = new Vector2(x, y),
                Size = new Vector2(_bridgeStartAsset.CropWidth * xRatio, _bridgeStartAsset.CropHeight * yRatio)
            });
            x += _bridgeStartAsset.CropWidth * xRatio - 1;
            for (int i = 0; i < platfroms; i++)
            {
                _bridges.Add(new Texture(_bridgeMiddleAsset)
                {
                    Offset = viewportOffset,
                    Position = new Vector2(x, y - 1),
                    Size = new Vector2(_bridgeMiddleAsset.CropWidth * xRatio, _bridgeMiddleAsset.CropHeight * yRatio)
                });
                x += _bridgeMiddleAsset.CropWidth * xRatio - 1;
            }
            _bridges.Add(new Texture(_bridgeEndAsset)
            {
                Offset = viewportOffset,
                Position = new Vector2(x, y),
                Size = new Vector2(_bridgeEndAsset.CropWidth * xRatio, _bridgeEndAsset.CropHeight * yRatio)
            });
            x += _bridgeEndAsset.CropWidth * xRatio;
        }

        private float sizeBridge(int platfroms, float xRatio, float yRatio)
        {
            float x = 0;
            x += _bridgeStartAsset.CropWidth * xRatio;
            for (int i = 0; i < platfroms; i++)
                x += _bridgeMiddleAsset.CropWidth * xRatio;
            x += _bridgeEndAsset.CropWidth * xRatio;
            return x;
        }

        public override void Dispose()
        {
            _seaAsset.Dispose();
            _cloudsAsset.Dispose();
            _farGroundsAsset.Dispose();
            _skyAsset.Dispose();
            _tilesetAsset.Dispose();
        }

        public override void Draw()
        {
            foreach (var sky in _skies) sky.InvokeDraw();
            foreach (var sea in _seas) sea.InvokeDraw();
            _farGrounds.InvokeDraw();
            foreach (var cloud in _clouds) cloud.InvokeDraw();
            foreach (var platform in _platforms) platform.InvokeDraw();
            foreach (var bidge in _bridges) bidge.InvokeDraw();
        }

        public override void Tick(Vector2 camera, Vector2 viewportSize)
        {
            foreach (var cloud in _clouds)
            {
                cloud.Position += new Vector2(1f, 0);
                if (cloud.Position.X > _cloudWidthSpan) cloud.Position = new Vector2(-cloud.Width + (cloud.Position.X - _cloudWidthSpan), cloud.Position.Y);
            }
            _farGrounds.Position = new Vector2((camera + viewportSize - _farGrounds.Size).X, _farGrounds.Position.Y);
        }
    }
}