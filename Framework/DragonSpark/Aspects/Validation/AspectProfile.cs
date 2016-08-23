using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	class AspectProfile : ParameterizedSourceBase<Type, MethodInfo>, IAspectProfile
	{
		readonly Type declaringType;
		readonly string methodName;
		readonly Func<MethodLocator.Parameter, MethodInfo> source;

		public AspectProfile( Type supportedType, string methodName ) : this( supportedType, supportedType, methodName ) {}
		public AspectProfile( Type supportedType, Type declaringType, string methodName ) : this( supportedType, declaringType, methodName, Defaults.Locator ) {}
		public AspectProfile( Type supportedType, Type declaringType, string methodName, Func<MethodLocator.Parameter, MethodInfo> source )
		{
			SupportedType = supportedType;
			this.declaringType = declaringType;
			this.methodName = methodName;
			this.source = source;
		}

		public Type SupportedType { get; }

		public override MethodInfo Get( Type parameter ) => source( new MethodLocator.Parameter( declaringType, methodName, parameter ) );
	}
}