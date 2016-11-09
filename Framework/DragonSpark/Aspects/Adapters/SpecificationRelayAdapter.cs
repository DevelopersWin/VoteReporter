using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class SpecificationRelayAdapter<T> : InvocationBase<T, bool>, ISpecificationRelay
	{
		readonly ISpecification<T> specification;
		public SpecificationRelayAdapter( ISpecification<T> specification )
		{
			this.specification = specification;
		}

		public override bool Invoke( T parameter ) => specification.IsSatisfiedBy( parameter.AsValid<T>() );
	}
}