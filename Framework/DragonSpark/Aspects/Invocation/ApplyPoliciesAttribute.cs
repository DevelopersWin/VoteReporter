using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Invocation
{
	[AttributeUsage( AttributeTargets.Class )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	// [MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[LinesOfCodeAvoided( 6 )]
	public class ApplyPoliciesAttribute : InstanceLevelAspect
	{
		// readonly static Action<Type> Command = ApplyPoliciesCommand.Default.Execute;

		readonly ImmutableArray<IPolicy> policies;

		public ApplyPoliciesAttribute( params Type[] policyTypes ) : this( policyTypes.SelectAssigned( Defaults.PolicySource ) ) {}

		public ApplyPoliciesAttribute( IEnumerable<IPolicy> policies )
		{
			this.policies = policies.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var policy in policies )
			{
				policy.Execute( Instance );
			}
		}
	}
}