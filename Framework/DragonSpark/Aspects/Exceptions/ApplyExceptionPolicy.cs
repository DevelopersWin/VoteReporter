using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using Polly;
using PostSharp.Aspects.Advices;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Exceptions
{
	[IntroduceInterface( typeof(IPolicySource) )]
	public sealed class ApplyExceptionPolicy : InstanceAspectBase, IPolicySource
	{
		readonly Policy policy;

		public ApplyExceptionPolicy( Type policyType ) :  base( Constructors.Default.Get( policyType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyExceptionPolicy( Policy policy )
		{
			this.policy = policy;
		}

		public Policy Get() => policy;
		// object ISource.Get() => Get();

		sealed class Constructors : TypedParameterConstructors<Policy, ApplyExceptionPolicy>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( Activator.Default.Get<Policy> ) {}
		}
	}
}