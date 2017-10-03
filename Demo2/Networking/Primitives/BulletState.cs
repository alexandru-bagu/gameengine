using GameEngine.Networking;
using System.IO;
using OpenTK;

namespace Demo2.Networking.Primitives
{
    public class BulletState : ISerializable
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Angle;

        public void Deserialize(BinaryReader reader)
        {
            Position = reader.ReadVector2();
            Velocity = reader.ReadVector2();
            Angle = reader.ReadSingle();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(Velocity);
            writer.Write(Angle);
        }
    }
}
