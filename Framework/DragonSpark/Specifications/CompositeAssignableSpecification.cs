using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Specifications
{
	public class CompositeAssignableSpecification : SpecificationWithContextBase<ImmutableArray<Type>, Type>
	{
		public CompositeAssignableSpecification( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public CompositeAssignableSpecification( ImmutableArray<Type> context ) : base( context ) {}
		

		public override bool IsSatisfiedBy( Type parameter )
		{
			foreach ( var adapter in Context )
			{
				if ( adapter.IsAssignableFrom( parameter ) )
				{
					return true;
				}
			}
			return false;
		}
	}
}