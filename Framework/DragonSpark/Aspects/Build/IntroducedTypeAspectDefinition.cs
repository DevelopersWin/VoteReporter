using System.Reflection;
using DragonSpark.Specifications;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedTypeAspectDefinition<T> : TypeAspectDefinition<T> where T : CompositionAspect, IAspect
	{
		public IntroducedTypeAspectDefinition( ISpecification<TypeInfo> specification ) : base( specification.Inverse() ) {}
	}
}