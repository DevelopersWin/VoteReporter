using DragonSpark.Sources.Parameterized;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime.Specifications
{
	public class AnySpecification : AnySpecification<object>
	{
		public AnySpecification( params ISpecification<object>[] specifications ) : base( specifications ) {}
	}

	public class AnySpecification<T> : SpecificationBase<T>
	{
		readonly ImmutableArray<ISpecification<T>> specifications;

		public AnySpecification( params ISpecification<T>[] specifications ) : this( Defaults<T>.Coercer, specifications ) {}

		public AnySpecification( Coerce<T> coercer, params ISpecification<T>[] specifications ) : base( coercer )
		{
			this.specifications = specifications.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( [Optional]T parameter )
		{
			foreach ( var specification in specifications )
			{
				if ( specification.IsSatisfiedBy( parameter ) )
				{
					return true;
				}
			}
			return false;
		}
	}
}