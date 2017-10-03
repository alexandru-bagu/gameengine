using System;
using System.IO;
using GameEngine.Networking;

namespace Demo1.Networking
{
	public class IdentifierPrimitive : ISerializable
	{
		public string Name = "";
		public string IP = "";

		public void Deserialize(BinaryReader reader)
		{
			Name = reader.ReadString();
			IP = reader.ReadString();
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write(IP);
		}
	}
}
