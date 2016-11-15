using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Specifications
{
	public class CompositeInstanceSpecification : SpecificationWithContextBase<ImmutableArray<Type>, object>
	{
		public CompositeInstanceSpecification( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public CompositeInstanceSpecification( ImmutableArray<Type> context ) : base( context ) {}
		
		public override bool IsSatisfiedBy( object parameter )
		{
			foreach ( var adapter in Context )
			{
				if ( adapter.IsInstanceOfType( parameter ) )
				{
					return true;
				}
			}
			return false;
		}
	}
}