using GameEngine.Cryptography;
using GameEngine.Reflection;
using System;

namespace GameEngine.Networking.Reflection
{
	public class ProcessorReflection
	{
		private Type _processorType, _parameterType;
		private Hash128 _hash;
		private IProcessor _instance;
		private Func<ISerializable> _generator;

		public IProcessor Instance => getInstance();
		public Func<ISerializable> Generator => getGenerator();

		public Type ProcessorType => _processorType;
		public Type ParameterType => _parameterType;
		public Hash128 ProcessorHash => _hash;

		public ProcessorReflection(Hash128 hash, Type processorType, Type parameterType)
		{
			_processorType = processorType;
			_parameterType = parameterType;
			_hash = hash;
		}

		private IProcessor getInstance()
		{
			if (_instance == null)
				_instance = Constructor.Create<IProcessor>(_processorType)();
			return _instance;
		}

		private Func<ISerializable> getGenerator()
		{
			if (_generator == null)
				_generator = Constructor.Create<ISerializable>(_parameterType);
			return _generator;
		}
	}
}