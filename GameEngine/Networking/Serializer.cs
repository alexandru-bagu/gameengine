using System.IO;

namespace GameEngine.Networking
{
    public class Serializer : BinaryWriter
    {
        private MemoryStream _stream;
        private bool _disposeStream;

		public Serializer(int capacity) : base(new MemoryStream(capacity))
		{
			_stream = (MemoryStream)base.BaseStream;
			_disposeStream = true;
		}

		public Serializer() : base(new MemoryStream())
        {
            _stream = (MemoryStream)base.BaseStream;
            _disposeStream = true;
        }

        public Serializer(MemoryStream stream)
        {
            _stream = stream;
            _disposeStream = false;
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        ~Serializer()
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
