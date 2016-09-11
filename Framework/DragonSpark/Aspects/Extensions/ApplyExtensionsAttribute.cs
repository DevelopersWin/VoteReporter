using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;

namespace DragonSpark.Aspects.Extensions
{
	[AttributeUsage( AttributeTargets.Class )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	// [MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[LinesOfCodeAvoided( 6 )]
	public class ApplyExtensionsAttribute : InstanceLevelAspect
	{
		// readonly static Action<Type> Command = ApplyPoliciesCommand.Default.Execute;

		readonly ImmutableArray<IExtension> policies;

		public ApplyExtensionsAttribute( params Type[] policyTypes ) : this( policyTypes.SelectAssigned( Defaults.PolicySource ) ) {}

		public ApplyExtensionsAttribute( IEnumerable<IExtension> policies )
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