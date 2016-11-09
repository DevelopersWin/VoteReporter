using DragonSpark.Extensions;
using DragonSpark.Sources;
using JetBrains.Annotations;
using Polly;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using DragonSpark.Aspects.Build;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Exceptions
{
	[IntroduceInterface( typeof(IPolicySource) )]
	[ProvideAspectRole( StandardRoles.ExceptionHandling ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ParameterValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )
		]
	public sealed class ApplyExceptionPolicyAttribute : InstanceBasedAspectBase, IPolicySource
	{
		readonly Policy policy;

		public ApplyExceptionPolicyAttribute( Type policyType ) :  base( Factory.Default.Get( policyType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyExceptionPolicyAttribute( Policy policy )
		{
			this.policy = policy;
		}

		public Policy Get() => policy;
		object ISource.Get() => Get();

		sealed class Factory : TypedParameterAspectFactory<Policy, ApplyExceptionPolicyAttribute>
		{
			public static Factory Default { get; } = new Factory();
			Factory() : base( Activator.Default.Get<Policy> ) {}
		}
	}
}