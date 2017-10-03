using System;
using System.IO;
using OpenTK;

namespace GameEngine.Networking
{
	public static class Extensions
	{
		public static void Write(this BinaryWriter writer, Vector2 vector)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
		}

		public static void Write(this BinaryWriter writer, Vector3 vector)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
			writer.Write(vector.Z);
		}

		public static void Write(this BinaryWriter writer, Vector4 vector)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
			writer.Write(vector.Z);
			writer.Write(vector.W);
		}

		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			return new Vector2(reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector4 ReadVector4(this BinaryReader reader)
		{
			return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
