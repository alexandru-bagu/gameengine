using OpenTK.Audio.OpenAL;
using System;

namespace GameEngine.Audio
{
    public class WaveData
    {
        public byte[] Sound { get; private set; }
        public int Channels { get; private set; }
        public int Bits { get; private set; }
        public int SampleRate { get; private set; }
        public int Length => Sound.Length;

        public WaveData(byte[] sound, int channels, int bits, int sampleRate)
        {
            Sound = sound;
            Channels = channels;
            Bits = bits;
            SampleRate = sampleRate;
        }

        public ALFormat GetSoundFormat()
        {
            switch (Channels)
            {
                case 1: return Bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return Bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}
