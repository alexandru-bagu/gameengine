using System;
using System.IO;
using GameEngine.Networking;
using OpenTK;

namespace Demo1.Networking.Primitives
{
	public class StatePrimitive : ISerializable
	{
		public static uint Counter = 0;

		public bool Puck;
		public Vector2 Position, Velocity;
		public uint Id = Counter++;

		public void Deserialize(BinaryReader reader)
		{
			Puck = reader.ReadBoolean();
			Position = reader.ReadVector2();
			Velocity = reader.ReadVector2();
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Puck);
			writer.Write(Position);
			writer.Write(Velocity);
		}
	}
}
