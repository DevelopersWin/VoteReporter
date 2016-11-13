using DragonSpark.Aspects.Adapters;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Specifications
{
	[LinesOfCodeAvoided( 1 ), ProvideAspectRole( KnownRoles.ParameterValidation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class Aspect : AdapterMethodBase
	{
		public Aspect() : base( AdapterInvocation<ISpecificationAdapter>.Default ) {}
	}
}