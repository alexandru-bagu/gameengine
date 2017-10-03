using GameEngine.Cryptography;
using GameEngine.Networking.Reflection;
using System;
using GameEngine.Threading;

namespace GameEngine.Networking
{
	internal class Factory
	{
		public static bool Process(IThreadContext context, GenericNetworkClient client, byte[] buffer)
		{
			using (var deserializer = new Deserializer(buffer))
			{
				var processorTypeHash = new Hash128();
				processorTypeHash.Deserialize(deserializer);
                var pair = Reflector.GetPair(processorTypeHash);
				if (pair == null) return false;
				var instance = pair.Instance;
				var data = pair.Generator();
				data.Deserialize(deserializer);
				instance.Process(context, client, data);
				return true;
			}
		}

        public static byte[] Serialize<TProcessor, TData>(TData data)
            where TProcessor : Processor<TData>
            where TData : ISerializable, new()
        {
            return Serialize(typeof(TProcessor), data);
        }
        
		public static byte[] Serialize<TProcessor>(ISerializable data)
		{
			var type = typeof(TProcessor);
			return Serialize(type, data, true);
		}

		public static byte[] Serialize(Type type, ISerializable data)
		{
			return Serialize(type, data, true);
		}

		public static byte[] Serialize(Type type, ISerializable data, bool validateTypes)
		{
			if (data == null)
				throw new ArgumentNullException();
			var pair = Reflector.GetPair(type);
			if (pair == null)
				throw new ArgumentException("Type is not derived from Processor<>");
			var hash = pair.ProcessorHash;
			if (validateTypes && pair.ParameterType != data.GetType())
				throw new ArgumentException("Data type must match the processor data type.");

			using (var serializer = new Serializer())
			{
				hash.Serialize(serializer);
				data.Serialize(serializer);
				return serializer.ToArray();
			}
		}
	}
}
