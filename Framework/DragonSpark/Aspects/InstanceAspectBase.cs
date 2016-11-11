using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects
{
	public abstract class InstanceAspectBase : TypeBasedAspectBase, IInstanceScopedAspect
	{
		readonly Func<object, IAspect> factory;

		protected InstanceAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( definition )
		{
			this.factory = factory;
		}

		protected InstanceAspectBase() {}

		public object CreateInstance( AdviceArgs adviceArgs ) => factory( adviceArgs.Instance );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	public abstract class AdapterMethodBase : MethodInterceptionAspectBase
	{
		readonly Func<object, IAdapter> source;

		protected AdapterMethodBase( Func<object, IAdapter> source )
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