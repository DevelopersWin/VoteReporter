using DragonSpark.Specifications;

namespace DragonSpark.Aspects
{
	public static class Defaults
	{
		public static IMethodLocator Specification { get; } = new MethodLocator( typeof(ISpecification<>), nameof( ISpecification<object>.IsSatisfiedBy ) );
	}
}