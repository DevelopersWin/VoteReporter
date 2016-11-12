using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Implementations
{
	public class AspectDefinition<T> : TypeAspectDefinition<T> where T : IAspect
	{
		public AspectDefinition( Type declaringType, params Type[] implementedTypes ) : base( 
			TypeAssignableSpecification.Defaults.Get( declaringType )
				.And( new AllSpecification<Type>( implementedTypes.Select( type => TypeAssignableSpecification.Defaults.Get( type ).Inverse() ).Fixed() ) )
				.Coerce( AsTypeCoercer.Default )
		) {}
	}
}