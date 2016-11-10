using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
		]
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ApplyRelayAspectBase : InvocationAspectBase
	{
		protected ApplyRelayAspectBase() {}

		protected ApplyRelayAspectBase( IAspectBuildDefinition definition ) : base( definition.Get, definition ) {}
	}
}