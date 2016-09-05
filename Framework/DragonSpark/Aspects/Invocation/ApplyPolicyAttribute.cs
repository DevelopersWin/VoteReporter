using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Immutable;
using System.Linq;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Invocation
{
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Class )]
	public class ApplyPolicyAttribute : InstanceLevelAspect
	{
		readonly static Func<Type, IPolicy> Create = Activator.Default.Get<IPolicy>;

		readonly ImmutableArray<Type> policyTypes;

		public ApplyPolicyAttribute( params Type[] policyTypes )
		{
			this.policyTypes = policyTypes.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var policy in policyTypes.SelectAssigned( Create ).ToArray() )
			{
				policy.Apply( Instance );
			}
		}
	}
}