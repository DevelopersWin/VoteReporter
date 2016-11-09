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
	/*[IntroduceInterface( typeof(IRelay), OverrideAction = InterfaceOverrideAction.Ignore )]*/
	public abstract class RelayAspectBase : InvocationAspectBase
	{
		protected RelayAspectBase( IAspectBuildDefinition definition ) : base( definition.Get, definition ) {}
		
		protected RelayAspectBase( Invoke invoke ) : base( invoke ) {}
	}
}