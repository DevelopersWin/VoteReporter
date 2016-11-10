using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class DefaultSpecificationImplementation<T> : DefaultSpecificationImplementationBase<T>
	{
		public DefaultSpecificationImplementation( ISpecificationAdapter specification ) : base( specification ) {}
	}

	public sealed class DefaultSpecificationImplementation : DefaultSpecificationImplementationBase<object>
	{
		public DefaultSpecificationImplementation( ISpecificationAdapter specification ) : base( specification ) {}
	}

	public abstract class DefaultSpecificationImplementationBase<T> : SpecificationBase<T>
	{
		readonly ISpecificationAdapter specification;

		protected DefaultSpecificationImplementationBase( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => (bool)specification.Get( parameter );
	}

	public sealed class DefaultParameterizedSourceImplementation : DelegatedParameterizedSource<object, object>
	{
		public DefaultParameterizedSourceImplementation( IParameterizedSourceAdapter adapter ) : base( adapter.Get ) {}
	}
}