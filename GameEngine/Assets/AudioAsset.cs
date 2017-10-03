using GameEngine.Audio;
using OpenTK.Audio.OpenAL;
using System;

namespace GameEngine.Assets
{
    public class AudioAsset : Asset
    {
        private bool _disposed = false;

        public int Length
        {
            get
            {
                int lenght;
                AL.GetBuffer(Id, ALGetBufferi.Size, out lenght);
                ALErrorStack.Check();
                return lenght;
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                AL.DeleteBuffer(Id);
                ALErrorStack.Check();
            }
        }

        public static AudioAsset LoadRelativePath(params string[] pathPieces)
        {
            return LoadAbsolutePath(AssetManager.RelativeAudio(pathPieces));
        }

        public static AudioAsset LoadAbsolutePath(string path)
        {
            if (!AssetManager.AssetExists(path)) throw new Exception($"Audio {path} is missing.");
            return LoadFromWave(WaveFileReader.FromFile(path));
        }

        public static AudioAsset LoadFromWave(WaveData data)
        {
            var id = AL.GenBuffer();
            ALErrorStack.Check();
            AL.BufferData(id, data.GetSoundFormat(), data.Sound, data.Length, data.SampleRate);
            ALErrorStack.Check();
            return new AudioAsset() { Id = id };
        }
    }
}
