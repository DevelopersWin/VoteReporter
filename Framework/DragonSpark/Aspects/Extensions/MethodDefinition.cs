using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensions
{
	public sealed class MethodDefinition : FactoryCache<Type, MethodInfo>, IMethodSource
	{
		public MethodDefinition( Type declaringType, string methodName )
		{
			DeclaringType = declaringType;
			MethodName = methodName;
		}

		public Type DeclaringType { get; }

		public string MethodName { get; }

		protected override MethodInfo Create( Type parameter )
		{
			var mapping = parameter.Adapt().GetMappedMethods( DeclaringType ).Introduce( MethodName, tuple => tuple.Item1.InterfaceMethod.Name == tuple.Item2 ).Only();
			var result = mapping.MappedMethod?.AsDeclared().AccountForGenericDefinition();
			return result;
		}
	}
}