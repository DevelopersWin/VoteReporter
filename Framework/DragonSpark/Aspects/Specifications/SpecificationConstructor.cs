using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Specifications
{
	class SpecificationConstructor : AdapterConstructorBase<ISpecification>
	{
		public static IParameterizedSource<Type, Func<object, ISpecification>> Default { get; } = new SpecificationConstructor().ToCache();
		SpecificationConstructor() : base( Defaults.Specification.DeclaringType, typeof(SpecificationAdapter<>) ) {}
	}
}