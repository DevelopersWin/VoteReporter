using DragonSpark.Specifications;

namespace DragonSpark.Aspects
{
	public static class Defaults
	{
		public static IMethodLocator Specification { get; } = new MethodDefinition( typeof(ISpecification<>), nameof( ISpecification<object>.IsSatisfiedBy ) );
	}
}