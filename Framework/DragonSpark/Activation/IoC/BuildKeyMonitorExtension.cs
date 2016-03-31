using System.Collections.Generic;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;

namespace DragonSpark.Activation.IoC
{
	public class BuildKeyMonitorExtension : BuilderStrategy, IRequiresRecovery
	{
		Stack<NamedTypeBuildKey> Stack => new ThreadAmbientChain<NamedTypeBuildKey>().Item;

		public override void PreBuildUp( IBuilderContext context )
		{
			var item = Stack;
			if ( item.Contains( context.BuildKey ) )
			{
				context.BuildComplete = true;
				context.Existing = null;
			}
			else
			{
				context.RecoveryStack.Add( this );
				item.Push( context.BuildKey );
			}
		}

		public override void PostBuildUp( IBuilderContext context ) => Recover();

		public void Recover() => Stack.Pop();
	}
}