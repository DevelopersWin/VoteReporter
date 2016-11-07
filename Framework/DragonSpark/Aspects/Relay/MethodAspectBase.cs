using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Relay
{

	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.EnhancedValidation ),
	 AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, KnownRoles.ValueConversion )
	]
	public abstract class MethodAspectBase : MethodInterceptionAspectBase
	{
		readonly Func<object, IInvocation> source;

		protected MethodAspectBase( Func<object, IInvocation> source )
		{
			this.source = source;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = source( args.Instance );
			if ( invocation != null )
			{
				args.ReturnValue = invocation.Invoke( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}
}