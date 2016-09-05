using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public struct MethodDescriptor
	{
		readonly IParameterizedSource<Type, MethodInfo> methods;

		public MethodDescriptor( Type declaringType, string methodName )
		{
			DeclaringType = declaringType;
			MethodName = methodName;
			methods = new Cache( declaringType, methodName );
		}

		public Type DeclaringType { get; }

		public string MethodName { get; }

		public MethodInfo Find( Type type ) => methods.Get( type );

		sealed class Cache : FactoryCache<Type, MethodInfo>
		{
			readonly Type declaringType;
			readonly string methodName;

			public Cache( Type declaringType, string methodName )
			{
				this.declaringType = declaringType;
				this.methodName = methodName;
			}

			protected override MethodInfo Create( Type parameter ) => parameter.GetMethod( declaringType, methodName ).AsDeclared();
		}
	}
}