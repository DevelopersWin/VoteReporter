using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class SpecificationAdapter<T> : InvocationBase<object, bool>, ISpecification
	{
		readonly ISpecification<T> specification;

		public SpecificationAdapter( ISpecification<T> specification )
		{
			this.specification = specification;
		}

		public override bool Invoke( object parameter ) => specification.IsSatisfiedBy( parameter.AsValid<T>() );
	}
}