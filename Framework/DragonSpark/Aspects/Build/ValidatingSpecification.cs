using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	sealed class ValidatingSpecification : AdapterAssignableSpecification
	{
		readonly ImmutableArray<Type> aspectTypes;
		readonly ImmutableArray<Type> types;

		public ValidatingSpecification( ImmutableArray<Type> aspectTypes, params Type[] types ) : base( types )
		{
			this.aspectTypes = aspectTypes;
			this.types = types.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter )
		{
			if ( !base.IsSatisfiedBy( parameter ) )
			{
				throw new InvalidOperationException( $"{parameter} does not implement any of the types required by aspect types {string.Join( ", ", aspectTypes.Select( t => t.FullName ) )}. {parameter} must implement one of the following expected interfaces: {string.Join( ", ", types.Select( t => t.FullName ) )}" );
			}
			return true;
		}
	}
}