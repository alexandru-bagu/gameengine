using System.IO;
using GameEngine.Networking;

namespace Demo1.Networking.Primitives
{
	public class ScorePrimitive : ISerializable
	{
		public bool GoalGreen;

		public void Deserialize(BinaryReader reader)
		{
			GoalGreen = reader.ReadBoolean();
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(GoalGreen);
		}
	}
}
