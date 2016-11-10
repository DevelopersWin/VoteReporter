using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion ),
	 UsedImplicitly
	]
	public abstract class RelayAspectBase : InvocationMethodAspectBase
	{
		protected RelayAspectBase( Func<object, IAdapter> source ) : base( source ) {}
	}
}