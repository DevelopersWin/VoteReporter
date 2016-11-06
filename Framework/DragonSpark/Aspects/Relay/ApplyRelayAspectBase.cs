using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
		]
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ApplyRelayAspectBase : InstanceLevelAspect, IAspectProvider
	{
		readonly ISupportDefinition definition;

		protected ApplyRelayAspectBase( ISupportDefinition definition )
		{
			this.definition = definition;
		}

		public override object CreateInstance( AdviceArgs adviceArgs ) => definition.Get( adviceArgs.Instance );
		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => definition.Get( (Type)targetElement );
		public override bool CompileTimeValidate( Type type ) => definition.IsSatisfiedBy( type );
	}
}