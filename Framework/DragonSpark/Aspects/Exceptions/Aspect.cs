using DragonSpark.Sources;
using JetBrains.Annotations;
using Polly;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Exceptions
{
	//[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.ExceptionHandling ), LinesOfCodeAvoided( 1 ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ParameterValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.EnhancedValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )
	]
	[AttributeUsage( AttributeTargets.Method ), UsedImplicitly]
	public sealed class Aspect : MethodInterceptionAspectBase
	{
		readonly static Func<object, Policy> Source = SourceCoercer<Policy>.Default.Get;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var policy = Source( args.Instance );
			if ( policy != null )
			{
				policy.Execute( args.Proceed );
			}
			else
			{
				args.Proceed();
			}
		}
	}
}