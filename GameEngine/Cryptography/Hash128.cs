using GameEngine.Networking;
using System.IO;

namespace GameEngine.Cryptography
{
    public class Hash128 : ISerializable
    {
        private ulong _a, _b;

        public ulong A => _a;
        public ulong B => _b;

        public Hash128()
        {
            _a = 0;
            _b = 0;
        }

        public Hash128(ulong a, ulong b)
        {
            _a = a;
            _b = b;
        }

        public Hash128(byte[] bytes)
        {
            _a = bytes.GetUInt64(0);
            _b = bytes.GetUInt64(8);
        }

        public static bool operator ==(Hash128 hash1, Hash128 hash2)
        {
            return hash1.equality(hash2);
        }

        public static bool operator !=(Hash128 hash1, Hash128 hash2)
        {
            return !hash1.equality(hash2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Hash128)
            {
                var hash = (Hash128)obj;
                return equality(hash);
            }
            return base.Equals(obj);
        }

        private bool equality(Hash128 hash)
        {
            return hash._a == _a && hash._b == _b;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(_a);
            writer.Write(_b);
        }

        public void Deserialize(BinaryReader reader)
        {
            _a = reader.ReadUInt64();
            _b = reader.ReadUInt64();
        }
    }
}
