using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects
{
	public sealed class SpecificationDefinition : Definition
	{
		public static SpecificationDefinition Default { get; } = new SpecificationDefinition();
		SpecificationDefinition() : this( new MethodStore( typeof(ISpecification<>), nameof(ISpecification<object>.IsSatisfiedBy) ) ) {}

		SpecificationDefinition( IMethodStore method ) : base( method.DeclaringType, method )
		{
			Method = method;
		}

		public IMethodStore Method { get; }
	}
}