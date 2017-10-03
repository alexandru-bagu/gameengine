using System.IO;
using GameEngine.Networking;
using OpenTK;

namespace Demo2.Networking.Primitives
{
    public class PlayerState : ISerializable
    {
        public static int IdCounter = 0;

        public int Id;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool Floating, Dead;

        public PlayerState(Player _player)
        {
            Position = _player.Position;
            Velocity = _player.Velocity;
            Floating = _player.Floating;
            Dead = _player.Dead;
            Id = IdCounter++;
        }

        public PlayerState()
        {

        }

        public void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Position = reader.ReadVector2();
            Velocity = reader.ReadVector2();
            Floating = reader.ReadBoolean();
            Dead = reader.ReadBoolean();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Position);
            writer.Write(Velocity);
            writer.Write(Floating);
            writer.Write(Dead);
        }
    }
}
