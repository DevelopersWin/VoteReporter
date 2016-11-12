using System;
using DragonSpark.Aspects.Adapters;
using PostSharp.Aspects;

namespace DragonSpark.Aspects
{
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