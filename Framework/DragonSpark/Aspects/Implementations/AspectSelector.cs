using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Implementations
{
	public class AspectSelector<T> : TypeAspectSelector<T> where T : IAspect
	{
		public AspectSelector( Type declaringType, params Type[] implementedTypes ) : base( 
			TypeAssignableSpecification.Defaults.Get( declaringType )
				.And( new AllSpecification<Type>( implementedTypes.Select( type => TypeAssignableSpecification.Defaults.Get( type ).Inverse() ).Fixed() ) )
				.ToDelegate()
		) {}
	}
}