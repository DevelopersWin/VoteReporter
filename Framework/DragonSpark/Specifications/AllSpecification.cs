using DragonSpark.Sources.Parameterized;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public class AllSpecification : AllSpecification<object>
	{
		public AllSpecification( params ISpecification<object>[] specifications ) : base( specifications ) {}
	}

	public class AllSpecification<T> : SpecificationBase<T>
	{
		readonly ImmutableArray<ISpecification<T>> specifications;

		public AllSpecification( params ISpecification<T>[] specifications ) : this( Defaults<T>.Coercer, specifications ) {}

		public AllSpecification( Coerce<T> coercer, params ISpecification<T>[] specifications ) : base( coercer )
		{
			this.specifications = specifications.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( [Optional]T parameter )
		{
			foreach ( var specification in specifications )
			{
				if ( !specification.IsSatisfiedBy( parameter ) )
				{
					return false;
				}
			}
			return true;
		}
	}
}