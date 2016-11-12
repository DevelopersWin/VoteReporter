using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Implementations
{
	public class AspectDefinition<T> : TypeAspectDefinition<T> where T : IAspect
	{
		public AspectDefinition( ITypeDefinition implementedType, params ITypeDefinition[] avoid )
			: base( implementedType.And( new AllSpecification<TypeInfo>( avoid.Select( definition => definition.Inverse() ).Fixed() ) ) ) {}
	}
}