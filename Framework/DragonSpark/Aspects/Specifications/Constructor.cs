using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Aspects.Specifications
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, ISpecificationAdapter>
	{
		public static IParameterizedSource<Type, Func<object, ISpecificationAdapter>> Default { get; } = new Constructor().ToCache();
		Constructor() : this( typeof(SpecificationAdapter<>) ) {}

		public Constructor( Type adapterType ) : base( SpecificationTypeDefinition.Default.ReferencedType, adapterType ) {}
	}
}