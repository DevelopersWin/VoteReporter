using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects
{
	public abstract class InstanceBasedAspectBase : TypeBasedAspectBase, IInstanceScopedAspect
	{
		readonly Func<object, IAspect> factory;

		protected InstanceBasedAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( definition )
		{
			this.factory = factory;
		}

		protected InstanceBasedAspectBase() {}

		public object CreateInstance( AdviceArgs adviceArgs ) => factory( adviceArgs.Instance );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	public abstract class InvocationAspectBase : InstanceBasedAspectBase
	{
		/*readonly Invoke invoke;

		protected InvocationAspectBase( Invoke invoke )
		{
			this.invoke = invoke;
		}*/

		protected InvocationAspectBase() {}

		protected InvocationAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		/*public object Get( object parameter ) => invoke( parameter );*/
	}

	public delegate object Invoke( object parameter );

	public abstract class InvocationMethodAspectBase : MethodInterceptionAspectBase
	{
		readonly Func<object, IAdapter> source;

		protected InvocationMethodAspectBase( Func<object, IAdapter> source )
		{
			this.source = source;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var instance = args.Instance;
			var invocation = source( instance );
			if ( invocation != null )
			{
				args.ReturnValue = invocation.Get( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}
}