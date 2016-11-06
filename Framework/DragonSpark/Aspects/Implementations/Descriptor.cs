using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Linq;

namespace DragonSpark.Aspects.Implementations
{
	public class Descriptor<T> : TypeBasedAspectInstanceLocator<T>, IDescriptor where T : IAspect
	{
		public Descriptor( Type declaringType, params Type[] implementedTypes ) : base( TypeAssignableSpecification.Defaults.Get( declaringType ).And( new AllSpecification<Type>( implementedTypes.Select( type => TypeAssignableSpecification.Defaults.Get( type ).Inverse() ).Fixed() ) ) )
		{
			ReferencedType = declaringType;
		}

		public Type ReferencedType { get; }
	}
}