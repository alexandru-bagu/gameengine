using System;
using System.IO;

namespace GameEngine.Audio
{
    public class WaveFileReader
    {
        public static WaveData FromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                int inLen = data_signature.Length;
                data_signature = data_signature.Replace("\0", "");
                inLen -= data_signature.Length;
                data_signature += new string(reader.ReadChars(inLen));
                int data_chunk_size = reader.ReadInt32();
                while (data_signature != "data")
                {
                    reader.ReadBytes(data_chunk_size);

                    data_signature = new string(reader.ReadChars(4));
                    data_chunk_size = reader.ReadInt32();
                }
                return new WaveData(reader.ReadBytes(data_chunk_size), num_channels, bits_per_sample, sample_rate);
            }
        }
    }
}
