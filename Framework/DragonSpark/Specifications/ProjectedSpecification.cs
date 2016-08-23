using System;

namespace DragonSpark.Specifications
{
	public class ProjectedSpecification<TOrigin, TDestination> : SpecificationBase<TDestination>
	{
		readonly Func<TOrigin, bool> @delegate;
		readonly Coerce<TOrigin> coerce;

		public ProjectedSpecification( ISpecification<TOrigin> inner, Func<TDestination, TOrigin> projection ) : this( inner.ToSpecificationDelegate(), projection ) {}

		public ProjectedSpecification( Func<TOrigin, bool> @delegate, Func<TDestination, TOrigin> projection ) : this( @delegate, new Projector<TDestination, TOrigin>( projection ).ToDelegate() ) {}

		ProjectedSpecification( Func<TOrigin, bool> @delegate, Coerce<TOrigin> coerce )
		{
			this.@delegate = @delegate;
			this.coerce = coerce;
		}

		public override bool IsSatisfiedBy( TDestination parameter ) => @delegate( coerce( parameter ) );
	}
}