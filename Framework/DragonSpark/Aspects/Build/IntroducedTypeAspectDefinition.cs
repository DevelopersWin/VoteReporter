using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedTypeAspectDefinition<T> : TypeAspectDefinition<T> where T : CompositionAspect, IAspect
	{
		public IntroducedTypeAspectDefinition( ISpecification<TypeInfo> specification ) : base( specification.Inverse() ) {}
	}
}