using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
		]
	public sealed class ApplyRelaysAttribute : TypeBasedAspectBase
	{
		public ApplyRelaysAttribute() : base( Definitions.Default ) {}
	}
}