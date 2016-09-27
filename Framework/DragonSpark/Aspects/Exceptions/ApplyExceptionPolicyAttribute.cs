using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Polly;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Exceptions
{
	[IntroduceInterface( typeof(IPolicySource) )]
	[ProvideAspectRole( StandardRoles.ExceptionHandling ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ParameterValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.EnhancedValidation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )
		]
	public class ApplyExceptionPolicyAttribute : ApplyInstanceAspectBase, IPolicySource
	{
		readonly static Func<Type, Policy> Source = Activator.Default.Get<Policy>;

		readonly ISource<Policy> source;

		public ApplyExceptionPolicyAttribute( Type policyType ) : this( policyType, Source ) {}

		protected ApplyExceptionPolicyAttribute( Type policyType, Func<Type, Policy> source ) : this( source.Fixed( policyType ) ) {}

		ApplyExceptionPolicyAttribute( ISource<Policy> source ) : base( Support.Default )
		{
			this.source = source;
		}

		public Policy Get() => source.Get();
		object ISource.Get() => Get();
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.ExceptionHandling ), LinesOfCodeAvoided( 1 ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ParameterValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.EnhancedValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )
	]
	public abstract class AspectBase : MethodInterceptionAspect {}

	[AttributeUsage( AttributeTargets.Method )]
	public class AppliedAspect : AspectBase
	{
		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var source = args.Instance as IPolicySource;
			if ( source != null )
			{
				source.Get().Execute( args.Proceed );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	/*[AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor )]
	public class ApplyExceptionPolicyAspect : AspectBase
	{
		readonly static Func<Type, Policy> Source = Activator.Default.Get<Policy>;

		readonly ISource<Policy> source;

		public ApplyExceptionPolicyAspect( Type policyType ) : this( policyType, Source ) {}

		protected ApplyExceptionPolicyAspect( Type policyType, Func<Type, Policy> source ) : this( source.Fixed( policyType ) ) {}

		ApplyExceptionPolicyAspect( ISource<Policy> source )
		{
			this.source = source;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args ) => source.Get().Execute( args.Proceed );
	}*/

	sealed class Support : SupportDefinition<AppliedAspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( GenericCommandTypeDefinition.Default, ParameterizedSourceTypeDefinition.Default, GenericSpecificationTypeDefinition.Default ) {}
	}

	public interface IPolicySource : ISource<Policy> {}
}