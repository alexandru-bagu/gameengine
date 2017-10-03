using System;
using GameEngine.Assets;
using OpenTK.Audio.OpenAL;

namespace GameEngine.Audio
{
    public class AudioSource : Asset
    {
        private bool _disposed = false;
        private bool _looping;
        private AudioAsset _asset;

        public bool IsPlaying => State == ALSourceState.Playing;

        public AudioSource(int sourceId)
        {
            Id = sourceId;
        }

        public void Reset()
        {
            Stop();
            Volume = 1f;
        }

        public ALSourceState State
        {
            get
            {
                int state;
                AL.GetSource(Id, ALGetSourcei.SourceState, out state);
                ALErrorStack.Check();
                return (ALSourceState)state;
            }
        }
        
        public bool Loop
        {
            get { return _looping; }
            set
            {
                AL.Source(Id, ALSourceb.Looping, _looping = value);
                ALErrorStack.Check();
            }
        }

        public int Offset
        {
            get
            {
                int offset;
                AL.GetSource(Id, ALGetSourcei.ByteOffset, out offset);
                ALErrorStack.Check();
                return offset;
            }
        }
        
        public float Volume
        {
            get
            {
                float volume;
                AL.GetSource(Id, ALSourcef.Gain, out volume);
                ALErrorStack.Check();
                return volume;
            }
            set
            {
                AL.Source(Id, ALSourcef.Gain, value);
                ALErrorStack.Check();
            }
        }

        public void Play()
        {
            AL.SourcePlay(Id);
            ALErrorStack.Check();
        }

        public void Stop()
        {
            AL.SourceStop(Id);
            ALErrorStack.Check();
        }

        public void Pause()
        {
            AL.SourcePause(Id);
            ALErrorStack.Check();
        }

        public void Seek(int position)
        {
            AL.Source(Id, ALSourcei.ByteOffset, position);
            ALErrorStack.Check();
        }

        public void Seek(float position)
        {
            if (position >= 0 && position < 1)
            {
                if (_asset == null) throw new Exception("Provided audio buffer is null.");
                var total = _asset.Length;
                Seek((int)(total * position));
            }
            else
            {
                Seek((int)position);
            }
        }

        public void Rewind()
        {
            AL.SourceRewind(Id);
            ALErrorStack.Check();
        }

        public void SetBuffer(AudioAsset asset)
        {
            _asset = asset;
            AL.Source(Id, ALSourcei.Buffer, _asset.Id);
            ALErrorStack.Check();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                AL.DeleteSource(Id);
            }
        }
    }
}
