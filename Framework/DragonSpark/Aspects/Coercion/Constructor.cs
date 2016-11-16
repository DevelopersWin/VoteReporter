using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, ICoercerAdapter>
	{
		public static IParameterizedSource<Type, Func<object, ICoercerAdapter>> Default { get; } = new Constructor().ToCache();
		Constructor() : this( typeof(CoercerAdapter<,>) ) {}

		public Constructor( Type adapterType ) : base( ParameterizedSourceTypeDefinition.Default.ReferencedType, adapterType ) {}
	}
}