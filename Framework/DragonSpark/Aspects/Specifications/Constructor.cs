using DragonSpark.Sources.Parameterized;
using System;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Specifications
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, ISpecification>
	{
		public static IParameterizedSource<Type, Func<object, ISpecification>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( GenericSpecificationTypeDefinition.Default.ReferencedType, typeof(SpecificationAdapter<>) ) {}
	}
}