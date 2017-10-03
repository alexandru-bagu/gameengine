using GameEngine.Assets;
using GameEngine.Audio;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;

namespace GameEngine.Audio
{
    public class AudioMixer
    {
        private AudioContext _context;
        private List<AudioSource> _sources;

        internal AudioMixer()
        {
            _context = new AudioContext();
            _context.MakeCurrent();
            _sources = new List<AudioSource>();
        }

        ~AudioMixer()
        {
            foreach (var source in _sources)
                source.Dispose();
            _context.Dispose();
        }

        private AudioSource findUnusedSource()
        {
            foreach (var source in _sources)
                if (!source.IsPlaying)
                    return source;
            return null;
        }

        public AudioSource GenerateSource(AudioAsset asset)
        {
            AudioSource source = findUnusedSource();
            if (source == null)
            {
                int sourceId = AL.GenSource();
                ALErrorStack.Check();
                source = new AudioSource(sourceId);
                _sources.Add(source);
            }
            source.SetBuffer(asset);
            return source;
        }
    }
}
