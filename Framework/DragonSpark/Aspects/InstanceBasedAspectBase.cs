using DragonSpark.Aspects.Build;
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

	public interface IInvocationAspect : IInvocation {}

	public abstract class InvocationAspectBase : InstanceBasedAspectBase, IInvocationAspect
	{
		readonly IInvocation invocation;

		protected InvocationAspectBase( IInvocation invocation )
		{
			this.invocation = invocation;
		}

		protected InvocationAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		public object Invoke( object parameter ) => invocation.Invoke( parameter );
	}

	public abstract class InvocationMethodAspectBase : MethodInterceptionAspectBase
	{
		readonly Func<object, IInvocation> source;

		protected InvocationMethodAspectBase( Func<object, IInvocation> source )
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