using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Specifications
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, ISpecificationAdapter>
	{
		public static IParameterizedSource<Type, Func<object, ISpecificationAdapter>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( SpecificationTypeDefinition.Default.ReferencedType, typeof(SpecificationAdapter<>) ) {}
	}
}