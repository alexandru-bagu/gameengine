using System.IO;

namespace GameEngine.Networking
{
    public class Deserializer : BinaryReader
    {
        private MemoryStream _stream;
        private bool _disposeStream;

        public Deserializer(byte[] buffer) : base(new MemoryStream(buffer))
        {
            _stream = (MemoryStream)BaseStream;
            _disposeStream = true;
        }

        ~Deserializer()
        {
            if (_disposeStream)
            {
                _disposeStream = false;
                _stream.Close();
                _stream.Dispose();
            }
        }
    }
}
