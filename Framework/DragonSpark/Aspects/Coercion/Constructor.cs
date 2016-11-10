using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, ICoercerAdapter>
	{
		public static IParameterizedSource<Type, Func<object, ICoercerAdapter>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( typeof(IParameterizedSource<,>), typeof(CoercerAdapter<,>) ) {}
	}
}