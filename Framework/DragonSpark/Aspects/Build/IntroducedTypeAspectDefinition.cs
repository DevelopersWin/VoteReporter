using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedTypeAspectDefinition<T> : TypeAspectDefinition<T> where T : CompositionAspect, IAspect
	{
		public IntroducedTypeAspectDefinition( ITypeAware typeAware ) : base( TypeAssignableSpecification.Defaults.Get( typeAware.ReferencedType ).Coerce( AsTypeCoercer.Default ).Inverse() ) {}
	}
}