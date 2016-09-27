using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public abstract class MethodAspectBase : AspectBase {}
}