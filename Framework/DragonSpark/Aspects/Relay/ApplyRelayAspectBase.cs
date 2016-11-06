using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
		]
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ApplyRelayAspectBase : InstanceLevelAspect
	{
		readonly IParameterizedSource<IAspect> definition;

		protected ApplyRelayAspectBase( IParameterizedSource<IAspect> definition )
		{
			this.definition = definition;
		}

		public override object CreateInstance( AdviceArgs adviceArgs ) => definition.Get( adviceArgs.Instance );
	}
}