using DragonSpark.Activation.Location;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;

namespace DragonSpark.Composition
{
	sealed class IsValidTypeSpecification : AnySpecification<Type>
	{
		public IsValidTypeSpecification( ICollection<Type> types ) : base( new DelegatedSpecification<Type>( types.Contains ), DefaultServices.Default ) {}

		/*public override bool IsSatisfiedBy( Type parameter = null )
		{
			var temp = DefaultServiceProvider.Default.IsSatisfiedBy( parameter );
			var stop = parameter.Name.Contains( "NotKnown" );
			var result = base.IsSatisfiedBy( parameter );
			return result;
		}*/
	}
}