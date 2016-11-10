using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Specifications
{
	[LinesOfCodeAvoided( 1 ), ProvideAspectRole( KnownRoles.ParameterValidation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class Aspect : InvocationMethodAspectBase
	{
		public Aspect() : base( SourceCoercer<ISpecificationAdapter>.Default.Get ) {}
	}
}