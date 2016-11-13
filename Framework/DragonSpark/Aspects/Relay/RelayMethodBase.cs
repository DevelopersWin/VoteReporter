using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion ),
	 UsedImplicitly
	]
	public abstract class RelayMethodBase : AdapterMethodBase
	{
		protected RelayMethodBase( IAdapterInvocation invocation ) : base( invocation ) {}
	}
}