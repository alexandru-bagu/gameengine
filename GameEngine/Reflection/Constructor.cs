using System;
using System.Linq;
using System.Linq.Expressions;

namespace GameEngine.Reflection
{
	public class Constructor
	{
		public static Func<ReturnType> Create<ReturnType>()
		{
			return Create<ReturnType>(typeof(ReturnType));
		}

		public static Func<ReturnType> Create<ReturnType>(Type returnType)
		{
			return (Func<ReturnType>)Create(returnType);
		}

		public static Func<ParameterType, ReturnType> Create<ReturnType, ParameterType>()
		{
			return Create<ReturnType, ParameterType>(typeof(ReturnType), typeof(ParameterType));
		}

		public static Func<ParameterType, ReturnType> Create<ReturnType, ParameterType>(Type returnType)
		{
			return Create<ReturnType, ParameterType>(returnType, typeof(ParameterType));
		}

		public static Func<ParameterType, ReturnType> Create<ReturnType, ParameterType>(Type returnType, Type parameterType)
		{
			return (Func<ParameterType, ReturnType>)Create(returnType, parameterType);
		}

		public static Func<ParameterType1, ParameterType2, ReturnType> Create<ReturnType, ParameterType1, ParameterType2>()
		{
			return Create<ReturnType, ParameterType1, ParameterType2>(typeof(ReturnType), typeof(ParameterType1), typeof(ParameterType2));
		}

		public static Func<ParameterType1, ParameterType2, ReturnType> Create<ReturnType, ParameterType1, ParameterType2>(Type returnType)
		{
			return Create<ReturnType, ParameterType1, ParameterType2>(returnType, typeof(ParameterType1), typeof(ParameterType2));
		}

		public static Func<ParameterType1, ParameterType2, ReturnType> Create<ReturnType, ParameterType1, ParameterType2>(Type returnType, Type parameterType1)
		{
			return Create<ReturnType, ParameterType1, ParameterType2>(returnType, parameterType1, typeof(ParameterType2));
		}

		public static Func<ParameterType1, ParameterType2, ReturnType> Create<ReturnType, ParameterType1, ParameterType2>(Type returnType, Type parameterType1, Type parameterType2)
		{
			return (Func<ParameterType1, ParameterType2, ReturnType>)Create(returnType, parameterType1, parameterType2);
		}

		public static Delegate Create(Type returnType, params Type[] parameterTypes)
		{
			var constructor = returnType.GetConstructor(parameterTypes);

			var @params = parameterTypes.Select(p => Expression.Parameter(p)).ToArray();
			var @new = Expression.New(constructor, @params);

			var baseType = typeof(Func<>);
				 if (parameterTypes.Length == 1) baseType = typeof(Func<,>);
			else if (parameterTypes.Length == 2) baseType = typeof(Func<,,>);
			else if (parameterTypes.Length == 3) baseType = typeof(Func<,,,>);
			else if (parameterTypes.Length == 4) baseType = typeof(Func<,,,,>);
			else if (parameterTypes.Length == 5) baseType = typeof(Func<,,,,,>);
			else if (parameterTypes.Length == 6) baseType = typeof(Func<,,,,,,>);
			else if (parameterTypes.Length == 7) baseType = typeof(Func<,,,,,,,>);
			else if (parameterTypes.Length == 8) baseType = typeof(Func<,,,,,,,,>);

			baseType = baseType.MakeGenericType(parameterTypes.Concat(new[] { returnType }).ToArray());
			var lambda = Expression.Lambda(baseType, @new, @params);
			return lambda.Compile();
		}
	}
}
