using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Specifications
{
	public interface ISpecificationAware<in T>
	{
		ISpecification<T> Specification { get; }
	}
}