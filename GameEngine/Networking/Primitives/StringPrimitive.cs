using System;
using System.IO;

namespace GameEngine.Networking.Primitives
{
	public class StringPrimitive : ISerializable
	{
		public string Value { get; set; }

		public void Deserialize(BinaryReader reader)
		{
			Value = reader.ReadString();
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}
}
