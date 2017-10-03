using GameEngine.Cryptography;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameEngine.Networking.Reflection
{
    public static class Reflector
    {
        static List<ProcessorReflection> _pairs;
        static Murmur3 _hashAlgorithm;

        static Reflector()
        {
            _pairs = new List<ProcessorReflection>();
            _hashAlgorithm = new Murmur3();

            AppDomain.CurrentDomain.AssemblyLoad += (s, e) =>
            {
                scanAssembly(e.LoadedAssembly);
            };

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                scanAssembly(assembly);
            }
        }

		public static ProcessorReflection GetPair(Type type)
        {
            foreach (var pair in _pairs)
                if (pair.ProcessorType == type)
                    return pair;
            return null;
        }

		public static ProcessorReflection GetPair(Hash128 hash)
        {
            foreach (var pair in _pairs)
                if (pair.ProcessorHash == hash)
                    return pair;
            return null;
        }

		private static void scanAssembly(Assembly assembly)
		{
			Type processorBaseClass = typeof(Processor<>);

			foreach (Type type in assembly.GetTypes())
			{
				if (!type.IsAbstract && type.BaseType != null && hasBaseType(type, processorBaseClass.Name))
				{
					var hasDefaultConstructor = false;
					foreach (var constructor in type.GetConstructors())
						if (constructor.GetParameters().Length == 0)
							hasDefaultConstructor = true;

					if (hasDefaultConstructor)
					{
						var genericArgs = type.BaseType.GetGenericArguments();
						var param = genericArgs[0];
						storeHash(type, param);
					}
				}
			}
		}

		private static bool hasBaseType(Type type, string requiredType)
		{
			if (type == null)
				return false;
			if (type.BaseType != null)
				return type.BaseType.Name == requiredType || hasBaseType(type.BaseType, requiredType);
			return false;
		}

        private static void storeHash(Type type, Type secondaryType)
        {
			var hash = new Hash128(_hashAlgorithm.ComputeHash(type.FullName));
			if (GetPair(hash) != null) 
				throw new Exception("Multiple types with the same hash. Possibly because of a secondary one parameter constructor.");
			_pairs.Add(new ProcessorReflection(hash, type, secondaryType));
        }
    }
}
