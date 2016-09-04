using System;
using System.Collections.Immutable;
using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem.Generics;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Invocation
{
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Class )]
	public class ApplyPolicyAttribute : InstanceLevelAspect
	{
		readonly static IGenericMethodContext<Execute> Context = typeof(ApplyPolicyAttribute).Adapt().GenericCommandMethods[nameof(Apply)];

		readonly ImmutableArray<Type> policyTypes;
		public ApplyPolicyAttribute( params Type[] policyTypes )
		{
			this.policyTypes = policyTypes.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var decorator in policyTypes.SelectAssigned( Activator.Default.Get ) )
			{
				Context.Make( Instance.GetType() ).Invoke( decorator, Instance );
			}
		}

		static void Apply<T>( IPolicy<T> decorator, T instance ) => decorator.Apply( instance );
	}
}