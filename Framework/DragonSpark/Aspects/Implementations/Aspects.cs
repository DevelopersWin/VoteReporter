using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Implementations
{
	public class Aspects<T> : TypeAspects<T> where T : ITypeLevelAspect
	{
		public Aspects( ITypeDefinition implementedType, params ITypeDefinition[] notImplemented )
			: base( implementedType.And( new AllSpecification<TypeInfo>( notImplemented.Select( definition => definition.Inverse() ).Fixed() ) ) ) {}
	}
}