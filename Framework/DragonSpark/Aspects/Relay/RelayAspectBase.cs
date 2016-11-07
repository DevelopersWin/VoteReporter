using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Relay
{
	public interface IRelay
	{
		object Invoke( object parameter );
	}

	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
		]
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	/*[IntroduceInterface( typeof(IRelay), OverrideAction = InterfaceOverrideAction.Ignore )]*/
	public abstract class RelayAspectBase : InstanceLevelAspect, IAspectProvider, IRelay
	{
		readonly IInvocation invocation;
		readonly IRelayMethodAspectBuildDefinition aspectBuildDefinition;

		protected RelayAspectBase( IRelayMethodAspectBuildDefinition aspectBuildDefinition )
		{
			this.aspectBuildDefinition = aspectBuildDefinition;
		}

		protected RelayAspectBase( IInvocation invocation )
		{
			this.invocation = invocation;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => aspectBuildDefinition.Get( (Type)targetElement );
		public override bool CompileTimeValidate( Type type ) => aspectBuildDefinition.IsSatisfiedBy( type );

		public override object CreateInstance( AdviceArgs adviceArgs ) => aspectBuildDefinition.Get( adviceArgs.Instance );
		public object Invoke( object parameter ) => invocation.Invoke( parameter );
	}
}