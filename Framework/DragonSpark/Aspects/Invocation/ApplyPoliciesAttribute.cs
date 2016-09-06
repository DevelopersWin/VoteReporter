using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects.Invocation
{
	[AttributeUsage( AttributeTargets.Class )]
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	// [MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[LinesOfCodeAvoided( 6 )]
	public class ApplyPoliciesAttribute : InstanceLevelAspect
	{
		// readonly static Action<Type> Command = ApplyPoliciesCommand.Default.Execute;

		readonly Action<Type> command;

		public ApplyPoliciesAttribute( params Type[] policyTypes ) : this( new ApplyPoliciesCommand( policyTypes.SelectAssigned( Defaults.PolicySource ) ).Execute ) {}

		public ApplyPoliciesAttribute( Action<Type> command )
		{
			this.command = command;
		}

		public override void RuntimeInitializeInstance() => command( Instance.GetType() );
	}
}